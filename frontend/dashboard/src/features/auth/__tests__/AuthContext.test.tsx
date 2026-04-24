import { describe, it, expect, vi, beforeEach } from 'vitest'
import { act } from 'react'
import { render, renderHook, waitFor } from '@testing-library/react'
import { AuthProvider, useAuth } from '@/features/auth/AuthContext'
import * as api from '@/services/api'

vi.mock('@/services/api', () => ({
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

describe('AuthContext', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    vi.mocked(api.get).mockResolvedValue({ ok: false, status: 401 })
    vi.mocked(api.post).mockResolvedValue({ ok: true, status: 200 })
    mockLocationAssign.mockClear()
  })

  describe('AuthProvider initialization', () => {
    it('starts with isLoading=true then resolves to false', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
    })

    it('does not call session check when no user in localStorage', async () => {
      renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => {})
      expect(api.get).not.toHaveBeenCalled()
    })

    it('restores session when user in localStorage and session check succeeds', async () => {
      const storedUser = { email: 'test@test.com', firstName: 'Test' }
      localStorage.setItem('auth:user', JSON.stringify(storedUser))
      vi.mocked(api.get).mockResolvedValueOnce({ ok: true, status: 200 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.user).toEqual(storedUser)
      expect(api.get).toHaveBeenCalledWith(`${AUTH_BASE}/session`)
    })

    it('clears user from localStorage when session check fails', async () => {
      localStorage.setItem('auth:user', JSON.stringify({ email: 'test@test.com' }))
      vi.mocked(api.get).mockResolvedValueOnce({ ok: false, status: 401 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
      expect(localStorage.getItem('auth:user')).toBeNull()
    })

    it('clears user from localStorage when stored JSON is malformed', async () => {
      localStorage.setItem('auth:user', 'not-valid-json')
      vi.mocked(api.get).mockResolvedValueOnce({ ok: true, status: 200 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isAuthenticated).toBe(false)
      expect(localStorage.getItem('auth:user')).toBeNull()
    })
  })

  describe('login', () => {
    it('successfully logs in and stores user info', async () => {
      const mockUser = { email: 'user@example.com', firstName: 'User' }
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200, data: { user: mockUser } })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const loginResult = await result.current.login('user@example.com', 'password')

      await waitFor(() => expect(result.current.isAuthenticated).toBe(true))
      expect(loginResult).toBe(true)
      expect(result.current.user).toEqual(mockUser)
      expect(localStorage.getItem('auth:user')).toBe(JSON.stringify(mockUser))
    })

    it('uses email as fallback user when API response has no user object', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200, data: {} })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      await result.current.login('fallback@example.com', 'password')

      await waitFor(() => expect(result.current.user).toEqual({ email: 'fallback@example.com' }))
    })

    it('returns false on failed login and does not set authenticated', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: false, status: 401 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const loginResult = await result.current.login('wrong@example.com', 'wrong')

      expect(loginResult).toBe(false)
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
    })

    it('includes application code and skipUnauthorized in login request', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: false, status: 401 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      await result.current.login('user@test.com', 'pass')

      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/login`,
        expect.objectContaining({ email: 'user@test.com', password: 'pass', applicationCode: expect.any(String) }),
        { skipUnauthorized: true }
      )
    })
  })

  describe('logout', () => {
    it('clears user state, localStorage, and calls logout endpoint', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200, data: { user: { email: 'test@test.com' } } })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      await result.current.login('test@test.com', 'password')
      await waitFor(() => expect(result.current.isAuthenticated).toBe(true))

      act(() => { result.current.logout() })

      await waitFor(() => expect(result.current.isAuthenticated).toBe(false))
      expect(result.current.user).toBeNull()
      expect(localStorage.getItem('auth:user')).toBeNull()
    })
  })

  describe('register', () => {
    it('successfully registers a user', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 201 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const registerResult = await result.current.register('John', 'Doe', 'john@example.com')

      expect(registerResult).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/register`,
        expect.objectContaining({ firstName: 'John', lastName: 'Doe', email: 'john@example.com', applicationCode: expect.any(String) }),
        { skipUnauthorized: true }
      )
    })

    it('returns false on failed registration', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: false, status: 400 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const registerResult = await result.current.register('John', 'Doe', 'existing@example.com')

      expect(registerResult).toBe(false)
    })
  })

  describe('changePassword', () => {
    it('successfully changes password', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const changeResult = await result.current.changePassword('oldpass', 'newpass', 'newpass')

      expect(changeResult).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/change-password`,
        { email: undefined, oldPassword: 'oldpass', newPassword: 'newpass', confirmPassword: 'newpass' },
        { skipUnauthorized: true }
      )
    })

    it('returns false when passwords do not match', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const changeResult = await result.current.changePassword('oldpass', 'newpass1', 'newpass2')

      expect(changeResult).toBe(false)
      expect(api.post).not.toHaveBeenCalled()
    })

    it('returns false when old password is empty', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const changeResult = await result.current.changePassword('', 'newpass', 'newpass')

      expect(changeResult).toBe(false)
    })
  })

  describe('resetPassword', () => {
    it('successfully resets password', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const resetResult = await result.current.resetPassword('user@test.com', 'verification-hash', 'newpass', 'newpass')

      expect(resetResult).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/change-password-reset`,
        { email: 'user@test.com', verificationHash: 'verification-hash', newPassword: 'newpass', confirmPassword: 'newpass' },
        { skipUnauthorized: true }
      )
    })

    it('returns false when passwords do not match', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const resetResult = await result.current.resetPassword('user@test.com', 'hash', 'pass1', 'pass2')

      expect(resetResult).toBe(false)
      expect(api.post).not.toHaveBeenCalled()
    })

    it('returns false when required fields are missing', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const resetResult = await result.current.resetPassword('', 'hash', 'pass', 'pass')

      expect(resetResult).toBe(false)
    })
  })

  describe('requestPasswordReset', () => {
    it('successfully requests password reset', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200 })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const requestResult = await result.current.requestPasswordReset!('user@test.com')

      expect(requestResult).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/request-password-reset`,
        { email: 'user@test.com' },
        { skipUnauthorized: true }
      )
    })

    it('returns false when email is empty', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      const requestResult = await result.current.requestPasswordReset!('')

      expect(requestResult).toBe(false)
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
      vi.mocked(api.post).mockResolvedValueOnce({ ok: true, status: 200, data: { user: { email: 'test@example.com' } } })

      const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      await result.current.login('test@example.com', 'password123')
      await waitFor(() => expect(result.current.isAuthenticated).toBe(true))

      const unauthorizedCallback = (api.onUnauthorized as ReturnType<typeof vi.fn>).mock.calls[0][0]
      act(() => { unauthorizedCallback() })

      await waitFor(() => expect(result.current.isAuthenticated).toBe(false))
      expect(result.current.user).toBeNull()
      expect(localStorage.getItem('auth:user')).toBeNull()
      expect(mockLocationAssign).toHaveBeenCalledWith('/login')
    })
  })
})
