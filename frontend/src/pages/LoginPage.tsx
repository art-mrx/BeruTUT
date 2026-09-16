import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useLocation, useNavigate, type Location } from 'react-router-dom'
import { useAuthStore } from '@/features/auth/authStore'
import { useLogin } from '@/features/auth/useLogin'
import { loginSchema, type LoginFormValues } from '@/features/auth/schemas'
import { getApiErrorMessage, getApiFieldErrors } from '@/lib/apiError'

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const login = useLogin()
  const isAuthenticated = useAuthStore((state) => Boolean(state.user))

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) })

  const from = (location.state as { from?: Location })?.from?.pathname ?? '/'

  if (isAuthenticated) {
    return <Navigate to={from} replace />
  }

  function onSubmit(values: LoginFormValues) {
    login.mutate(values, {
      onSuccess: () => navigate(from, { replace: true }),
      onError: (error) => {
        const fieldErrors = getApiFieldErrors(error)
        if (fieldErrors) {
          for (const [field, message] of Object.entries(fieldErrors)) {
            setError(field as keyof LoginFormValues, { message })
          }
        }
      },
    })
  }

  return (
    <div className="mx-auto max-w-sm">
      <h1 className="mb-4 text-2xl font-semibold text-gray-900">Вход</h1>

      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-gray-700">Email</span>
          <input
            type="email"
            {...register('email')}
            className="rounded border border-gray-300 px-3 py-2"
            autoComplete="email"
          />
          {errors.email && <span className="text-sm text-red-600">{errors.email.message}</span>}
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-gray-700">Пароль</span>
          <input
            type="password"
            {...register('password')}
            className="rounded border border-gray-300 px-3 py-2"
            autoComplete="current-password"
          />
          {errors.password && <span className="text-sm text-red-600">{errors.password.message}</span>}
        </label>

        {login.isError && !getApiFieldErrors(login.error) && (
          <p className="text-sm text-red-600">{getApiErrorMessage(login.error, 'Не удалось войти.')}</p>
        )}

        <button
          type="submit"
          disabled={login.isPending}
          className="rounded bg-gray-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
        >
          {login.isPending ? 'Входим…' : 'Войти'}
        </button>
      </form>

      <p className="mt-4 text-sm text-gray-600">
        Нет аккаунта?{' '}
        <Link to="/register" className="text-blue-600 hover:underline">
          Зарегистрироваться
        </Link>
      </p>
    </div>
  )
}
