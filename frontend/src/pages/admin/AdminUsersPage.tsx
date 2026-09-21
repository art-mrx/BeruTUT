import { useState } from 'react'
import { Pagination } from '@/components/Pagination'
import { useAdminUsers, useSetUserBlocked } from '@/features/admin/useAdminUsers'
import { useAuthStore } from '@/features/auth/authStore'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'
import { getApiErrorMessage } from '@/lib/apiError'
import type { AdminUser } from '@/types'

const PAGE_SIZE = 10

type StatusFilter = '' | 'active' | 'blocked'

const dateFormat: Intl.DateTimeFormatOptions = { dateStyle: 'short', timeStyle: 'short' }

export function AdminUsersPage() {
  const currentUserId = useAuthStore((state) => state.user?.id)
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('')
  const [sortBy, setSortBy] = useState<'newest' | 'oldest'>('newest')
  const [actionError, setActionError] = useState<string | null>(null)

  const debouncedSearch = useDebouncedValue(search).trim()
  const filtersActive = Boolean(search || statusFilter)

  const { data, isLoading, isError } = useAdminUsers({
    page,
    pageSize: PAGE_SIZE,
    sortBy,
    search: debouncedSearch || undefined,
    isBlocked: statusFilter === '' ? undefined : statusFilter === 'blocked',
  })
  const setBlocked = useSetUserBlocked()

  // Any filter change restarts pagination, otherwise the current page may not exist in the new result set.
  function changeFilter<T>(setter: (value: T) => void) {
    return (value: T) => {
      setter(value)
      setPage(1)
    }
  }

  function handleToggle(user: AdminUser) {
    const block = !user.isBlocked
    if (
      block &&
      !window.confirm(
        `Заблокировать пользователя ${user.email}? Он не сможет войти в аккаунт, а все его текущие сессии будут завершены.`,
      )
    ) {
      return
    }
    setActionError(null)
    setBlocked.mutate(
      { id: user.id, blocked: block },
      { onError: (error) => setActionError(getApiErrorMessage(error, 'Не удалось изменить статус пользователя.')) },
    )
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-semibold text-gray-900">Пользователи</h1>

      <div className="mb-4 flex flex-wrap items-end gap-3 rounded-lg border border-gray-200 bg-white p-3 text-sm">
        <label className="flex flex-col gap-1">
          <span className="text-gray-600">Поиск</span>
          <input
            type="search"
            value={search}
            onChange={(e) => changeFilter(setSearch)(e.target.value)}
            placeholder="имя, email или номер"
            className="w-56 rounded border border-gray-300 px-2 py-1"
          />
        </label>

        <label className="flex flex-col gap-1">
          <span className="text-gray-600">Статус</span>
          <select
            value={statusFilter}
            onChange={(e) => changeFilter(setStatusFilter)(e.target.value as StatusFilter)}
            className="rounded border border-gray-300 px-2 py-1"
          >
            <option value="">Все</option>
            <option value="active">Активные</option>
            <option value="blocked">Заблокированные</option>
          </select>
        </label>

        <label className="flex flex-col gap-1">
          <span className="text-gray-600">Сортировка</span>
          <select
            value={sortBy}
            onChange={(e) => changeFilter(setSortBy)(e.target.value as 'newest' | 'oldest')}
            className="rounded border border-gray-300 px-2 py-1"
          >
            <option value="newest">Сначала новые</option>
            <option value="oldest">Сначала старые</option>
          </select>
        </label>

        {filtersActive && (
          <button
            type="button"
            onClick={() => {
              setSearch('')
              setStatusFilter('')
              setPage(1)
            }}
            className="rounded border border-gray-300 px-3 py-1 text-gray-700 hover:bg-gray-50"
          >
            Сбросить
          </button>
        )}
      </div>

      {actionError && <p className="mb-3 text-sm text-red-600">{actionError}</p>}
      {isLoading && <p className="text-gray-500">Загрузка…</p>}
      {isError && <p className="text-red-600">Не удалось загрузить пользователей.</p>}
      {data && data.items.length === 0 && <p className="text-gray-500">Пользователей не найдено.</p>}

      {data && data.items.length > 0 && (
        <>
          <p className="mb-2 text-sm text-gray-500">Всего: {data.totalCount}</p>
          <div className="overflow-x-auto rounded-lg border border-gray-200">
            <table className="w-full min-w-[820px] bg-white text-sm">
              <thead className="bg-gray-50 text-left text-gray-600">
                <tr>
                  <th className="px-4 py-2 font-medium">№</th>
                  <th className="px-4 py-2 font-medium">Пользователь</th>
                  <th className="px-4 py-2 font-medium">Роль</th>
                  <th className="px-4 py-2 font-medium">Регистрация</th>
                  <th className="px-4 py-2 font-medium">Заказов</th>
                  <th className="px-4 py-2 font-medium">Статус</th>
                  <th className="px-4 py-2" />
                </tr>
              </thead>
              <tbody>
                {data.items.map((user) => (
                  <tr key={user.id} className="border-t border-gray-100 align-top">
                    <td className="px-4 py-2 font-mono text-xs text-gray-500" title={user.id}>
                      {user.id.slice(0, 8)}
                    </td>
                    <td className="px-4 py-2">
                      <p className="text-gray-900">
                        {user.fullName}
                        {user.id === currentUserId && <span className="ml-2 text-xs text-gray-400">(это вы)</span>}
                      </p>
                      <a href={`mailto:${user.email}`} className="text-blue-600 hover:underline">
                        {user.email}
                      </a>
                    </td>
                    <td className="px-4 py-2 text-gray-700">{user.role === 'Admin' ? 'Администратор' : 'Клиент'}</td>
                    <td className="whitespace-nowrap px-4 py-2 text-gray-700">
                      {new Date(user.createdAt).toLocaleString('ru-RU', dateFormat)}
                    </td>
                    <td className="px-4 py-2 text-gray-900">{user.ordersCount}</td>
                    <td className="px-4 py-2">
                      {user.isBlocked ? (
                        <>
                          <span className="rounded-full bg-red-100 px-2 py-0.5 text-xs font-medium text-red-800">
                            Заблокирован
                          </span>
                          {user.blockedAt && (
                            <p className="mt-1 text-xs text-gray-400">
                              с {new Date(user.blockedAt).toLocaleString('ru-RU', dateFormat)}
                            </p>
                          )}
                        </>
                      ) : (
                        <span className="rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800">
                          Активен
                        </span>
                      )}
                    </td>
                    <td className="whitespace-nowrap px-4 py-2 text-right">
                      {user.role === 'Admin' ? (
                        <span className="text-xs text-gray-400">—</span>
                      ) : (
                        <button
                          type="button"
                          disabled={setBlocked.isPending}
                          onClick={() => handleToggle(user)}
                          className={`rounded px-3 py-1 text-xs font-medium disabled:opacity-50 ${
                            user.isBlocked
                              ? 'bg-green-600 text-white hover:bg-green-700'
                              : 'border border-red-300 text-red-700 hover:bg-red-50'
                          }`}
                        >
                          {user.isBlocked ? 'Разблокировать' : 'Заблокировать'}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  )
}
