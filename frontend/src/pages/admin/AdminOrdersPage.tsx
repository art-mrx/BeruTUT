import { useState } from 'react'
import { Pagination } from '@/components/Pagination'
import { useAdminOrders } from '@/features/admin/useAdminOrders'
import { useUpdateOrderStatus } from '@/features/admin/useUpdateOrderStatus'
import { ORDER_STATUS_LABELS } from '@/features/orders/statusLabels'
import { getApiErrorMessage } from '@/lib/apiError'
import { formatPrice } from '@/lib/format'
import type { OrderStatus } from '@/types'

const PAGE_SIZE = 10
const ALL_STATUSES = Object.keys(ORDER_STATUS_LABELS) as OrderStatus[]

export function AdminOrdersPage() {
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState<OrderStatus | ''>('')
  const [rowError, setRowError] = useState<string | null>(null)

  const { data, isLoading, isError } = useAdminOrders({ page, pageSize: PAGE_SIZE, status: status || undefined })
  const updateStatus = useUpdateOrderStatus()

  function handleStatusChange(orderId: string, newStatus: OrderStatus) {
    setRowError(null)
    updateStatus.mutate(
      { id: orderId, status: newStatus },
      { onError: (error) => setRowError(getApiErrorMessage(error, 'Не удалось изменить статус заказа.')) },
    )
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Заказы</h1>
        <label className="flex items-center gap-2 text-sm">
          <span className="text-gray-600">Статус</span>
          <select
            value={status}
            onChange={(e) => {
              setStatus(e.target.value as OrderStatus | '')
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
      </div>

      {rowError && <p className="mb-3 text-sm text-red-600">{rowError}</p>}

      {isLoading && <p className="text-gray-500">Загрузка…</p>}
      {isError && <p className="text-red-600">Не удалось загрузить заказы.</p>}

      {data && data.items.length === 0 && <p className="text-gray-500">Заказов не найдено.</p>}

      {data && data.items.length > 0 && (
        <>
          <div className="overflow-x-auto rounded-lg border border-gray-200">
          <table className="w-full min-w-[640px] bg-white text-sm">
            <thead className="bg-gray-50 text-left text-gray-600">
              <tr>
                <th className="px-4 py-2 font-medium">Дата</th>
                <th className="px-4 py-2 font-medium">Покупатель</th>
                <th className="px-4 py-2 font-medium">Позиций</th>
                <th className="px-4 py-2 font-medium">Сумма</th>
                <th className="px-4 py-2 font-medium">Статус</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((order) => (
                <tr key={order.id} className="border-t border-gray-100">
                  <td className="px-4 py-2 text-gray-900">{new Date(order.createdAt).toLocaleDateString('ru-RU')}</td>
                  <td className="px-4 py-2 text-gray-700">
                    <div>{order.customerFullName}</div>
                    <div className="text-xs text-gray-400">{order.customerEmail}</div>
                  </td>
                  <td className="px-4 py-2 text-gray-900">{order.items.length}</td>
                  <td className="px-4 py-2 text-gray-900">{formatPrice(order.totalAmount)}</td>
                  <td className="px-4 py-2">
                    <select
                      value={order.status}
                      onChange={(e) => handleStatusChange(order.id, e.target.value as OrderStatus)}
                      disabled={updateStatus.isPending}
                      className="rounded border border-gray-300 px-2 py-1 text-sm"
                    >
                      {ALL_STATUSES.map((s) => (
                        <option key={s} value={s}>
                          {ORDER_STATUS_LABELS[s]}
                        </option>
                      ))}
                    </select>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  )
}
