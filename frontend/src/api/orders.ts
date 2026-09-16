import { apiClient } from '@/api/client'
import type { AdminOrder, Order, OrderStatus, PaginatedList } from '@/types'

export const ordersApi = {
  create: (shippingAddress: string) => apiClient.post<Order>('/orders', { shippingAddress }).then((r) => r.data),

  list: (params: { page?: number; pageSize?: number } = {}) =>
    apiClient.get<PaginatedList<Order>>('/orders', { params }).then((r) => r.data),

  getById: (id: string) => apiClient.get<Order>(`/orders/${id}`).then((r) => r.data),
}

export const adminOrdersApi = {
  list: (params: { page?: number; pageSize?: number; status?: OrderStatus } = {}) =>
    apiClient.get<PaginatedList<AdminOrder>>('/admin/orders', { params }).then((r) => r.data),

  updateStatus: (id: string, status: OrderStatus) =>
    apiClient.put<AdminOrder>(`/admin/orders/${id}/status`, { status }).then((r) => r.data),
}
