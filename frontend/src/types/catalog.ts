export interface Category {
  id: string
  name: string
  slug: string
  parentCategoryId: string | null
}

export interface Product {
  id: string
  name: string
  slug: string
  description: string
  price: number
  stockQuantity: number
  categoryId: string
  categoryName: string
  isActive: boolean
  createdAt: string
  updatedAt: string
  imageUrls: string[]
}

export type ProductSortBy = 'price_asc' | 'price_desc' | 'newest'

export interface ProductListParams {
  page?: number
  pageSize?: number
  categoryId?: string
  search?: string
  sortBy?: ProductSortBy
  minPrice?: number
  maxPrice?: number
}
