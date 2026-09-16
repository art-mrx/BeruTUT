import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useRef, useState, type ChangeEvent } from 'react'
import { useForm } from 'react-hook-form'
import { Pagination } from '@/components/Pagination'
import { productSchema, type ProductFormValues } from '@/features/admin/schemas'
import { useAdminProducts } from '@/features/admin/useAdminProducts'
import {
  useCreateProduct,
  useDeleteProduct,
  useUpdateProduct,
  useUploadProductImage,
} from '@/features/admin/useProductMutations'
import { useCategories } from '@/features/catalog/useCategories'
import { getApiErrorMessage, getApiFieldErrors } from '@/lib/apiError'
import { formatPrice } from '@/lib/format'
import type { Product } from '@/types'

const PAGE_SIZE = 10

export function AdminProductsPage() {
  const [page, setPage] = useState(1)
  const { data: products, isLoading } = useAdminProducts({ page, pageSize: PAGE_SIZE })
  const { data: categories } = useCategories()

  const [editing, setEditing] = useState<Product | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [rowError, setRowError] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [uploadTargetId, setUploadTargetId] = useState<string | null>(null)

  const createProduct = useCreateProduct()
  const updateProduct = useUpdateProduct()
  const deleteProduct = useDeleteProduct()
  const uploadImage = useUploadProductImage()

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<ProductFormValues>({ resolver: zodResolver(productSchema) })

  useEffect(() => {
    if (editing) {
      reset({
        name: editing.name,
        slug: editing.slug,
        description: editing.description,
        price: editing.price,
        stockQuantity: editing.stockQuantity,
        categoryId: editing.categoryId,
      })
    } else {
      reset({ name: '', slug: '', description: '', price: 0, stockQuantity: 0, categoryId: '' })
    }
  }, [editing, reset])

  const mutation = editing ? updateProduct : createProduct

  function onSubmit(values: ProductFormValues) {
    const options = {
      onSuccess: () => {
        setShowForm(false)
        setEditing(null)
      },
      onError: (error: unknown) => {
        const fieldErrors = getApiFieldErrors(error)
        if (fieldErrors) {
          for (const [field, message] of Object.entries(fieldErrors)) {
            setError(field as keyof ProductFormValues, { message })
          }
        }
      },
    }

    if (editing) {
      updateProduct.mutate(
        {
          id: editing.id,
          payload: { ...values, description: values.description ?? '', isActive: editing.isActive },
        },
        options,
      )
    } else {
      createProduct.mutate({ ...values, description: values.description ?? '' }, options)
    }
  }

  function toggleActive(product: Product) {
    setRowError(null)
    const onError = (error: unknown) => setRowError(getApiErrorMessage(error, 'Не удалось изменить статус товара.'))

    if (product.isActive) {
      // DELETE only ever deactivates (see backend DeleteProductCommandHandler) — the
      // semantically correct call for hiding a product.
      deleteProduct.mutate(product.id, { onError })
    } else {
      // There's no dedicated "reactivate" endpoint, so flip isActive back via update.
      updateProduct.mutate(
        {
          id: product.id,
          payload: {
            name: product.name,
            slug: product.slug,
            description: product.description,
            price: product.price,
            stockQuantity: product.stockQuantity,
            categoryId: product.categoryId,
            isActive: true,
          },
        },
        { onError },
      )
    }
  }

  function openImagePicker(productId: string) {
    setUploadTargetId(productId)
    fileInputRef.current?.click()
  }

  function handleFileSelected(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    e.target.value = ''
    if (!file || !uploadTargetId) return
    setRowError(null)
    uploadImage.mutate(
      { id: uploadTargetId, file },
      { onError: (error) => setRowError(getApiErrorMessage(error, 'Не удалось загрузить изображение.')) },
    )
  }

  return (
    <div>
      <input ref={fileInputRef} type="file" accept="image/jpeg,image/png,image/webp" hidden onChange={handleFileSelected} />

      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Товары</h1>
        <button
          type="button"
          onClick={() => {
            setEditing(null)
            setShowForm((v) => !v)
          }}
          className="rounded bg-gray-900 px-4 py-2 text-sm font-medium text-white"
        >
          {showForm && !editing ? 'Отмена' : 'Добавить товар'}
        </button>
      </div>

      {(showForm || editing) && (
        <form onSubmit={handleSubmit(onSubmit)} className="mb-6 flex flex-wrap items-end gap-3 rounded-lg border border-gray-200 bg-white p-4">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-gray-600">Название</span>
            <input {...register('name')} className="w-48 rounded border border-gray-300 px-2 py-1" />
            {errors.name && <span className="text-xs text-red-600">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-gray-600">Slug</span>
            <input {...register('slug')} className="w-40 rounded border border-gray-300 px-2 py-1" />
            {errors.slug && <span className="text-xs text-red-600">{errors.slug.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-gray-600">Категория</span>
            <select {...register('categoryId')} className="w-40 rounded border border-gray-300 px-2 py-1">
              <option value="">Выберите…</option>
              {categories?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
            {errors.categoryId && <span className="text-xs text-red-600">{errors.categoryId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-gray-600">Цена</span>
            <input
              type="number"
              step="0.01"
              {...register('price', { valueAsNumber: true })}
              className="w-24 rounded border border-gray-300 px-2 py-1"
            />
            {errors.price && <span className="text-xs text-red-600">{errors.price.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-gray-600">Остаток</span>
            <input
              type="number"
              {...register('stockQuantity', { valueAsNumber: true })}
              className="w-20 rounded border border-gray-300 px-2 py-1"
            />
            {errors.stockQuantity && <span className="text-xs text-red-600">{errors.stockQuantity.message}</span>}
          </label>
          <label className="flex w-full flex-col gap-1 text-sm">
            <span className="text-gray-600">Описание</span>
            <textarea {...register('description')} rows={2} className="rounded border border-gray-300 px-2 py-1" />
          </label>
          <button
            type="submit"
            disabled={mutation.isPending}
            className="rounded bg-gray-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {editing ? 'Сохранить' : 'Создать'}
          </button>
          {mutation.isError && !getApiFieldErrors(mutation.error) && (
            <p className="w-full text-sm text-red-600">{getApiErrorMessage(mutation.error)}</p>
          )}
        </form>
      )}

      {rowError && <p className="mb-3 text-sm text-red-600">{rowError}</p>}

      {isLoading && <p className="text-gray-500">Загрузка…</p>}

      {products && (
        <>
          <table className="w-full overflow-hidden rounded-lg border border-gray-200 bg-white text-sm">
            <thead className="bg-gray-50 text-left text-gray-600">
              <tr>
                <th className="px-4 py-2 font-medium">Товар</th>
                <th className="px-4 py-2 font-medium">Категория</th>
                <th className="px-4 py-2 font-medium">Цена</th>
                <th className="px-4 py-2 font-medium">Остаток</th>
                <th className="px-4 py-2 font-medium">Статус</th>
                <th className="px-4 py-2" />
              </tr>
            </thead>
            <tbody>
              {products.items.map((product) => (
                <tr key={product.id} className="border-t border-gray-100">
                  <td className="flex items-center gap-2 px-4 py-2 text-gray-900">
                    <div className="h-10 w-10 flex-shrink-0 overflow-hidden rounded bg-gray-100">
                      {product.imageUrls[0] && (
                        <img src={product.imageUrls[0]} alt="" className="h-full w-full object-cover" />
                      )}
                    </div>
                    {product.name}
                  </td>
                  <td className="px-4 py-2 text-gray-500">{product.categoryName}</td>
                  <td className="px-4 py-2 text-gray-900">{formatPrice(product.price)}</td>
                  <td className="px-4 py-2 text-gray-900">{product.stockQuantity}</td>
                  <td className="px-4 py-2">
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                        product.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'
                      }`}
                    >
                      {product.isActive ? 'Активен' : 'Скрыт'}
                    </span>
                  </td>
                  <td className="whitespace-nowrap px-4 py-2 text-right">
                    <button
                      type="button"
                      onClick={() => openImagePicker(product.id)}
                      className="mr-3 text-blue-600 hover:underline"
                    >
                      Фото
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setEditing(product)
                        setShowForm(true)
                      }}
                      className="mr-3 text-blue-600 hover:underline"
                    >
                      Изменить
                    </button>
                    <button type="button" onClick={() => toggleActive(product)} className="text-red-600 hover:underline">
                      {product.isActive ? 'Скрыть' : 'Показать'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <Pagination page={products.page} totalPages={products.totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  )
}
