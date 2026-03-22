import { describe, it, expect, vi, beforeEach } from 'vitest'
import { act } from 'react'
import { render, renderHook } from '@testing-library/react'
import { AuthProvider, useAuth } from '@/auth/AuthContext'
import * as api from '@/api'

// Mock the API module
vi.mock('@/api', () => ({
  post: vi.fn(),
  setAuthToken: vi.fn(),
  onUnauthorized: vi.fn()
}))

// Mock window.location.assign
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
    mockLocationAssign.mockClear()
  })

  describe('AuthProvider initialization', () => {
    it('initializes with no user when localStorage is empty', () => {
      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
      expect(result.current.token).toBeNull()
    })

    it('restores valid token from localStorage', () => {
      // Create a valid JWT token (not expired)
      const futureTimestamp = Math.floor(Date.now() / 1000) + 3600 // 1 hour from now
      const payload = { email: 'test@test.com', exp: futureTimestamp }
      const encodedPayload = btoa(JSON.stringify(payload))
      const mockToken = `header.${encodedPayload}.signature`

      localStorage.setItem('auth:token', mockToken)
      localStorage.setItem('auth:user', JSON.stringify({ email: 'test@test.com' }))

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.user).toEqual({ email: 'test@test.com' })
      expect(result.current.token).toBe(mockToken)
    })

    it('clears expired token from localStorage', () => {
      // Create an expired JWT token
      const pastTimestamp = Math.floor(Date.now() / 1000) - 3600 // 1 hour ago
      const payload = { email: 'test@test.com', exp: pastTimestamp }
      const encodedPayload = btoa(JSON.stringify(payload))
      const mockToken = `header.${encodedPayload}.signature`

      localStorage.setItem('auth:token', mockToken)
      localStorage.setItem('auth:user', JSON.stringify({ email: 'test@test.com' }))

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
      expect(localStorage.getItem('auth:token')).toBeNull()
      expect(localStorage.getItem('auth:user')).toBeNull()
    })

    it('extracts user email from token if user data missing', () => {
      const futureTimestamp = Math.floor(Date.now() / 1000) + 3600
      const payload = { email: 'from-token@test.com', exp: futureTimestamp }
      const encodedPayload = btoa(JSON.stringify(payload))
      const mockToken = `header.${encodedPayload}.signature`

      localStorage.setItem('auth:token', mockToken)
      // No user data in localStorage

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(result.current.user).toEqual({ email: 'from-token@test.com' })
    })

    it('calls setAuthToken on mount', () => {
      renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(api.setAuthToken).toHaveBeenCalled()
    })
  })

  describe('login', () => {
    it('successfully logs in and stores token', async () => {
      const mockToken = 'mock-jwt-token'
      const mockUser = { email: 'user@example.com' }

      vi.mocked(api.post).mockResolvedValueOnce({
        ok: true,
        status: 200,
        data: { token: mockToken, user: mockUser }
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let loginResult: boolean = false
      await act(async () => {
        loginResult = await result.current.login('user@example.com', 'password')
      })

      expect(loginResult).toBe(true)
      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.user).toEqual(mockUser)
      expect(result.current.token).toBe(mockToken)
      expect(localStorage.getItem('auth:token')).toBe(mockToken)
      expect(api.setAuthToken).toHaveBeenCalledWith(mockToken)
    })

    it('uses email as fallback when user not returned', async () => {
      const mockToken = 'mock-jwt-token'

      vi.mocked(api.post).mockResolvedValueOnce({
        ok: true,
        status: 200,
        data: { token: mockToken }
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      await act(async () => {
        await result.current.login('fallback@example.com', 'password')
      })

      expect(result.current.user).toEqual({ email: 'fallback@example.com' })
    })

    it('returns false on failed login', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({
        ok: false,
        status: 401,
        error: 'Invalid credentials'
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let loginResult: boolean = true
      await act(async () => {
        loginResult = await result.current.login('wrong@example.com', 'wrong')
      })

      expect(loginResult).toBe(false)
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
    })

    it('includes application code in login request', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({
        ok: false,
        status: 401
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      await act(async () => {
        await result.current.login('user@test.com', 'pass')
      })

      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/login`,
        expect.objectContaining({
          email: 'user@test.com',
          password: 'pass',
          applicationCode: expect.any(String)
        })
      )
    })
  })

  describe('logout', () => {
    it('clears user and token', async () => {
      const mockToken = 'mock-jwt-token'
      vi.mocked(api.post).mockResolvedValueOnce({
        ok: true,
        status: 200,
        data: { token: mockToken, user: { email: 'test@test.com' } }
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      await act(async () => {
        await result.current.login('test@test.com', 'password')
      })

      expect(result.current.isAuthenticated).toBe(true)

      act(() => {
        result.current.logout()
      })

      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
      expect(result.current.token).toBeNull()
      expect(localStorage.getItem('auth:token')).toBeNull()
      expect(localStorage.getItem('auth:user')).toBeNull()
    })
  })

  describe('register', () => {
    it('successfully registers a user', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({
        ok: true,
        status: 201
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let registerResult: boolean = false
      await act(async () => {
        registerResult = await result.current.register('John', 'Doe', 'john@example.com')
      })

      expect(registerResult).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/register`,
        expect.objectContaining({
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          applicationCode: expect.any(String)
        })
      )
    })

    it('returns false on failed registration', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({
        ok: false,
        status: 400,
        error: 'Email already exists'
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let registerResult: boolean = true
      await act(async () => {
        registerResult = await result.current.register('John', 'Doe', 'existing@example.com')
      })

      expect(registerResult).toBe(false)
    })
  })

  describe('changePassword', () => {
    it('successfully changes password', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({
        ok: true,
        status: 200
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let changeResult: boolean = false
      await act(async () => {
        changeResult = await result.current.changePassword('oldpass', 'newpass', 'newpass')
      })

      expect(changeResult).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/change-password`,
        {
          email: undefined,
          oldPassword: 'oldpass',
          newPassword: 'newpass',
          confirmPassword: 'newpass'
        }
      )
    })

    it('returns false when passwords do not match', async () => {
      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let changeResult: boolean = true
      await act(async () => {
        changeResult = await result.current.changePassword('oldpass', 'newpass1', 'newpass2')
      })

      expect(changeResult).toBe(false)
      expect(api.post).not.toHaveBeenCalled()
    })

    it('returns false when old password is empty', async () => {
      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let changeResult: boolean = true
      await act(async () => {
        changeResult = await result.current.changePassword('', 'newpass', 'newpass')
      })

      expect(changeResult).toBe(false)
    })
  })

  describe('resetPassword', () => {
    it('successfully resets password', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({
        ok: true,
        status: 200
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let resetResult: boolean = false
      await act(async () => {
        resetResult = await result.current.resetPassword(
          'user@test.com',
          'verification-hash',
          'newpass',
          'newpass'
        )
      })

      expect(resetResult).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/change-password-reset`,
        {
          email: 'user@test.com',
          verificationHash: 'verification-hash',
          newPassword: 'newpass',
          confirmPassword: 'newpass'
        }
      )
    })

    it('returns false when passwords do not match', async () => {
      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let resetResult: boolean = true
      await act(async () => {
        resetResult = await result.current.resetPassword(
          'user@test.com',
          'hash',
          'pass1',
          'pass2'
        )
      })

      expect(resetResult).toBe(false)
      expect(api.post).not.toHaveBeenCalled()
    })

    it('returns false when required fields are missing', async () => {
      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let resetResult: boolean = true
      await act(async () => {
        resetResult = await result.current.resetPassword('', 'hash', 'pass', 'pass')
      })

      expect(resetResult).toBe(false)
    })
  })

  describe('requestPasswordReset', () => {
    it('successfully requests password reset', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({
        ok: true,
        status: 200
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let requestResult: boolean = false
      await act(async () => {
        requestResult = await result.current.requestPasswordReset!('user@test.com')
      })

      expect(requestResult).toBe(true)
      expect(api.post).toHaveBeenCalledWith(
        `${AUTH_BASE}/request-password-reset`,
        { email: 'user@test.com' }
      )
    })

    it('returns false when email is empty', async () => {
      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      let requestResult: boolean = true
      await act(async () => {
        requestResult = await result.current.requestPasswordReset!('')
      })

      expect(requestResult).toBe(false)
      expect(api.post).not.toHaveBeenCalled()
    })
  })

  describe('useAuth hook', () => {
    it('throws error when used outside AuthProvider', () => {
      const TestComponent = () => {
        useAuth()
        return null
      }

      expect(() => render(<TestComponent />)).toThrow(
        'useAuth must be used within AuthProvider'
      )
    })
  })

  describe('unauthorized handler', () => {
    it('sets up unauthorized handler on mount', () => {
      renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(api.onUnauthorized).toHaveBeenCalled()
    })

    it('clears auth state and redirects when unauthorized handler is invoked', async () => {
      // Setup: authenticate first
      vi.mocked(api.post).mockResolvedValueOnce({
        ok: true,
        status: 200,
        data: {
          token: 'test-jwt-token',
          user: { id: 1, username: 'testuser', email: 'test@example.com' }
        }
      })

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      // Login to set auth state
      await act(async () => {
        await result.current.login('test@example.com', 'password123')
      })

      // Verify user is authenticated
      expect(result.current.isAuthenticated).toBe(true)

      // Capture the callback passed to onUnauthorized
      expect(api.onUnauthorized).toHaveBeenCalled()
      const unauthorizedCallback = (api.onUnauthorized as ReturnType<typeof vi.fn>).mock.calls[0][0]

      // Invoke the unauthorized handler
      act(() => {
        unauthorizedCallback()
      })

      // Verify auth state is cleared
      expect(api.setAuthToken).toHaveBeenCalledWith(null)
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()

      // Verify redirect to login
      expect(mockLocationAssign).toHaveBeenCalledWith('/login')
    })
  })

  describe('JWT decoding', () => {
    it('handles invalid JWT format gracefully', () => {
      localStorage.setItem('auth:token', 'invalid-token')

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.token).toBeNull()
    })

    it('handles malformed JWT payload', () => {
      const mockToken = 'header.invalid-base64!@#.signature'
      localStorage.setItem('auth:token', mockToken)

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(result.current.isAuthenticated).toBe(false)
    })

    it('extracts user from sub field when email is absent in token', () => {
      const futureTimestamp = Math.floor(Date.now() / 1000) + 3600
      const payload = { sub: 'sub@test.com', exp: futureTimestamp }
      const encodedPayload = btoa(JSON.stringify(payload))
      const mockToken = `header.${encodedPayload}.signature`

      localStorage.setItem('auth:token', mockToken)
      // No user in localStorage so it falls back to decoding the token

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(result.current.user).toEqual({ email: 'sub@test.com' })
    })

    it('returns null user when valid token has no email or sub field', () => {
      const futureTimestamp = Math.floor(Date.now() / 1000) + 3600
      const payload = { role: 'user', exp: futureTimestamp } // no email, no sub
      const encodedPayload = btoa(JSON.stringify(payload))
      const mockToken = `header.${encodedPayload}.signature`

      localStorage.setItem('auth:token', mockToken)
      // No user in localStorage so it falls back to decoding the token

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(result.current.isAuthenticated).toBe(true) // token is valid
      expect(result.current.user).toBeNull()            // but no email to extract
    })

    it('handles JWT payload with base64 length mod 4 equal to 1', () => {
      // A single-character base64url payload triggers the pad === 1 branch in decodeJwtPayload.
      // atob will fail for this degenerate input and the function returns null (token rejected).
      const mockToken = 'header.A.signature'
      localStorage.setItem('auth:token', mockToken)

      const { result } = renderHook(() => useAuth(), {
        wrapper: AuthProvider
      })

      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.token).toBeNull()
    })
  })
})
