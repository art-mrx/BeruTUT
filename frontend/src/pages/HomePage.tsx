import { useSearchParams } from 'react-router-dom'
import { Pagination } from '@/components/Pagination'
import { ProductCard } from '@/components/ProductCard'
import { ProductFilters, type ProductFiltersValue } from '@/features/catalog/ProductFilters'
import { useProducts } from '@/features/catalog/useProducts'
import type { ProductListParams, ProductSortBy } from '@/types'

const PAGE_SIZE = 12

export function HomePage() {
  const [searchParams, setSearchParams] = useSearchParams()

  const page = Number(searchParams.get('page') ?? '1')
  const search = searchParams.get('search') ?? ''
  const categoryId = searchParams.get('categoryId') ?? ''
  const sortBy = (searchParams.get('sortBy') ?? '') as ProductSortBy | ''
  const minPrice = searchParams.get('minPrice') ?? ''
  const maxPrice = searchParams.get('maxPrice') ?? ''

  const filtersValue: ProductFiltersValue = { search, categoryId, sortBy, minPrice, maxPrice }

  const params: ProductListParams = {
    page,
    pageSize: PAGE_SIZE,
    search: search || undefined,
    categoryId: categoryId || undefined,
    sortBy: sortBy || undefined,
    minPrice: minPrice ? Number(minPrice) : undefined,
    maxPrice: maxPrice ? Number(maxPrice) : undefined,
  }

  const { data, isLoading, isError } = useProducts(params)

  function handleFilterChange(patch: Partial<ProductListParams>) {
    const next = new URLSearchParams(searchParams)
    for (const [key, value] of Object.entries(patch)) {
      if (value === undefined || value === '') {
        next.delete(key)
      } else {
        next.set(key, String(value))
      }
    }
    next.set('page', '1')
    setSearchParams(next)
  }

  function handlePageChange(nextPage: number) {
    const next = new URLSearchParams(searchParams)
    next.set('page', String(nextPage))
    setSearchParams(next)
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-semibold text-gray-900">Каталог</h1>

      <ProductFilters value={filtersValue} onChange={handleFilterChange} />

      {isLoading && <p className="text-gray-500">Загрузка…</p>}
      {isError && <p className="text-red-600">Не удалось загрузить товары. Попробуйте обновить страницу.</p>}

      {data && data.items.length === 0 && <p className="text-gray-500">Товары не найдены.</p>}

      {data && data.items.length > 0 && (
        <>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {data.items.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} onPageChange={handlePageChange} />
        </>
      )}
    </div>
  )
}
