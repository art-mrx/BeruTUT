import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '@/features/auth/authStore'

export function AdminRoute() {
  const user = useAuthStore((state) => state.user)

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (user.role !== 'Admin') {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
