import { useMutation, useQueryClient } from '@tanstack/react-query'
import { productsApi, type ProductPayload, type ProductUpdatePayload } from '@/api/products'

function useInvalidateProducts() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['admin', 'products'] })
    queryClient.invalidateQueries({ queryKey: ['products'] })
  }
}

export function useCreateProduct() {
  const invalidate = useInvalidateProducts()
  return useMutation({
    mutationFn: (payload: ProductPayload) => productsApi.create(payload),
    onSuccess: invalidate,
  })
}

export function useUpdateProduct() {
  const invalidate = useInvalidateProducts()
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: ProductUpdatePayload }) => productsApi.update(id, payload),
    onSuccess: invalidate,
  })
}

export function useDeleteProduct() {
  const invalidate = useInvalidateProducts()
  return useMutation({
    mutationFn: (id: string) => productsApi.remove(id),
    onSuccess: invalidate,
  })
}

export function useUploadProductImage() {
  const invalidate = useInvalidateProducts()
  return useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) => productsApi.uploadImage(id, file),
    onSuccess: invalidate,
  })
}
