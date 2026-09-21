import { apiClient } from '@/api/client'
import type { AdminOrder, Order, OrderStatus, PaginatedList } from '@/types'

export interface CreateOrderPayload {
  shippingAddress: string
  contactPhone: string
}

export const ordersApi = {
  create: (payload: CreateOrderPayload) => apiClient.post<Order>('/orders', payload).then((r) => r.data),

  list: (params: { page?: number; pageSize?: number } = {}) =>
    apiClient.get<PaginatedList<Order>>('/orders', { params }).then((r) => r.data),

  getById: (id: string) => apiClient.get<Order>(`/orders/${id}`).then((r) => r.data),
}

export interface AdminOrderListParams {
  page?: number
  pageSize?: number
  status?: OrderStatus
  /** ISO timestamps: createdFrom inclusive, createdTo exclusive. */
  createdFrom?: string
  createdTo?: string
  /** Fragment of the order number. */
  search?: string
  sortBy?: 'newest' | 'oldest'
}

export const adminOrdersApi = {
  list: (params: AdminOrderListParams = {}) =>
    apiClient.get<PaginatedList<AdminOrder>>('/admin/orders', { params }).then((r) => r.data),

  updateStatus: (id: string, status: OrderStatus) =>
    apiClient.put<AdminOrder>(`/admin/orders/${id}/status`, { status }).then((r) => r.data),
}
