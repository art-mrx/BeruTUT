import { apiClient } from '@/api/client'
import type { PaginatedList, Product, ProductListParams } from '@/types'

export interface ProductPayload {
  name: string
  slug: string
  description: string
  price: number
  stockQuantity: number
  categoryId: string
  isActive?: boolean
}

export const productsApi = {
  list: (params: ProductListParams = {}) =>
    apiClient.get<PaginatedList<Product>>('/products', { params }).then((r) => r.data),

  getById: (id: string) => apiClient.get<Product>(`/products/${id}`).then((r) => r.data),

  create: (payload: ProductPayload) => apiClient.post<Product>('/products', payload).then((r) => r.data),

  update: (id: string, payload: ProductPayload) =>
    apiClient.put<Product>(`/products/${id}`, payload).then((r) => r.data),

  remove: (id: string) => apiClient.delete<void>(`/products/${id}`).then((r) => r.data),

  uploadImage: (id: string, file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient
      .post<{ id: string; productId: string; url: string; sortOrder: number }>(`/products/${id}/images`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data)
  },
}
