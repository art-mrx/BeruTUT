import { useMutation, useQueryClient } from '@tanstack/react-query'
import { adminOrdersApi } from '@/api/orders'
import type { OrderStatus } from '@/types'

export function useUpdateOrderStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: OrderStatus }) => adminOrdersApi.updateStatus(id, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] })
      // Rejecting an order returns its items to stock, so product stock shown elsewhere is stale now.
      queryClient.invalidateQueries({ queryKey: ['admin', 'products'] })
      queryClient.invalidateQueries({ queryKey: ['products'] })
    },
  })
}
