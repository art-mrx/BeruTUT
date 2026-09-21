import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { adminUsersApi, type AdminUserListParams } from '@/api/adminUsers'

export function useAdminUsers(params: AdminUserListParams, options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: ['admin', 'users', params],
    queryFn: () => adminUsersApi.list(params),
    placeholderData: (previousData) => previousData,
    enabled: options.enabled ?? true,
  })
}

export function useSetUserBlocked() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, blocked }: { id: string; blocked: boolean }) =>
      blocked ? adminUsersApi.block(id) : adminUsersApi.unblock(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'users'] }),
  })
}
