export type UserRole = 'Customer' | 'Admin'

export interface User {
  id: string
  email: string
  fullName: string
  role: UserRole
}

export interface AuthResult {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
  user: User
}

export interface RegisterPayload {
  email: string
  password: string
  fullName: string
}

export interface LoginPayload {
  email: string
  password: string
}
