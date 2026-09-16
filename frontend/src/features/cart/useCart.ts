import { useQuery } from '@tanstack/react-query'
import { cartApi } from '@/api/cart'
import { useAuthStore } from '@/features/auth/authStore'

export const cartQueryKey = ['cart'] as const

export function useCart() {
  const isAuthenticated = useAuthStore((state) => Boolean(state.user))

  return useQuery({
    queryKey: cartQueryKey,
    queryFn: cartApi.get,
    enabled: isAuthenticated,
  })
}
