import { useQuery } from '@tanstack/react-query'
import { ordersApi } from '@/api/orders'

export function useOrders(params: { page?: number; pageSize?: number } = {}) {
  return useQuery({
    queryKey: ['orders', params],
    queryFn: () => ordersApi.list(params),
    placeholderData: (previousData) => previousData,
  })
}
