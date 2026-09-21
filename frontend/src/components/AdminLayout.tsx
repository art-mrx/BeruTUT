import { NavLink, Outlet } from 'react-router-dom'
import { useAdminOrders } from '@/features/admin/useAdminOrders'

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `rounded px-3 py-1.5 text-sm font-medium ${isActive ? 'bg-gray-900 text-white' : 'text-gray-700 hover:bg-gray-100'}`

export function AdminLayout() {
  // Polled so a newly placed order shows up in the badge without a manual reload.
  const { data: pending } = useAdminOrders({ page: 1, pageSize: 1, status: 'New' }, { refetchInterval: 30_000 })
  const pendingCount = pending?.totalCount ?? 0

  return (
    <div>
      <nav className="mb-6 flex flex-wrap gap-2 border-b border-gray-200 pb-3">
        <NavLink to="/admin/orders/pending" className={linkClass}>
          Ожидают подтверждения
          {pendingCount > 0 && (
            <span className="ml-2 rounded-full bg-amber-500 px-2 py-0.5 text-xs font-semibold text-white">
              {pendingCount}
            </span>
          )}
        </NavLink>
        <NavLink to="/admin/orders" end className={linkClass}>
          Все заказы
        </NavLink>
        <NavLink to="/admin/products" className={linkClass}>
          Товары
        </NavLink>
        <NavLink to="/admin/categories" className={linkClass}>
          Категории
        </NavLink>
        <NavLink to="/admin/users" className={linkClass}>
          Пользователи
        </NavLink>
      </nav>
      <Outlet />
    </div>
  )
}
