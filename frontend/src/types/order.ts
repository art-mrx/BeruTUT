export type OrderStatus = 'New' | 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled'

export interface OrderItem {
  id: string
  productId: string
  productName: string
  price: number
  quantity: number
  lineTotal: number
}

export interface Order {
  id: string
  status: OrderStatus
  totalAmount: number
  shippingAddress: string
  createdAt: string
  items: OrderItem[]
}

export interface AdminOrder extends Order {
  userId: string
  customerEmail: string
  customerFullName: string
}
