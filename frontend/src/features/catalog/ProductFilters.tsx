import { useEffect, useState } from 'react'
import { useCategories } from '@/features/catalog/useCategories'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'
import type { ProductListParams, ProductSortBy } from '@/types'

export interface ProductFiltersValue {
  search: string
  categoryId: string
  sortBy: ProductSortBy | ''
  minPrice: string
  maxPrice: string
}

interface ProductFiltersProps {
  value: ProductFiltersValue
  onChange: (patch: Partial<ProductListParams>) => void
}

export function ProductFilters({ value, onChange }: ProductFiltersProps) {
  const { data: categories } = useCategories()

  const [search, setSearch] = useState(value.search)
  const [minPrice, setMinPrice] = useState(value.minPrice)
  const [maxPrice, setMaxPrice] = useState(value.maxPrice)

  const debouncedSearch = useDebouncedValue(search)
  const debouncedMinPrice = useDebouncedValue(minPrice)
  const debouncedMaxPrice = useDebouncedValue(maxPrice)

  useEffect(() => {
    if (debouncedSearch !== value.search) {
      onChange({ search: debouncedSearch || undefined })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch])

  useEffect(() => {
    if (debouncedMinPrice !== value.minPrice) {
      onChange({ minPrice: debouncedMinPrice ? Number(debouncedMinPrice) : undefined })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedMinPrice])

  useEffect(() => {
    if (debouncedMaxPrice !== value.maxPrice) {
      onChange({ maxPrice: debouncedMaxPrice ? Number(debouncedMaxPrice) : undefined })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedMaxPrice])

  return (
    <div className="mb-6 flex flex-wrap items-end gap-3 rounded-lg border border-gray-200 bg-white p-4">
      <label className="flex flex-col gap-1 text-sm">
        <span className="text-gray-600">Поиск</span>
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Название или описание"
          className="w-56 rounded border border-gray-300 px-2 py-1"
        />
      </label>

      <label className="flex flex-col gap-1 text-sm">
        <span className="text-gray-600">Категория</span>
        <select
          value={value.categoryId}
          onChange={(e) => onChange({ categoryId: e.target.value || undefined })}
          className="w-44 rounded border border-gray-300 px-2 py-1"
        >
          <option value="">Все категории</option>
          {categories?.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
      </label>

      <label className="flex flex-col gap-1 text-sm">
        <span className="text-gray-600">Цена от</span>
        <input
          type="number"
          min={0}
          value={minPrice}
          onChange={(e) => setMinPrice(e.target.value)}
          className="w-24 rounded border border-gray-300 px-2 py-1"
        />
      </label>

      <label className="flex flex-col gap-1 text-sm">
        <span className="text-gray-600">Цена до</span>
        <input
          type="number"
          min={0}
          value={maxPrice}
          onChange={(e) => setMaxPrice(e.target.value)}
          className="w-24 rounded border border-gray-300 px-2 py-1"
        />
      </label>

      <label className="flex flex-col gap-1 text-sm">
        <span className="text-gray-600">Сортировка</span>
        <select
          value={value.sortBy}
          onChange={(e) => onChange({ sortBy: (e.target.value || undefined) as ProductSortBy | undefined })}
          className="w-44 rounded border border-gray-300 px-2 py-1"
        >
          <option value="">По умолчанию</option>
          <option value="newest">Сначала новые</option>
          <option value="price_asc">Сначала дешевле</option>
          <option value="price_desc">Сначала дороже</option>
        </select>
      </label>
    </div>
  )
}
