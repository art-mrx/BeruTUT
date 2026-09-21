import { isAxiosError } from 'axios'

export const ACCOUNT_BLOCKED_MESSAGE = 'Ваш аккаунт заблокирован. Обратитесь к администратору.'

const FLAG_KEY = 'berutut-account-blocked'

/** 403 from the auth endpoints means the account is blocked (nothing else answers 403 there). */
export function isAccountBlockedError(error: unknown): boolean {
  return isAxiosError(error) && error.response?.status === 403
}

// When an admin blocks a user who is mid-session, the next request logs them out silently. The flag
// lets the login page explain why instead of just showing an empty form.
export function markAccountBlocked() {
  try {
    sessionStorage.setItem(FLAG_KEY, '1')
  } catch {
    // sessionStorage can be unavailable (private mode) — the notice is a nicety, not required.
  }
}

export function wasAccountBlocked(): boolean {
  try {
    return sessionStorage.getItem(FLAG_KEY) === '1'
  } catch {
    return false
  }
}

export function clearAccountBlocked() {
  try {
    sessionStorage.removeItem(FLAG_KEY)
  } catch {
    // ignore
  }
}
