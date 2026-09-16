export interface CartItem {
  id: string
  productId: string
  productName: string
  imageUrl: string | null
  price: number
  quantity: number
  lineTotal: number
}

export interface Cart {
  id: string
  items: CartItem[]
  totalAmount: number
}
