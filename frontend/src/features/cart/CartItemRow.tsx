import { formatPrice } from '@/lib/format'
import { getApiErrorMessage } from '@/lib/apiError'
import { useRemoveCartItem, useUpdateCartItem } from '@/features/cart/useCartMutations'
import type { CartItem } from '@/types'

export function CartItemRow({ item }: { item: CartItem }) {
  const updateItem = useUpdateCartItem()
  const removeItem = useRemoveCartItem()

  function changeQuantity(delta: number) {
    const nextQuantity = item.quantity + delta
    if (nextQuantity < 1) return
    updateItem.mutate({ itemId: item.id, quantity: nextQuantity })
  }

  return (
    <div className="flex items-center gap-4 border-b border-gray-200 py-4 last:border-0">
      <div className="h-16 w-16 flex-shrink-0 overflow-hidden rounded bg-gray-100">
        {item.imageUrl ? (
          <img src={item.imageUrl} alt={item.productName} className="h-full w-full object-cover" />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-[10px] text-gray-400">Нет фото</div>
        )}
      </div>

      <div className="flex-1">
        <p className="font-medium text-gray-900">{item.productName}</p>
        <p className="text-sm text-gray-500">{formatPrice(item.price)}</p>
        {updateItem.isError && (
          <p className="mt-1 text-xs text-red-600">{getApiErrorMessage(updateItem.error, 'Не удалось изменить количество.')}</p>
        )}
      </div>

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => changeQuantity(-1)}
          disabled={item.quantity <= 1 || updateItem.isPending}
          className="h-7 w-7 rounded border border-gray-300 text-sm disabled:cursor-not-allowed disabled:opacity-40"
        >
          −
        </button>
        <span className="w-6 text-center text-sm">{item.quantity}</span>
        <button
          type="button"
          onClick={() => changeQuantity(1)}
          disabled={updateItem.isPending}
          className="h-7 w-7 rounded border border-gray-300 text-sm disabled:cursor-not-allowed disabled:opacity-40"
        >
          +
        </button>
      </div>

      <p className="w-20 text-right font-medium text-gray-900">{formatPrice(item.lineTotal)}</p>

      <button
        type="button"
        onClick={() => removeItem.mutate(item.id)}
        disabled={removeItem.isPending}
        className="text-sm text-gray-400 hover:text-red-600 disabled:opacity-40"
      >
        Удалить
      </button>
    </div>
  )
}
