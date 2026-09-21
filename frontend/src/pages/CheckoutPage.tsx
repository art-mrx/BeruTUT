import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { useCart } from '@/features/cart/useCart'
import { useCreateOrder } from '@/features/orders/useCreateOrder'
import { checkoutSchema, type CheckoutFormValues } from '@/features/orders/schemas'
import { getApiErrorMessage } from '@/lib/apiError'
import { formatPrice } from '@/lib/format'

export function CheckoutPage() {
  const navigate = useNavigate()
  const { data: cart, isLoading } = useCart()
  const createOrder = useCreateOrder()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CheckoutFormValues>({ resolver: zodResolver(checkoutSchema) })

  function onSubmit(values: CheckoutFormValues) {
    createOrder.mutate(values, {
      onSuccess: (order) => navigate(`/orders/${order.id}`, { replace: true }),
    })
  }

  if (isLoading) {
    return <p className="text-gray-500">Загрузка…</p>
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div>
        <h1 className="mb-4 text-2xl font-semibold text-gray-900">Оформление заказа</h1>
        <p className="text-gray-500">Корзина пуста — нечего оформлять.</p>
        <Link to="/" className="mt-2 inline-block text-blue-600 hover:underline">
          В каталог
        </Link>
      </div>
    )
  }

  return (
    <div className="grid gap-8 md:grid-cols-2">
      <div>
        <h1 className="mb-4 text-2xl font-semibold text-gray-900">Оформление заказа</h1>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-gray-700">Телефон для связи</span>
            <input
              type="tel"
              autoComplete="tel"
              placeholder="+7 900 000-00-00"
              {...register('contactPhone')}
              className="rounded border border-gray-300 px-3 py-2"
            />
            {errors.contactPhone && <span className="text-sm text-red-600">{errors.contactPhone.message}</span>}
            <span className="text-xs text-gray-500">Мы позвоним вам, чтобы подтвердить заказ.</span>
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-gray-700">Адрес доставки</span>
            <textarea
              {...register('shippingAddress')}
              rows={3}
              className="rounded border border-gray-300 px-3 py-2"
            />
            {errors.shippingAddress && (
              <span className="text-sm text-red-600">{errors.shippingAddress.message}</span>
            )}
          </label>

          {createOrder.isError && (
            <p className="text-sm text-red-600">{getApiErrorMessage(createOrder.error, 'Не удалось оформить заказ.')}</p>
          )}

          <button
            type="submit"
            disabled={createOrder.isPending}
            className="rounded bg-gray-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {createOrder.isPending ? 'Оформляем…' : 'Подтвердить заказ'}
          </button>
        </form>
      </div>

      <div>
        <h2 className="mb-4 text-lg font-semibold text-gray-900">Ваш заказ</h2>
        <div className="rounded-lg border border-gray-200 bg-white p-4">
          {cart.items.map((item) => (
            <div key={item.id} className="flex justify-between py-1 text-sm">
              <span className="text-gray-700">
                {item.productName} × {item.quantity}
              </span>
              <span className="text-gray-900">{formatPrice(item.lineTotal)}</span>
            </div>
          ))}
          <div className="mt-3 flex justify-between border-t border-gray-200 pt-3 font-semibold text-gray-900">
            <span>Итого</span>
            <span>{formatPrice(cart.totalAmount)}</span>
          </div>
        </div>
      </div>
    </div>
  )
}
