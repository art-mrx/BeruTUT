import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { formatPrice } from '@/lib/format'
import { useProduct } from '@/features/catalog/useProduct'

export function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>()
  const { data: product, isLoading, isError } = useProduct(slug)
  const [activeImage, setActiveImage] = useState(0)

  if (isLoading) {
    return <p className="text-gray-500">Загрузка…</p>
  }

  if (isError || !product) {
    return (
      <div>
        <p className="text-red-600">Товар не найден.</p>
        <Link to="/" className="mt-2 inline-block text-blue-600 hover:underline">
          Вернуться в каталог
        </Link>
      </div>
    )
  }

  const image = product.imageUrls[activeImage]

  return (
    <div className="grid gap-8 md:grid-cols-2">
      <div>
        <div className="aspect-square w-full overflow-hidden rounded-lg bg-gray-100">
          {image ? (
            <img src={image} alt={product.name} className="h-full w-full object-cover" />
          ) : (
            <div className="flex h-full w-full items-center justify-center text-sm text-gray-400">Нет фото</div>
          )}
        </div>
        {product.imageUrls.length > 1 && (
          <div className="mt-3 flex gap-2">
            {product.imageUrls.map((url, index) => (
              <button
                key={url}
                type="button"
                onClick={() => setActiveImage(index)}
                className={`h-16 w-16 overflow-hidden rounded border ${
                  index === activeImage ? 'border-gray-900' : 'border-gray-200'
                }`}
              >
                <img src={url} alt="" className="h-full w-full object-cover" />
              </button>
            ))}
          </div>
        )}
      </div>

      <div>
        <span className="text-sm text-gray-500">{product.categoryName}</span>
        <h1 className="mt-1 text-2xl font-semibold text-gray-900">{product.name}</h1>
        <p className="mt-4 text-3xl font-bold text-gray-900">{formatPrice(product.price)}</p>

        <p className="mt-2 text-sm">
          {product.stockQuantity > 0 ? (
            <span className="text-green-700">В наличии: {product.stockQuantity} шт.</span>
          ) : (
            <span className="text-red-600">Нет в наличии</span>
          )}
        </p>

        {product.description && <p className="mt-4 whitespace-pre-line text-gray-700">{product.description}</p>}

        <p className="mt-6 text-sm text-gray-400">Добавление в корзину появится на этапе 8.</p>
      </div>
    </div>
  )
}
