import { ORDER_STATUS_BADGE_CLASSES, ORDER_STATUS_LABELS } from '@/features/orders/statusLabels'
import type { OrderStatus } from '@/types'

export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  return (
    <span className={`inline-block rounded-full px-3 py-1 text-xs font-medium ${ORDER_STATUS_BADGE_CLASSES[status]}`}>
      {ORDER_STATUS_LABELS[status]}
    </span>
  )
}
