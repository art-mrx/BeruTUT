import { useQuery } from '@tanstack/react-query'
import { ordersApi } from '@/api/orders'

export function useOrder(id: string | undefined) {
  return useQuery({
    queryKey: ['orders', id],
    queryFn: () => ordersApi.getById(id!),
    enabled: Boolean(id),
  })
}
