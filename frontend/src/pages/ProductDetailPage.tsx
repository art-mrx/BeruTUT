import { useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { useProduct } from '@/features/catalog/useProduct'
import { useAddCartItem } from '@/features/cart/useCartMutations'
import { useAuthStore } from '@/features/auth/authStore'
import { getApiErrorMessage } from '@/lib/apiError'
import { formatPrice } from '@/lib/format'

export function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>()
  const location = useLocation()
  const { data: product, isLoading, isError } = useProduct(slug)
  const [activeImage, setActiveImage] = useState(0)
  const [quantity, setQuantity] = useState(1)
  const isAuthenticated = useAuthStore((state) => Boolean(state.user))
  const addCartItem = useAddCartItem()

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

        {product.stockQuantity > 0 && (
          <div className="mt-6">
            {isAuthenticated ? (
              <>
                <div className="flex items-center gap-3">
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                      className="h-8 w-8 rounded border border-gray-300 text-sm"
                    >
                      −
                    </button>
                    <span className="w-6 text-center">{quantity}</span>
                    <button
                      type="button"
                      onClick={() => setQuantity((q) => Math.min(product.stockQuantity, q + 1))}
                      className="h-8 w-8 rounded border border-gray-300 text-sm"
                    >
                      +
                    </button>
                  </div>
                  <button
                    type="button"
                    disabled={addCartItem.isPending}
                    onClick={() => addCartItem.mutate({ productId: product.id, quantity })}
                    className="rounded bg-gray-900 px-5 py-2 text-sm font-medium text-white disabled:opacity-50"
                  >
                    {addCartItem.isPending ? 'Добавляем…' : 'В корзину'}
                  </button>
                </div>
                {addCartItem.isError && (
                  <p className="mt-2 text-sm text-red-600">
                    {getApiErrorMessage(addCartItem.error, 'Не удалось добавить товар в корзину.')}
                  </p>
                )}
                {addCartItem.isSuccess && <p className="mt-2 text-sm text-green-700">Добавлено в корзину.</p>}
              </>
            ) : (
              <p className="text-sm text-gray-600">
                <Link to="/login" state={{ from: location }} className="text-blue-600 hover:underline">
                  Войдите
                </Link>
                , чтобы добавить товар в корзину.
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
