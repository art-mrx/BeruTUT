import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { categorySchema, type CategoryFormValues } from '@/features/admin/schemas'
import { useCreateCategory, useDeleteCategory, useUpdateCategory } from '@/features/admin/useCategoryMutations'
import { useCategories } from '@/features/catalog/useCategories'
import { getApiErrorMessage, getApiFieldErrors } from '@/lib/apiError'
import type { Category } from '@/types'

export function AdminCategoriesPage() {
  const { data: categories, isLoading } = useCategories()
  const [editing, setEditing] = useState<Category | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [deleteError, setDeleteError] = useState<string | null>(null)

  const createCategory = useCreateCategory()
  const updateCategory = useUpdateCategory()
  const deleteCategory = useDeleteCategory()

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<CategoryFormValues>({ resolver: zodResolver(categorySchema) })

  useEffect(() => {
    if (editing) {
      reset({ name: editing.name, slug: editing.slug, parentCategoryId: editing.parentCategoryId ?? '' })
    } else {
      reset({ name: '', slug: '', parentCategoryId: '' })
    }
  }, [editing, reset])

  const mutation = editing ? updateCategory : createCategory

  function onSubmit(values: CategoryFormValues) {
    const payload = { name: values.name, slug: values.slug, parentCategoryId: values.parentCategoryId || null }

    const options = {
      onSuccess: () => {
        setShowForm(false)
        setEditing(null)
      },
      onError: (error: unknown) => {
        const fieldErrors = getApiFieldErrors(error)
        if (fieldErrors) {
          for (const [field, message] of Object.entries(fieldErrors)) {
            setError(field as keyof CategoryFormValues, { message })
          }
        }
      },
    }

    if (editing) {
      updateCategory.mutate({ id: editing.id, payload }, options)
    } else {
      createCategory.mutate(payload, options)
    }
  }

  function handleDelete(category: Category) {
    if (!window.confirm(`Удалить категорию "${category.name}"?`)) return
    setDeleteError(null)
    deleteCategory.mutate(category.id, {
      onError: (error) => setDeleteError(getApiErrorMessage(error, 'Не удалось удалить категорию.')),
    })
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Категории</h1>
        <button
          type="button"
          onClick={() => {
            setEditing(null)
            setShowForm((v) => !v)
          }}
          className="rounded bg-gray-900 px-4 py-2 text-sm font-medium text-white"
        >
          {showForm && !editing ? 'Отмена' : 'Добавить категорию'}
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
            <input {...register('slug')} className="w-48 rounded border border-gray-300 px-2 py-1" />
            {errors.slug && <span className="text-xs text-red-600">{errors.slug.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-gray-600">Родительская категория</span>
            <select {...register('parentCategoryId')} className="w-48 rounded border border-gray-300 px-2 py-1">
              <option value="">Нет</option>
              {categories
                ?.filter((c) => c.id !== editing?.id)
                .map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
            </select>
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

      {deleteError && <p className="mb-3 text-sm text-red-600">{deleteError}</p>}

      {isLoading && <p className="text-gray-500">Загрузка…</p>}

      {categories && (
        <div className="overflow-x-auto rounded-lg border border-gray-200">
        <table className="w-full min-w-[480px] bg-white text-sm">
          <thead className="bg-gray-50 text-left text-gray-600">
            <tr>
              <th className="px-4 py-2 font-medium">Название</th>
              <th className="px-4 py-2 font-medium">Slug</th>
              <th className="px-4 py-2 font-medium">Родитель</th>
              <th className="px-4 py-2" />
            </tr>
          </thead>
          <tbody>
            {categories.map((category) => (
              <tr key={category.id} className="border-t border-gray-100">
                <td className="px-4 py-2 text-gray-900">{category.name}</td>
                <td className="px-4 py-2 text-gray-500">{category.slug}</td>
                <td className="px-4 py-2 text-gray-500">
                  {categories.find((c) => c.id === category.parentCategoryId)?.name ?? '—'}
                </td>
                <td className="px-4 py-2 text-right">
                  <button
                    type="button"
                    onClick={() => {
                      setEditing(category)
                      setShowForm(true)
                    }}
                    className="mr-3 text-blue-600 hover:underline"
                  >
                    Изменить
                  </button>
                  <button type="button" onClick={() => handleDelete(category)} className="text-red-600 hover:underline">
                    Удалить
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        </div>
      )}
    </div>
  )
}
