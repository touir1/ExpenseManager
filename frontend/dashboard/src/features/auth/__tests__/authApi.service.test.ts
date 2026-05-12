import { describe, it, expect, vi, beforeEach } from 'vitest'
import * as api from '@/services/api.service'
import {
  sessionCheck,
  refreshRequest,
  loginRequest,
  logoutRequest,
  registerRequest,
  changePasswordRequest,
  createPasswordRequest,
  resetPasswordRequest,
  requestPasswordResetRequest,
  resendVerificationRequest,
} from '../services/authApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
}))

const BASE = '/api/users/auth'

describe('authApi.service', () => {
  beforeEach(() => vi.clearAllMocks())

  it('sessionCheck calls GET /auth/session with skipUnauthorized and silent', () => {
    sessionCheck()
    expect(api.get).toHaveBeenCalledWith(`${BASE}/session`, { skipUnauthorized: true, silent: true })
  })

  it('refreshRequest calls POST /auth/refresh with skipUnauthorized and silent', () => {
    refreshRequest()
    expect(api.post).toHaveBeenCalledWith(`${BASE}/refresh`, {}, { skipUnauthorized: true, silent: true })
  })

  it('loginRequest calls POST /auth/login with credentials', () => {
    loginRequest('user@example.com', 'pass', 'APP', true)
    expect(api.post).toHaveBeenCalledWith(
      `${BASE}/login`,
      { email: 'user@example.com', password: 'pass', applicationCode: 'APP', rememberMe: true },
      { skipUnauthorized: true, silent: true }
    )
  })

  it('logoutRequest calls POST /auth/logout', () => {
    logoutRequest()
    expect(api.post).toHaveBeenCalledWith(`${BASE}/logout`, {}, { skipUnauthorized: true })
  })

  it('registerRequest calls POST /auth/register with user details', () => {
    registerRequest('John', 'Doe', 'john@example.com', 'APP')
    expect(api.post).toHaveBeenCalledWith(
      `${BASE}/register`,
      { firstName: 'John', lastName: 'Doe', email: 'john@example.com', applicationCode: 'APP' },
      { skipUnauthorized: true }
    )
  })

  it('changePasswordRequest calls POST /auth/change-password', () => {
    changePasswordRequest('user@example.com', 'old', 'new')
    expect(api.post).toHaveBeenCalledWith(
      `${BASE}/change-password`,
      { email: 'user@example.com', oldPassword: 'old', newPassword: 'new' },
      { skipUnauthorized: true }
    )
  })

  it('createPasswordRequest calls POST /auth/create-password', () => {
    createPasswordRequest('user@example.com', 'hash123', 'newpass')
    expect(api.post).toHaveBeenCalledWith(
      `${BASE}/create-password`,
      { email: 'user@example.com', verificationHash: 'hash123', newPassword: 'newpass' },
      { skipUnauthorized: true }
    )
  })

  it('resetPasswordRequest calls POST /auth/change-password-reset', () => {
    resetPasswordRequest('user@example.com', 'hash123', 'newpass')
    expect(api.post).toHaveBeenCalledWith(
      `${BASE}/change-password-reset`,
      { email: 'user@example.com', verificationHash: 'hash123', newPassword: 'newpass' },
      { skipUnauthorized: true }
    )
  })

  it('requestPasswordResetRequest calls POST /auth/request-password-reset', () => {
    requestPasswordResetRequest('user@example.com', 'APP')
    expect(api.post).toHaveBeenCalledWith(
      `${BASE}/request-password-reset`,
      { email: 'user@example.com', appCode: 'APP' },
      { skipUnauthorized: true }
    )
  })

  it('resendVerificationRequest calls POST /auth/resend-verification with skipUnauthorized and silent', () => {
    resendVerificationRequest('user@example.com', 'APP')
    expect(api.post).toHaveBeenCalledWith(
      `${BASE}/resend-verification`,
      { email: 'user@example.com', applicationCode: 'APP' },
      { skipUnauthorized: true, silent: true }
    )
  })
})
