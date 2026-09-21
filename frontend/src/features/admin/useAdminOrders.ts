import { useQuery } from '@tanstack/react-query'
import { adminOrdersApi } from '@/api/orders'
import type { OrderStatus } from '@/types'

export function useAdminOrders(
  params: { page?: number; pageSize?: number; status?: OrderStatus },
  options: { refetchInterval?: number } = {},
) {
  return useQuery({
    queryKey: ['admin', 'orders', params],
    queryFn: () => adminOrdersApi.list(params),
    placeholderData: (previousData) => previousData,
    refetchInterval: options.refetchInterval,
  })
}
