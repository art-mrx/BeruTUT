import { apiClient } from '@/api/client'
import type { AdminUser, PaginatedList } from '@/types'

export interface AdminUserListParams {
  page?: number
  pageSize?: number
  /** Fragment of email, name or user number. */
  search?: string
  isBlocked?: boolean
  sortBy?: 'newest' | 'oldest'
}

export const adminUsersApi = {
  list: (params: AdminUserListParams = {}) =>
    apiClient.get<PaginatedList<AdminUser>>('/admin/users', { params }).then((r) => r.data),

  block: (id: string) => apiClient.post<AdminUser>(`/admin/users/${id}/block`).then((r) => r.data),

  unblock: (id: string) => apiClient.post<AdminUser>(`/admin/users/${id}/unblock`).then((r) => r.data),
}
