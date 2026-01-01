import { createContext, useContext, useEffect, useMemo, useState, ReactNode } from 'react'
import { post, setAuthToken, onUnauthorized } from '@/api'

export type AuthContextValue = {
  isAuthenticated: boolean
  user: { email: string } | null
  token?: string | null
  login: (email: string, password: string) => Promise<boolean>
  logout: () => void
  register: (firstName: string, lastName: string, email: string) => Promise<boolean>
  changePassword: (oldPassword: string, newPassword: string, repeatPassword: string) => Promise<boolean>
  resetPassword: (email: string, verificationHash: string, newPassword: string, repeatPassword: string) => Promise<boolean>
  requestPasswordReset?: (email: string) => Promise<boolean>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  // JWT token stored in memory for now (no persistence)
  const [token, setToken] = useState<string | null>(() => {
    const t = localStorage.getItem('auth:token')
    if (!t) return null
    const payload = decodeJwtPayload(t)
    if (!payload || (payload.exp && payload.exp * 1000 <= Date.now())) {
      localStorage.removeItem('auth:token')
      localStorage.removeItem('auth:user')
      return null
    }
    return t
  })
  const [user, setUser] = useState<{ email: string } | null>(() => {
    const u = localStorage.getItem('auth:user')
    if (u) {
      try { return JSON.parse(u) } catch { /* ignore */ }
    }
    const t = localStorage.getItem('auth:token')
    if (t) {
      const p = decodeJwtPayload(t)
      const email = (p && (p.email || p.sub)) as string | undefined
      if (email) return { email }
    }
    return null
  })

  // Application code to identify this app instance when registering
  const APPLICATION_CODE = import.meta.env.VITE_APPLICATION_CODE ?? 'EXPENSES_MANAGER'

  const isAuthenticated = !!token

  // Configure API client unauthorized handler
  onUnauthorized(() => {
    setAuthToken(null)
    setToken(null)
    setUser(null)
    // redirect happens in api.ts if no handler, but we ensure state is clean
    window.location.assign('/login')
  })

  // Keep API client token in sync and persist in localStorage
  useEffect(() => {
    setAuthToken(token ?? null)
    if (token) {
      localStorage.setItem('auth:token', token)
    } else {
      localStorage.removeItem('auth:token')
    }
    if (user) {
      localStorage.setItem('auth:user', JSON.stringify(user))
    } else {
      localStorage.removeItem('auth:user')
    }
  }, [token, user])

  const login: AuthContextValue['login'] = async (email, password) => {
    const { ok, data } = await post<{ token: string; user?: { email: string } }>(
      '/api/auth/login',
      { email, password, applicationCode: APPLICATION_CODE }
    )
    if (ok && data?.token) {
      setToken(data.token)
      setAuthToken(data.token)
      setUser(data.user ?? { email })
      return true
    }
    return false
  }

  const logout = () => {
    setToken(null)
    setUser(null)
    localStorage.removeItem('auth:token')
    localStorage.removeItem('auth:user')
  }

  const register: AuthContextValue['register'] = async (firstName, lastName, email) => {
    const { ok } = await post<unknown>('/api/auth/register', {
      firstName,
      lastName,
      email,
      applicationCode: APPLICATION_CODE,
    })
    return ok
  }

  const changePassword: AuthContextValue['changePassword'] = async (oldPassword, newPassword, repeatPassword) => {
    if (!oldPassword || !newPassword || newPassword !== repeatPassword) return false
    const { ok } = await post<unknown>('/api/auth/change-password', { oldPassword, newPassword, confirmPassword: repeatPassword })
    return ok
  }

  const resetPassword: AuthContextValue['resetPassword'] = async (email, verificationHash, newPassword, repeatPassword) => {
    if (!email || !verificationHash || !newPassword || newPassword !== repeatPassword) return false
    const { ok } = await post<unknown>('/api/auth/change-password-reset', { email, verificationHash, newPassword, confirmPassword: repeatPassword })
    return ok
  }

  const requestPasswordReset: NonNullable<AuthContextValue['requestPasswordReset']> = async (email: string) => {
    if (!email) return false
    const { ok } = await post<unknown>('/api/auth/request-password-reset', { email })
    return ok
  }

  const value = useMemo<AuthContextValue>(() => ({
    isAuthenticated,
    user,
    token,
    login,
    logout,
    register,
    changePassword,
    resetPassword,
    requestPasswordReset,
  }), [isAuthenticated, user, token])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}

function decodeJwtPayload(token: string): any | null {
  const parts = token.split('.')
  if (parts.length < 2) return null
  try {
    let payload = parts[1]
    payload = payload.replace(/-/g, '+').replace(/_/g, '/')
    const pad = payload.length % 4
    if (pad === 2) payload += '=='
    else if (pad === 3) payload += '='
    else if (pad === 1) payload += '=== '
    const json = atob(payload)
    return JSON.parse(json)
  } catch {
    return null
  }
}
