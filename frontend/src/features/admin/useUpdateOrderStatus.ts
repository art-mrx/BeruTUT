import { useMutation, useQueryClient } from '@tanstack/react-query'
import { adminOrdersApi } from '@/api/orders'
import type { OrderStatus } from '@/types'

export function useUpdateOrderStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: OrderStatus }) => adminOrdersApi.updateStatus(id, status),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] }),
  })
}
