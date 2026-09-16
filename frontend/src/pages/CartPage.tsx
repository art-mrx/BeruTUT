import { Link } from 'react-router-dom'
import { CartItemRow } from '@/features/cart/CartItemRow'
import { useCart } from '@/features/cart/useCart'
import { useClearCart } from '@/features/cart/useCartMutations'
import { formatPrice } from '@/lib/format'

export function CartPage() {
  const { data: cart, isLoading, isError } = useCart()
  const clearCart = useClearCart()

  if (isLoading) {
    return <p className="text-gray-500">Загрузка…</p>
  }

  if (isError) {
    return <p className="text-red-600">Не удалось загрузить корзину.</p>
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div>
        <h1 className="mb-4 text-2xl font-semibold text-gray-900">Корзина</h1>
        <p className="text-gray-500">Корзина пуста.</p>
        <Link to="/" className="mt-2 inline-block text-blue-600 hover:underline">
          В каталог
        </Link>
      </div>
    )
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-semibold text-gray-900">Корзина</h1>

      <div className="rounded-lg border border-gray-200 bg-white px-4">
        {cart.items.map((item) => (
          <CartItemRow key={item.id} item={item} />
        ))}
      </div>

      <div className="mt-4 flex items-center justify-between">
        <button
          type="button"
          onClick={() => clearCart.mutate()}
          disabled={clearCart.isPending}
          className="text-sm text-gray-500 hover:text-red-600 disabled:opacity-40"
        >
          Очистить корзину
        </button>

        <div className="flex items-center gap-4">
          <span className="text-lg font-semibold text-gray-900">Итого: {formatPrice(cart.totalAmount)}</span>
          <Link
            to="/checkout"
            className="rounded bg-gray-900 px-5 py-2 text-sm font-medium text-white hover:bg-gray-800"
          >
            Оформить заказ
          </Link>
        </div>
      </div>
    </div>
  )
}
