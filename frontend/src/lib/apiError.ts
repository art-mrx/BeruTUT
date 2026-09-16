import { isAxiosError } from 'axios'
import type { ProblemDetails } from '@/types'

export function getApiErrorMessage(error: unknown, fallback = 'Произошла ошибка. Попробуйте ещё раз.'): string {
  if (isAxiosError<ProblemDetails>(error) && error.response?.data) {
    const data = error.response.data
    if (data.errors) {
      return Object.values(data.errors).flat().join(' ')
    }
    return data.detail ?? data.title ?? fallback
  }
  return fallback
}

/** Field-level validation errors from ASP.NET/FluentValidation, keyed by PascalCase property name. */
export function getApiFieldErrors(error: unknown): Record<string, string> | null {
  if (isAxiosError<ProblemDetails>(error) && error.response?.data.errors) {
    const errors = error.response.data.errors
    return Object.fromEntries(
      Object.entries(errors).map(([field, messages]) => [
        field.charAt(0).toLowerCase() + field.slice(1),
        messages.join(' '),
      ]),
    )
  }
  return null
}
