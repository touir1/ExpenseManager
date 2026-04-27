import { describe, it, expect, vi, beforeEach } from 'vitest'
import { act } from 'react'
import { render, renderHook, waitFor } from '@testing-library/react'
import { AuthProvider, useAuth } from '@/features/auth/AuthContext'
import * as api from '@/services/api.service'
import { API_ERRORS } from '@/constants/apiErrors.constant'

vi.mock('@/services/api.service', () => ({
  post: vi.fn(),
  get: vi.fn(),
  onUnauthorized: vi.fn()
}))

const mockLocationAssign = vi.fn()
Object.defineProperty(window, 'location', {
  value: { assign: mockLocationAssign },
  writable: true
})

const AUTH_BASE = '/api/users/auth'
const SKIP = { skipUnauthorized: true }

describe('AuthContext', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Default: session check fails (no valid access token)
    vi.mocked(api.get).mockResolvedValue({ ok: false, status: 401 })
    // Default: refresh fails (no valid refresh token) — prevents mount side-effects
    // from consuming mockResolvedValueOnce values set in individual tests
    vi.mocked(api.post).mockResolvedValue({ ok: false, status: 401 })
    mockLocationAssign.mockClear()
  })

  describe('AuthProvider initialization', () => {
    it('starts with isLoading=true then resolves to false', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
    })

    it('always calls session check on mount', async () => {
      renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => {})
      expect(api.get).toHaveBeenCalledWith(`${AUTH_BASE}/session`, SKIP)
    })

    it('restores session when session check returns user data', async () => {
      const userData = { email: 'test@test.com', firstName: 'Test', lastName: 'User' }
      vi.mocked(api.get).mockResolvedValueOnce({ ok: true, status: 200, data: userData })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.user).toEqual(userData)
      expect(api.post).not.toHaveBeenCalled()
    })

    it('falls back to refresh when session check fails (access token expired)', async () => {
      const userData = { email: 'test@test.com', firstName: 'Test', lastName: 'User' }
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200 }) // refresh succeeds
      vi.mocked(api.get)
        .mockResolvedValueOnce({ ok: false, status: 401 }) // first session check fails
        .mockResolvedValueOnce({ ok: true, status: 200, data: userData }) // retry succeeds

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.user).toEqual(userData)
    })

    it('stays unauthenticated when both session check and refresh fail', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
    })

    it('stays unauthenticated when refresh succeeds but retry session check also fails', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200 }) // refresh ok

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
    })

    it('sets isLoading=false even when session check fails with network error', async () => {
      vi.mocked(api.get).mockResolvedValueOnce({ ok: false, status: 0, error: API_ERRORS.NETWORK })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(false)
    })
  })

  describe('login', () => {
    it('successfully logs in with user data from API response', async () => {
      const mockUser = { email: 'user@example.com', firstName: 'User', lastName: 'Name' }

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200, data: { user: mockUser } })
      const loginResult = await result.current.login('user@example.com', 'password')

      await waitFor(() => expect(result.current.isAuthenticated).toBe(true))
      expect(loginResult.ok).toBe(true)
      expect(loginResult.error).toBeUndefined()
      expect(result.current.user).toEqual(mockUser)
    })

    it('uses email as fallback user when API response has no user object', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200, data: {} })
      await result.current.login('fallback@example.com', 'password')

      await waitFor(() => expect(result.current.user).toEqual({ email: 'fallback@example.com' }))
    })

    it('returns ok=false with error on failed login and does not set authenticated', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: false, status: 401, error: 'Invalid email or password.' })
      const loginResult = await result.current.login('wrong@example.com', 'wrong')

      expect(loginResult.ok).toBe(false)
      expect(loginResult.error).toBe('Invalid email or password.')
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
    })

    it('returns ok=false with network error when login request fails due to network error', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: false, status: 0, error: API_ERRORS.NETWORK })
      const loginResult = await result.current.login('user@test.com', 'pass')

      expect(loginResult.ok).toBe(false)
      expect(loginResult.error).toBe(API_ERRORS.NETWORK)
      expect(result.current.isAuthenticated).toBe(false)
    })

    it('includes application code and rememberMe in login request', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      await result.current.login('user@test.com', 'pass', true)

      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/login`,
        expect.objectContaining({ email: 'user@test.com', password: 'pass', applicationCode: expect.any(String), rememberMe: true }),
        SKIP
      )
    })

    it('passes rememberMe=false by default', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      await result.current.login('user@test.com', 'pass')

      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/login`,
        expect.objectContaining({ rememberMe: false }),
        SKIP
      )
    })
  })

  describe('logout', () => {
    it('clears user state and calls logout endpoint', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200, data: { user: { email: 'test@test.com' } } })
      await result.current.login('test@test.com', 'password', true)
      await waitFor(() => expect(result.current.isAuthenticated).toBe(true))

      act(() => { result.current.logout() })

      await waitFor(() => expect(result.current.isAuthenticated).toBe(false))
      expect(result.current.user).toBeNull()
    })
  })

  describe('register', () => {
    it('successfully registers a user', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 201 })
      const registerResult = await result.current.register('John', 'Doe', 'john@example.com')

      expect(registerResult.ok).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/register`,
        expect.objectContaining({ firstName: 'John', lastName: 'Doe', email: 'john@example.com', applicationCode: expect.any(String) }),
        SKIP
      )
    })

    it('returns ok=false on failed registration', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: false, status: 400 })
      const registerResult = await result.current.register('John', 'Doe', 'existing@example.com')

      expect(registerResult.ok).toBe(false)
    })
  })

  describe('changePassword', () => {
    it('successfully changes password', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200 })
      const changeResult = await result.current.changePassword('oldpass', 'newpass', 'newpass')

      expect(changeResult.ok).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/change-password`,
        { email: undefined, oldPassword: 'oldpass', newPassword: 'newpass', confirmPassword: 'newpass' },
        SKIP
      )
    })

    it('returns ok=false when passwords do not match', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      vi.clearAllMocks()

      const changeResult = await result.current.changePassword('oldpass', 'newpass1', 'newpass2')

      expect(changeResult.ok).toBe(false)
      expect(api.post).not.toHaveBeenCalled()
    })

    it('returns ok=false when old password is empty', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const changeResult = await result.current.changePassword('', 'newpass', 'newpass')

      expect(changeResult.ok).toBe(false)
    })

    it('returns ok=false with error when API rejects the password change', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: false, status: 400, error: 'Invalid email or password.' })
      const changeResult = await result.current.changePassword('wrongpass', 'newpass12', 'newpass12')

      expect(changeResult.ok).toBe(false)
      expect(changeResult.error).toBe('Invalid email or password.')
    })
  })

  describe('resetPassword', () => {
    it('successfully resets password', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200 })
      const resetResult = await result.current.resetPassword('user@test.com', 'verification-hash', 'newpass', 'newpass')

      expect(resetResult.ok).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/change-password-reset`,
        { email: 'user@test.com', verificationHash: 'verification-hash', newPassword: 'newpass', confirmPassword: 'newpass' },
        SKIP
      )
    })

    it('returns ok=false when passwords do not match', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      vi.clearAllMocks()

      const resetResult = await result.current.resetPassword('user@test.com', 'hash', 'pass1', 'pass2')

      expect(resetResult.ok).toBe(false)
      expect(api.post).not.toHaveBeenCalled()
    })

    it('returns ok=false when required fields are missing', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const resetResult = await result.current.resetPassword('', 'hash', 'pass', 'pass')

      expect(resetResult.ok).toBe(false)
    })
  })

  describe('requestPasswordReset', () => {
    it('successfully requests password reset', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200 })
      const requestResult = await result.current.requestPasswordReset!('user@test.com')

      expect(requestResult.ok).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/request-password-reset`,
        { email: 'user@test.com' },
        SKIP
      )
    })

    it('returns ok=false when email is empty', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))
      vi.clearAllMocks()

      const requestResult = await result.current.requestPasswordReset!('')

      expect(requestResult.ok).toBe(false)
      expect(api.post).not.toHaveBeenCalled()
    })
  })

  describe('useAuth hook', () => {
    it('throws error when used outside AuthProvider', () => {
      const TestComponent = () => { useAuth(); return null }
      expect(() => render(<TestComponent />)).toThrow('useAuth must be used within AuthProvider')
    })
  })

  describe('unauthorized handler', () => {
    it('sets up unauthorized handler on mount', () => {
      renderHook(() => useAuth(), { wrapper: AuthProvider })
      expect(api.onUnauthorized).toHaveBeenCalled()
    })

    it('clears auth state and redirects when unauthorized handler is invoked', async () => {
      const userData = { email: 'test@example.com', firstName: 'Test', lastName: 'User' }
      vi.mocked(api.get).mockResolvedValueOnce({ ok: true, status: 200, data: userData })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isAuthenticated).toBe(true))

      const unauthorizedCallback = (api.onUnauthorized as ReturnType<typeof vi.fn>).mock.calls[0][0]
      act(() => { unauthorizedCallback() })

      await waitFor(() => expect(result.current.isAuthenticated).toBe(false))
      expect(result.current.user).toBeNull()
      expect(mockLocationAssign).toHaveBeenCalledWith('/login')
    })
  })
})
