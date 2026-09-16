import { Link, useParams } from 'react-router-dom'
import { ORDER_STATUS_LABELS } from '@/features/orders/statusLabels'
import { useOrder } from '@/features/orders/useOrder'
import { formatPrice } from '@/lib/format'

export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: order, isLoading, isError } = useOrder(id)

  if (isLoading) {
    return <p className="text-gray-500">Загрузка…</p>
  }

  if (isError || !order) {
    return (
      <div>
        <p className="text-red-600">Заказ не найден.</p>
        <Link to="/orders" className="mt-2 inline-block text-blue-600 hover:underline">
          К списку заказов
        </Link>
      </div>
    )
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">
          Заказ от {new Date(order.createdAt).toLocaleDateString('ru-RU')}
        </h1>
        <span className="rounded-full bg-gray-100 px-3 py-1 text-sm font-medium text-gray-700">
          {ORDER_STATUS_LABELS[order.status]}
        </span>
      </div>

      <p className="mb-4 text-sm text-gray-600">Адрес доставки: {order.shippingAddress}</p>

      <div className="rounded-lg border border-gray-200 bg-white p-4">
        {order.items.map((item) => (
          <div key={item.id} className="flex justify-between border-b border-gray-100 py-2 text-sm last:border-0">
            <span className="text-gray-700">
              {item.productName} × {item.quantity}
            </span>
            <span className="text-gray-900">{formatPrice(item.lineTotal)}</span>
          </div>
        ))}
        <div className="mt-3 flex justify-between border-t border-gray-200 pt-3 font-semibold text-gray-900">
          <span>Итого</span>
          <span>{formatPrice(order.totalAmount)}</span>
        </div>
      </div>

      <Link to="/orders" className="mt-4 inline-block text-blue-600 hover:underline">
        К списку заказов
      </Link>
    </div>
  )
}
