import { useState } from 'react'
import { OrderStatusBadge } from '@/components/OrderStatusBadge'
import { Pagination } from '@/components/Pagination'
import { useAdminOrders } from '@/features/admin/useAdminOrders'
import { useUpdateOrderStatus } from '@/features/admin/useUpdateOrderStatus'
import { ORDER_STATUS_ACTIONS, ORDER_STATUS_LABELS } from '@/features/orders/statusLabels'
import { getApiErrorMessage } from '@/lib/apiError'
import { formatPrice } from '@/lib/format'
import type { AdminOrder, OrderStatus } from '@/types'

const PAGE_SIZE = 10
const ALL_STATUSES = Object.keys(ORDER_STATUS_LABELS) as OrderStatus[]

const ACTION_BUTTON_CLASSES: Partial<Record<OrderStatus, string>> = {
  Approved: 'bg-green-600 text-white hover:bg-green-700',
  Cancelled: 'border border-red-300 text-red-700 hover:bg-red-50',
}
const DEFAULT_ACTION_CLASSES = 'border border-gray-300 text-gray-700 hover:bg-gray-50'

export function AdminOrdersPage({ mode }: { mode: 'pending' | 'all' }) {
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState<OrderStatus | ''>('')
  const [actionError, setActionError] = useState<string | null>(null)

  const status: OrderStatus | undefined = mode === 'pending' ? 'New' : statusFilter || undefined
  const { data, isLoading, isError } = useAdminOrders(
    { page, pageSize: PAGE_SIZE, status },
    { refetchInterval: mode === 'pending' ? 30_000 : undefined },
  )
  const updateStatus = useUpdateOrderStatus()

  function handleAction(order: AdminOrder, next: OrderStatus) {
    if (
      next === 'Cancelled' &&
      !window.confirm('Отклонить заказ? Товары из заказа вернутся в наличие, это действие нельзя отменить.')
    ) {
      return
    }
    setActionError(null)
    updateStatus.mutate(
      { id: order.id, status: next },
      { onError: (error) => setActionError(getApiErrorMessage(error, 'Не удалось изменить статус заказа.')) },
    )
  }

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-semibold text-gray-900">
          {mode === 'pending' ? 'Заказы, ожидающие подтверждения' : 'Все заказы'}
        </h1>
        {mode === 'all' && (
          <label className="flex items-center gap-2 text-sm">
            <span className="text-gray-600">Статус</span>
            <select
              value={statusFilter}
              onChange={(e) => {
                setStatusFilter(e.target.value as OrderStatus | '')
                setPage(1)
              }}
              className="rounded border border-gray-300 px-2 py-1"
            >
              <option value="">Все</option>
              {ALL_STATUSES.map((s) => (
                <option key={s} value={s}>
                  {ORDER_STATUS_LABELS[s]}
                </option>
              ))}
            </select>
          </label>
        )}
      </div>

      {mode === 'pending' && (
        <p className="mb-4 text-sm text-gray-500">
          Позвоните клиенту по указанному номеру. «Подтвердить» — товары остаются зарезервированными, «Отклонить» —
          товары возвращаются в наличие.
        </p>
      )}

      {actionError && <p className="mb-3 text-sm text-red-600">{actionError}</p>}
      {isLoading && <p className="text-gray-500">Загрузка…</p>}
      {isError && <p className="text-red-600">Не удалось загрузить заказы.</p>}

      {data && data.items.length === 0 && (
        <p className="text-gray-500">
          {mode === 'pending' ? 'Нет заказов, ожидающих подтверждения.' : 'Заказов не найдено.'}
        </p>
      )}

      {data && data.items.length > 0 && (
        <>
          <div className="flex flex-col gap-4">
            {data.items.map((order) => (
              <article key={order.id} className="rounded-lg border border-gray-200 bg-white p-4">
                <header className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <p className="font-medium text-gray-900">
                      Заказ от{' '}
                      {new Date(order.createdAt).toLocaleString('ru-RU', { dateStyle: 'short', timeStyle: 'short' })}
                    </p>
                    <p className="text-xs text-gray-400">№ {order.id.slice(0, 8)}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="font-semibold text-gray-900">{formatPrice(order.totalAmount)}</span>
                    <OrderStatusBadge status={order.status} />
                  </div>
                </header>

                <div className="mt-3 grid gap-3 text-sm sm:grid-cols-2">
                  <div>
                    <p className="text-xs uppercase tracking-wide text-gray-400">Клиент</p>
                    <p className="text-gray-900">{order.customerFullName}</p>
                    <a href={`mailto:${order.customerEmail}`} className="block text-blue-600 hover:underline">
                      {order.customerEmail}
                    </a>
                    {order.contactPhone ? (
                      <a href={`tel:${order.contactPhone}`} className="block font-medium text-blue-600 hover:underline">
                        {order.contactPhone}
                      </a>
                    ) : (
                      <span className="block text-gray-400">Телефон не указан</span>
                    )}
                  </div>
                  <div>
                    <p className="text-xs uppercase tracking-wide text-gray-400">Адрес доставки</p>
                    <p className="whitespace-pre-line text-gray-900">{order.shippingAddress}</p>
                  </div>
                </div>

                <ul className="mt-3 divide-y divide-gray-100 rounded border border-gray-100 text-sm">
                  {order.items.map((item) => (
                    <li key={item.id} className="flex justify-between gap-3 px-3 py-1.5">
                      <span className="text-gray-700">
                        {item.productName} × {item.quantity}
                      </span>
                      <span className="text-gray-900">{formatPrice(item.lineTotal)}</span>
                    </li>
                  ))}
                </ul>

                {order.allowedNextStatuses.length > 0 && (
                  <footer className="mt-3 flex flex-wrap gap-2">
                    {order.allowedNextStatuses.map((next) => (
                      <button
                        key={next}
                        type="button"
                        disabled={updateStatus.isPending}
                        onClick={() => handleAction(order, next)}
                        className={`rounded px-4 py-1.5 text-sm font-medium disabled:opacity-50 ${
                          ACTION_BUTTON_CLASSES[next] ?? DEFAULT_ACTION_CLASSES
                        }`}
                      >
                        {ORDER_STATUS_ACTIONS[next]}
                      </button>
                    ))}
                  </footer>
                )}
              </article>
            ))}
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  )
}
