import { useQuery } from '@tanstack/react-query'
import { adminOrdersApi, type AdminOrderListParams } from '@/api/orders'

export function useAdminOrders(
  params: AdminOrderListParams,
  options: { refetchInterval?: number; enabled?: boolean } = {},
) {
  return useQuery({
    queryKey: ['admin', 'orders', params],
    queryFn: () => adminOrdersApi.list(params),
    placeholderData: (previousData) => previousData,
    refetchInterval: options.refetchInterval,
    enabled: options.enabled ?? true,
  })
}
