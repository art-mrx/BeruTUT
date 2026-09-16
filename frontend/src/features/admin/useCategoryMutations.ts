import { useMutation, useQueryClient } from '@tanstack/react-query'
import { categoriesApi, type CategoryPayload } from '@/api/categories'

function useInvalidateCategories() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: ['categories'] })
}

export function useCreateCategory() {
  const invalidate = useInvalidateCategories()
  return useMutation({
    mutationFn: (payload: CategoryPayload) => categoriesApi.create(payload),
    onSuccess: invalidate,
  })
}

export function useUpdateCategory() {
  const invalidate = useInvalidateCategories()
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: CategoryPayload }) => categoriesApi.update(id, payload),
    onSuccess: invalidate,
  })
}

export function useDeleteCategory() {
  const invalidate = useInvalidateCategories()
  return useMutation({
    mutationFn: (id: string) => categoriesApi.remove(id),
    onSuccess: invalidate,
  })
}
