import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/features/auth/authStore'
import { useRegister } from '@/features/auth/useRegister'
import { registerSchema, type RegisterFormValues } from '@/features/auth/schemas'
import { getApiErrorMessage, getApiFieldErrors } from '@/lib/apiError'

export function RegisterPage() {
  const navigate = useNavigate()
  const registerMutation = useRegister()
  const isAuthenticated = useAuthStore((state) => Boolean(state.user))

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) })

  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }

  function onSubmit(values: RegisterFormValues) {
    registerMutation.mutate(
      { email: values.email, password: values.password, fullName: values.fullName },
      {
        onSuccess: () => navigate('/', { replace: true }),
        onError: (error) => {
          const fieldErrors = getApiFieldErrors(error)
          if (fieldErrors) {
            for (const [field, message] of Object.entries(fieldErrors)) {
              setError(field as keyof RegisterFormValues, { message })
            }
          }
        },
      },
    )
  }

  return (
    <div className="mx-auto max-w-sm">
      <h1 className="mb-4 text-2xl font-semibold text-gray-900">Регистрация</h1>

      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-gray-700">Имя</span>
          <input
            type="text"
            {...register('fullName')}
            className="rounded border border-gray-300 px-3 py-2"
            autoComplete="name"
          />
          {errors.fullName && <span className="text-sm text-red-600">{errors.fullName.message}</span>}
        </label>

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
            autoComplete="new-password"
          />
          {errors.password && <span className="text-sm text-red-600">{errors.password.message}</span>}
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-gray-700">Повторите пароль</span>
          <input
            type="password"
            {...register('confirmPassword')}
            className="rounded border border-gray-300 px-3 py-2"
            autoComplete="new-password"
          />
          {errors.confirmPassword && <span className="text-sm text-red-600">{errors.confirmPassword.message}</span>}
        </label>

        {registerMutation.isError && !getApiFieldErrors(registerMutation.error) && (
          <p className="text-sm text-red-600">
            {getApiErrorMessage(registerMutation.error, 'Не удалось зарегистрироваться.')}
          </p>
        )}

        <button
          type="submit"
          disabled={registerMutation.isPending}
          className="rounded bg-gray-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
        >
          {registerMutation.isPending ? 'Регистрируем…' : 'Зарегистрироваться'}
        </button>
      </form>

      <p className="mt-4 text-sm text-gray-600">
        Уже есть аккаунт?{' '}
        <Link to="/login" className="text-blue-600 hover:underline">
          Войти
        </Link>
      </p>
    </div>
  )
}
