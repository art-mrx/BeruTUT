import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/features/auth/authStore'
import { isAccountBlockedError, markAccountBlocked } from '@/lib/accountBlocked'
import type { AuthResult } from '@/types'

export const apiClient = axios.create({ baseURL: '/api' })

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

interface RetriableConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
}

// The backend rotates refresh tokens on every use and treats a reused (already
// consumed) refresh token as a theft signal that revokes ALL of the user's
// sessions (see backend/docs DECISIONS.md). If two requests 401 at the same
// moment we must only call /auth/refresh once — this promise makes concurrent
// callers share the same in-flight refresh instead of racing each other.
let refreshPromise: Promise<string> | null = null

async function refreshAccessToken(): Promise<string> {
  const refreshToken = useAuthStore.getState().refreshToken
  if (!refreshToken) {
    throw new Error('No refresh token available')
  }

  const { data } = await axios.post<AuthResult>('/api/auth/refresh', { refreshToken })
  useAuthStore.getState().setAuth(data)
  return data.accessToken
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableConfig | undefined
    const isAuthEndpoint = originalRequest?.url?.includes('/auth/login') || originalRequest?.url?.includes('/auth/refresh')

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && !isAuthEndpoint) {
      originalRequest._retry = true

      try {
        refreshPromise ??= refreshAccessToken().finally(() => {
          refreshPromise = null
        })
        const newAccessToken = await refreshPromise

        originalRequest.headers.set('Authorization', `Bearer ${newAccessToken}`)
        return apiClient(originalRequest)
      } catch (refreshError) {
        if (isAccountBlockedError(refreshError)) {
          markAccountBlocked()
        }
        useAuthStore.getState().clearAuth()
        return Promise.reject(refreshError)
      }
    }

    return Promise.reject(error)
  },
)
