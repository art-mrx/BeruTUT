import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { AuthResult, User } from '@/types'

interface AuthState {
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  setAuth: (result: AuthResult) => void
  clearAuth: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      setAuth: (result) =>
        set({
          user: result.user,
          accessToken: result.accessToken,
          refreshToken: result.refreshToken,
        }),
      clearAuth: () => set({ user: null, accessToken: null, refreshToken: null }),
    }),
    { name: 'berutut-auth' },
  ),
)
