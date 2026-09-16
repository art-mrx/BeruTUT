import { Link, Outlet } from 'react-router-dom'
import { useAuthStore } from '@/features/auth/authStore'
import { useCart } from '@/features/cart/useCart'
import { useLogout } from '@/hooks/useLogout'

export function Layout() {
  const user = useAuthStore((state) => state.user)
  const logout = useLogout()
  const { data: cart } = useCart()
  const itemCount = cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="border-b border-gray-200 bg-white">
        <nav className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-x-4 gap-y-2 px-4 py-3">
          <Link to="/" className="text-lg font-semibold text-gray-900">
            BeruTUT
          </Link>
          <div className="flex flex-wrap items-center gap-x-4 gap-y-2 text-sm text-gray-700">
            <Link to="/cart">Корзина{itemCount > 0 && ` (${itemCount})`}</Link>
            {user ? (
              <>
                <Link to="/orders">Мои заказы</Link>
                {user.role === 'Admin' && <Link to="/admin/products">Админка</Link>}
                <span className="hidden text-gray-400 sm:inline">{user.email}</span>
                <button type="button" onClick={logout} className="text-gray-700 hover:text-gray-900">
                  Выйти
                </button>
              </>
            ) : (
              <>
                <Link to="/login">Войти</Link>
                <Link to="/register">Регистрация</Link>
              </>
            )}
          </div>
        </nav>
      </header>
      <main className="mx-auto max-w-6xl px-4 py-6">
        <Outlet />
      </main>
    </div>
  )
}
