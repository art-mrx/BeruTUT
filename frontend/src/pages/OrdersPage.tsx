import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Pagination } from '@/components/Pagination'
import { ORDER_STATUS_LABELS } from '@/features/orders/statusLabels'
import { useOrders } from '@/features/orders/useOrders'
import { formatPrice } from '@/lib/format'

export function OrdersPage() {
  const [page, setPage] = useState(1)
  const { data, isLoading, isError } = useOrders({ page, pageSize: 10 })

  if (isLoading) {
    return <p className="text-gray-500">Загрузка…</p>
  }

  if (isError) {
    return <p className="text-red-600">Не удалось загрузить заказы.</p>
  }

  if (!data || data.items.length === 0) {
    return (
      <div>
        <h1 className="mb-4 text-2xl font-semibold text-gray-900">Мои заказы</h1>
        <p className="text-gray-500">У вас пока нет заказов.</p>
        <Link to="/" className="mt-2 inline-block text-blue-600 hover:underline">
          В каталог
        </Link>
      </div>
    )
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-semibold text-gray-900">Мои заказы</h1>

      <div className="flex flex-col gap-3">
        {data.items.map((order) => (
          <Link
            key={order.id}
            to={`/orders/${order.id}`}
            className="flex items-center justify-between rounded-lg border border-gray-200 bg-white p-4 hover:shadow-sm"
          >
            <div>
              <p className="font-medium text-gray-900">Заказ от {new Date(order.createdAt).toLocaleDateString('ru-RU')}</p>
              <p className="text-sm text-gray-500">{order.items.length} позиций</p>
            </div>
            <div className="text-right">
              <p className="font-semibold text-gray-900">{formatPrice(order.totalAmount)}</p>
              <p className="text-sm text-gray-500">{ORDER_STATUS_LABELS[order.status]}</p>
            </div>
          </Link>
        ))}
      </div>

      <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
    </div>
  )
}
