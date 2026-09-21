import type { OrderStatus } from '@/types'

// "Cancelled" is the API/DB name; in the UI it is what the admin calls "rejected".
export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  New: 'Ожидает подтверждения',
  Approved: 'Одобрен',
  Processing: 'В обработке',
  Shipped: 'Отправлен',
  Delivered: 'Доставлен',
  Cancelled: 'Отклонён',
}

// Button text for moving an order *to* the given status.
export const ORDER_STATUS_ACTIONS: Partial<Record<OrderStatus, string>> = {
  Approved: 'Подтвердить',
  Processing: 'В обработку',
  Shipped: 'Отправлен',
  Delivered: 'Доставлен',
  Cancelled: 'Отклонить',
}

export const ORDER_STATUS_BADGE_CLASSES: Record<OrderStatus, string> = {
  New: 'bg-amber-100 text-amber-800',
  Approved: 'bg-green-100 text-green-800',
  Processing: 'bg-blue-100 text-blue-800',
  Shipped: 'bg-indigo-100 text-indigo-800',
  Delivered: 'bg-gray-200 text-gray-700',
  Cancelled: 'bg-red-100 text-red-800',
}
