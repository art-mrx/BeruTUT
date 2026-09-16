import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '@/api/orders'
import { cartQueryKey } from '@/features/cart/useCart'

export function useCreateOrder() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (shippingAddress: string) => ordersApi.create(shippingAddress),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: cartQueryKey })
      queryClient.invalidateQueries({ queryKey: ['orders'] })
    },
  })
}
