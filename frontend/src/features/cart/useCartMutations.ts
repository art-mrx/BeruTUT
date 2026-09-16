import { useQueryClient, useMutation } from '@tanstack/react-query'
import { cartApi } from '@/api/cart'
import { cartQueryKey } from '@/features/cart/useCart'
import type { Cart } from '@/types'

export function useAddCartItem() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) =>
      cartApi.addItem(productId, quantity),
    onSuccess: (cart: Cart) => queryClient.setQueryData(cartQueryKey, cart),
  })
}

export function useUpdateCartItem() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ itemId, quantity }: { itemId: string; quantity: number }) =>
      cartApi.updateItem(itemId, quantity),
    onSuccess: (cart: Cart) => queryClient.setQueryData(cartQueryKey, cart),
  })
}

export function useRemoveCartItem() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (itemId: string) => cartApi.removeItem(itemId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: cartQueryKey }),
  })
}

export function useClearCart() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => cartApi.clear(),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: cartQueryKey }),
  })
}
