import { get, post } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { User } from '@/features/auth/types/auth.type'

const AUTH_BASE = '/api/users/auth'

export type LoginResponse = { user?: User }

export function sessionCheck(): Promise<ApiResponse<User>> {
  return get<User>(`${AUTH_BASE}/session`, { skipUnauthorized: true })
}

export function refreshRequest(): Promise<ApiResponse<void>> {
  return post<void>(`${AUTH_BASE}/refresh`, {}, { skipUnauthorized: true })
}

export function loginRequest(
  email: string,
  password: string,
  applicationCode: string,
  rememberMe: boolean
): Promise<ApiResponse<LoginResponse>> {
  return post<LoginResponse>(
    `${AUTH_BASE}/login`,
    { email, password, applicationCode, rememberMe },
    { skipUnauthorized: true }
  )
}

export function logoutRequest(): Promise<ApiResponse<unknown>> {
  return post<unknown>(`${AUTH_BASE}/logout`, {}, { skipUnauthorized: true })
}

export function registerRequest(
  firstName: string,
  lastName: string,
  email: string,
  applicationCode: string
): Promise<ApiResponse<unknown>> {
  return post<unknown>(
    `${AUTH_BASE}/register`,
    { firstName, lastName, email, applicationCode },
    { skipUnauthorized: true }
  )
}

export function changePasswordRequest(
  email: string | undefined,
  oldPassword: string,
  newPassword: string,
  confirmPassword: string
): Promise<ApiResponse<unknown>> {
  return post<unknown>(
    `${AUTH_BASE}/change-password`,
    { email, oldPassword, newPassword, confirmPassword },
    { skipUnauthorized: true }
  )
}

export function resetPasswordRequest(
  email: string,
  verificationHash: string,
  newPassword: string,
  confirmPassword: string
): Promise<ApiResponse<unknown>> {
  return post<unknown>(
    `${AUTH_BASE}/change-password-reset`,
    { email, verificationHash, newPassword, confirmPassword },
    { skipUnauthorized: true }
  )
}

export function requestPasswordResetRequest(email: string, applicationCode: string): Promise<ApiResponse<unknown>> {
  return post<unknown>(`${AUTH_BASE}/request-password-reset`, { email, appCode: applicationCode }, { skipUnauthorized: true })
}
