import { apiClient } from '@/api/client'
import type { AuthResult, LoginPayload, RegisterPayload, User } from '@/types'

export const authApi = {
  register: (payload: RegisterPayload) => apiClient.post<AuthResult>('/auth/register', payload).then((r) => r.data),

  login: (payload: LoginPayload) => apiClient.post<AuthResult>('/auth/login', payload).then((r) => r.data),

  refresh: (refreshToken: string) =>
    apiClient.post<AuthResult>('/auth/refresh', { refreshToken }).then((r) => r.data),

  logout: (refreshToken: string) => apiClient.post<void>('/auth/logout', { refreshToken }).then((r) => r.data),

  me: () => apiClient.get<User>('/auth/me').then((r) => r.data),
}
