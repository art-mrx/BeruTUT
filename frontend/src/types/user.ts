import type { UserRole } from '@/types/auth'

export interface AdminUser {
  id: string
  email: string
  fullName: string
  role: UserRole
  createdAt: string
  isBlocked: boolean
  blockedAt: string | null
  ordersCount: number
}
