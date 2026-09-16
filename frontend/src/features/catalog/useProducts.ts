import { useQuery } from '@tanstack/react-query'
import { productsApi } from '@/api/products'
import type { ProductListParams } from '@/types'

export function useProducts(params: ProductListParams) {
  return useQuery({
    queryKey: ['products', params],
    queryFn: () => productsApi.list(params),
    placeholderData: (previousData) => previousData,
  })
}
