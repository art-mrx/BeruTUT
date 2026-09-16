import { useQuery } from '@tanstack/react-query'
import { adminProductsApi } from '@/api/products'
import type { ProductListParams } from '@/types'

export function useAdminProducts(params: ProductListParams) {
  return useQuery({
    queryKey: ['admin', 'products', params],
    queryFn: () => adminProductsApi.list(params),
    placeholderData: (previousData) => previousData,
  })
}
