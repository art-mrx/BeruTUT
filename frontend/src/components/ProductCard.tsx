import { Link } from 'react-router-dom'
import { formatPrice } from '@/lib/format'
import type { Product } from '@/types'

export function ProductCard({ product }: { product: Product }) {
  const image = product.imageUrls[0]

  return (
    <Link
      to={`/products/${product.slug}`}
      className="group flex flex-col overflow-hidden rounded-lg border border-gray-200 bg-white transition hover:shadow-md"
    >
      <div className="aspect-square w-full bg-gray-100">
        {image ? (
          <img
            src={image}
            alt={product.name}
            className="h-full w-full object-cover transition group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-sm text-gray-400">Нет фото</div>
        )}
      </div>
      <div className="flex flex-1 flex-col gap-1 p-3">
        <span className="text-xs text-gray-500">{product.categoryName}</span>
        <h3 className="line-clamp-2 text-sm font-medium text-gray-900">{product.name}</h3>
        <div className="mt-auto flex items-center justify-between pt-2">
          <span className="text-base font-semibold text-gray-900">{formatPrice(product.price)}</span>
          {product.stockQuantity === 0 && <span className="text-xs text-red-600">Нет в наличии</span>}
        </div>
      </div>
    </Link>
  )
}
