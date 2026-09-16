import { Link, Outlet } from 'react-router-dom'
import { useAuthStore } from '@/features/auth/authStore'
import { useLogout } from '@/hooks/useLogout'

export function Layout() {
  const user = useAuthStore((state) => state.user)
  const logout = useLogout()

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="border-b border-gray-200 bg-white">
        <nav className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
          <Link to="/" className="text-lg font-semibold text-gray-900">
            BeruTUT
          </Link>
          <div className="flex items-center gap-4 text-sm text-gray-700">
            <Link to="/cart">Корзина</Link>
            {user ? (
              <>
                <Link to="/orders">Мои заказы</Link>
                {user.role === 'Admin' && <Link to="/admin/products">Админка</Link>}
                <span className="text-gray-400">{user.email}</span>
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
