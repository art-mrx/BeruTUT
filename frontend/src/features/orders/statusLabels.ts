import type { OrderStatus } from '@/types'

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  New: 'Новый',
  Processing: 'В обработке',
  Shipped: 'Отправлен',
  Delivered: 'Доставлен',
  Cancelled: 'Отменён',
}
