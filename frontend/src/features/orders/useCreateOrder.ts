import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ordersApi, type CreateOrderPayload } from '@/api/orders'
import { cartQueryKey } from '@/features/cart/useCart'

export function useCreateOrder() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateOrderPayload) => ordersApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: cartQueryKey })
      queryClient.invalidateQueries({ queryKey: ['orders'] })
    },
  })
}
