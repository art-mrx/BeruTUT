import { apiClient } from '@/api/client'
import type { Category } from '@/types'

export interface CategoryPayload {
  name: string
  slug: string
  parentCategoryId?: string | null
}

export const categoriesApi = {
  list: () => apiClient.get<Category[]>('/categories').then((r) => r.data),

  getById: (id: string) => apiClient.get<Category>(`/categories/${id}`).then((r) => r.data),

  create: (payload: CategoryPayload) => apiClient.post<Category>('/categories', payload).then((r) => r.data),

  update: (id: string, payload: CategoryPayload) =>
    apiClient.put<Category>(`/categories/${id}`, payload).then((r) => r.data),

  remove: (id: string) => apiClient.delete<void>(`/categories/${id}`).then((r) => r.data),
}
