import { useCallback } from 'react'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/features/auth/authStore'

export function useLogout() {
  const refreshToken = useAuthStore((state) => state.refreshToken)
  const clearAuth = useAuthStore((state) => state.clearAuth)

  return useCallback(() => {
    if (refreshToken) {
      // Best-effort: revoke the refresh token server-side. Local state is
      // cleared regardless of whether this call succeeds.
      void authApi.logout(refreshToken).catch(() => undefined)
    }
    clearAuth()
  }, [refreshToken, clearAuth])
}
