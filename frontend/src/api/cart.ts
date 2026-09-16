import { apiClient } from '@/api/client'
import type { Cart } from '@/types'

export const cartApi = {
  get: () => apiClient.get<Cart>('/cart').then((r) => r.data),

  addItem: (productId: string, quantity: number) =>
    apiClient.post<Cart>('/cart/items', { productId, quantity }).then((r) => r.data),

  updateItem: (itemId: string, quantity: number) =>
    apiClient.put<Cart>(`/cart/items/${itemId}`, { quantity }).then((r) => r.data),

  removeItem: (itemId: string) => apiClient.delete<void>(`/cart/items/${itemId}`).then((r) => r.data),

  clear: () => apiClient.delete<void>('/cart').then((r) => r.data),
}
