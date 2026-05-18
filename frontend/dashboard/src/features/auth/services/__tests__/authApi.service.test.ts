import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
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
} from '../authApi.service'

// Mock api service
vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
}))

describe('authApi.service', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.clearAllMocks()
  })

  describe('sessionCheck', () => {
    it('should call get with correct endpoint', async () => {
      const mockResponse = { ok: true, status: 200, data: { id: '1', email: 'test@test.com' } }
      vi.mocked(api.get).mockResolvedValue(mockResponse)

      const result = await sessionCheck()

      expect(api.get).toHaveBeenCalledWith('/api/users/auth/session', {
        skipUnauthorized: true,
        silent: true,
      })
      expect(result).toEqual(mockResponse)
    })

    it('should handle session check failure', async () => {
      const mockResponse = { ok: false, status: 401, error: 'Unauthorized' }
      vi.mocked(api.get).mockResolvedValue(mockResponse)

      const result = await sessionCheck()

      expect(result).toEqual(mockResponse)
    })
  })

  describe('refreshRequest', () => {
    it('should call post with correct endpoint', async () => {
      const mockResponse = { ok: true, status: 200, data: undefined }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await refreshRequest()

      expect(api.post).toHaveBeenCalledWith('/api/users/auth/refresh', {}, {
        skipUnauthorized: true,
        silent: true,
      })
      expect(result).toEqual(mockResponse)
    })

    it('should handle refresh failure', async () => {
      const mockResponse = { ok: false, status: 401, error: 'Token expired' }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await refreshRequest()

      expect(result).toEqual(mockResponse)
    })
  })

  describe('loginRequest', () => {
    it('should call post with correct login credentials', async () => {
      const mockResponse = { ok: true, status: 200, data: { user: { id: '1', email: 'test@test.com' } } }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await loginRequest('test@test.com', 'password123', 'EXPENSES_MANAGER', false)

      expect(api.post).toHaveBeenCalledWith('/api/users/auth/login', {
        email: 'test@test.com',
        password: 'password123',
        applicationCode: 'EXPENSES_MANAGER',
        rememberMe: false,
      }, { skipUnauthorized: true, silent: true })
      expect(result).toEqual(mockResponse)
    })

    it('should handle login with rememberMe enabled', async () => {
      const mockResponse = { ok: true, status: 200, data: { user: { id: '1', email: 'test@test.com' } } }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      await loginRequest('test@test.com', 'password123', 'EXPENSES_MANAGER', true)

      expect(api.post).toHaveBeenCalledWith(
        '/api/users/auth/login',
        expect.objectContaining({ rememberMe: true }),
        expect.any(Object)
      )
    })

    it('should handle login failure', async () => {
      const mockResponse = { ok: false, status: 401, error: 'Invalid credentials' }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await loginRequest('test@test.com', 'wrongpassword', 'EXPENSES_MANAGER', false)

      expect(result).toEqual(mockResponse)
    })
  })

  describe('logoutRequest', () => {
    it('should call post with correct endpoint', async () => {
      const mockResponse = { ok: true, status: 200, data: null }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await logoutRequest()

      expect(api.post).toHaveBeenCalledWith('/api/users/auth/logout', {}, {
        skipUnauthorized: true,
      })
      expect(result).toEqual(mockResponse)
    })

    it('should handle logout failure', async () => {
      const mockResponse = { ok: false, status: 500, error: 'Server error' }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await logoutRequest()

      expect(result).toEqual(mockResponse)
    })
  })

  describe('registerRequest', () => {
    it('should call post with registration data', async () => {
      const mockResponse = { ok: true, status: 200, data: null }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await registerRequest('John', 'Doe', 'john@test.com', 'EXPENSES_MANAGER')

      expect(api.post).toHaveBeenCalledWith('/api/users/auth/register', {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@test.com',
        applicationCode: 'EXPENSES_MANAGER',
      }, { skipUnauthorized: true })
      expect(result).toEqual(mockResponse)
    })

    it('should handle registration failure', async () => {
      const mockResponse = { ok: false, status: 400, error: 'Email already exists' }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await registerRequest('Jane', 'Smith', 'jane@test.com', 'EXPENSES_MANAGER')

      expect(result).toEqual(mockResponse)
    })
  })

  describe('changePasswordRequest', () => {
    it('should call post with password change data', async () => {
      const mockResponse = { ok: true, status: 200, data: null }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await changePasswordRequest('test@test.com', 'oldpass', 'newpass')

      expect(api.post).toHaveBeenCalledWith('/api/users/auth/change-password', {
        email: 'test@test.com',
        oldPassword: 'oldpass',
        newPassword: 'newpass',
      }, { skipUnauthorized: true })
      expect(result).toEqual(mockResponse)
    })

    it('should handle password change with undefined email', async () => {
      const mockResponse = { ok: true, status: 200, data: null }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      await changePasswordRequest(undefined, 'oldpass', 'newpass')

      expect(api.post).toHaveBeenCalledWith('/api/users/auth/change-password', {
        email: undefined,
        oldPassword: 'oldpass',
        newPassword: 'newpass',
      }, { skipUnauthorized: true })
    })

    it('should handle password change failure', async () => {
      const mockResponse = { ok: false, status: 400, error: 'Incorrect password' }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await changePasswordRequest('test@test.com', 'wrongold', 'newpass')

      expect(result).toEqual(mockResponse)
    })
  })

  describe('createPasswordRequest', () => {
    it('should call post with create password data', async () => {
      const mockResponse = { ok: true, status: 200, data: null }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await createPasswordRequest('test@test.com', 'hash123', 'newpass')

      expect(api.post).toHaveBeenCalledWith('/api/users/auth/create-password', {
        email: 'test@test.com',
        verificationHash: 'hash123',
        newPassword: 'newpass',
      }, { skipUnauthorized: true })
      expect(result).toEqual(mockResponse)
    })

    it('should handle create password failure', async () => {
      const mockResponse = { ok: false, status: 400, error: 'Invalid verification hash' }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await createPasswordRequest('test@test.com', 'invalihash', 'newpass')

      expect(result).toEqual(mockResponse)
    })
  })

  describe('resetPasswordRequest', () => {
    it('should call post with reset password data', async () => {
      const mockResponse = { ok: true, status: 200, data: null }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await resetPasswordRequest('test@test.com', 'hash123', 'newpass')

      expect(api.post).toHaveBeenCalledWith('/api/users/auth/change-password-reset', {
        email: 'test@test.com',
        verificationHash: 'hash123',
        newPassword: 'newpass',
      }, { skipUnauthorized: true })
      expect(result).toEqual(mockResponse)
    })

    it('should handle reset password failure', async () => {
      const mockResponse = { ok: false, status: 400, error: 'Link expired' }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await resetPasswordRequest('test@test.com', 'expiredhash', 'newpass')

      expect(result).toEqual(mockResponse)
    })
  })

  describe('requestPasswordResetRequest', () => {
    it('should call post with password reset request data', async () => {
      const mockResponse = { ok: true, status: 200, data: null }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await requestPasswordResetRequest('test@test.com', 'EXPENSES_MANAGER')

      expect(api.post).toHaveBeenCalledWith('/api/users/auth/request-password-reset', {
        email: 'test@test.com',
        appCode: 'EXPENSES_MANAGER',
      }, { skipUnauthorized: true })
      expect(result).toEqual(mockResponse)
    })

    it('should handle password reset request failure', async () => {
      const mockResponse = { ok: false, status: 400, error: 'Email not found' }
      vi.mocked(api.post).mockResolvedValue(mockResponse)

      const result = await requestPasswordResetRequest('nonexistent@test.com', 'EXPENSES_MANAGER')

      expect(result).toEqual(mockResponse)
    })
  })
})
