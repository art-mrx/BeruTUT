import { useQuery } from '@tanstack/react-query'
import { productsApi } from '@/api/products'

export function useProduct(idOrSlug: string | undefined) {
  return useQuery({
    queryKey: ['products', idOrSlug],
    queryFn: () => productsApi.getById(idOrSlug!),
    enabled: Boolean(idOrSlug),
  })
}
