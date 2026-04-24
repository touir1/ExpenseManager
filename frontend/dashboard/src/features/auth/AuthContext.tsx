import { createContext, useContext, useEffect, useMemo, useState, ReactNode } from 'react'
import { post, get, onUnauthorized } from '@/services/api'

export type AuthContextValue = {
  isAuthenticated: boolean
  isLoading: boolean
  user: { email: string; firstName?: string } | null
  login: (email: string, password: string) => Promise<boolean>
  logout: () => void
  register: (firstName: string, lastName: string, email: string) => Promise<boolean>
  changePassword: (oldPassword: string, newPassword: string, repeatPassword: string) => Promise<boolean>
  resetPassword: (email: string, verificationHash: string, newPassword: string, repeatPassword: string) => Promise<boolean>
  requestPasswordReset?: (email: string) => Promise<boolean>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)
const AUTH_BASE = '/api/users/auth'

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [user, setUser] = useState<{ email: string; firstName?: string } | null>(null)

  const APPLICATION_CODE = import.meta.env.VITE_APPLICATION_CODE ?? 'EXPENSES_MANAGER'

  // On mount: restore session from HttpOnly cookie, using stored user info for display
  useEffect(() => {
    const storedUser = localStorage.getItem('auth:user')
    if (!storedUser) {
      setIsLoading(false)
      return
    }
    get<void>(`${AUTH_BASE}/session`).then(({ ok }) => {
      if (ok) {
        try {
          setUser(JSON.parse(storedUser))
          setIsAuthenticated(true)
        } catch {
          localStorage.removeItem('auth:user')
        }
      } else {
        localStorage.removeItem('auth:user')
      }
      setIsLoading(false)
    })
  }, [])

  // Handle session expiry (401 on authenticated endpoints)
  onUnauthorized(() => {
    setUser(null)
    setIsAuthenticated(false)
    localStorage.removeItem('auth:user')
    window.location.assign('/login')
  })

  const login: AuthContextValue['login'] = async (email, password) => {
    const { ok, data } = await post<{ user?: { email: string; firstName?: string } }>(
      `${AUTH_BASE}/login`,
      { email, password, applicationCode: APPLICATION_CODE },
      { skipUnauthorized: true }
    )
    if (ok) {
      const userData = data?.user ?? { email }
      setUser(userData)
      setIsAuthenticated(true)
      localStorage.setItem('auth:user', JSON.stringify(userData))
      return true
    }
    return false
  }

  const logout = () => {
    post<unknown>(`${AUTH_BASE}/logout`, {}, { skipUnauthorized: true }).catch(() => {})
    setUser(null)
    setIsAuthenticated(false)
    localStorage.removeItem('auth:user')
  }

  const register: AuthContextValue['register'] = async (firstName, lastName, email) => {
    const { ok } = await post<unknown>(`${AUTH_BASE}/register`, {
      firstName,
      lastName,
      email,
      applicationCode: APPLICATION_CODE,
    }, { skipUnauthorized: true })
    return ok
  }

  const changePassword: AuthContextValue['changePassword'] = async (oldPassword, newPassword, repeatPassword) => {
    if (!oldPassword || !newPassword || newPassword !== repeatPassword) return false
    const { ok } = await post<unknown>(`${AUTH_BASE}/change-password`, { email: user?.email, oldPassword, newPassword, confirmPassword: repeatPassword }, { skipUnauthorized: true })
    return ok
  }

  const resetPassword: AuthContextValue['resetPassword'] = async (email, verificationHash, newPassword, repeatPassword) => {
    if (!email || !verificationHash || !newPassword || newPassword !== repeatPassword) return false
    const { ok } = await post<unknown>(`${AUTH_BASE}/change-password-reset`, { email, verificationHash, newPassword, confirmPassword: repeatPassword }, { skipUnauthorized: true })
    return ok
  }

  const requestPasswordReset: NonNullable<AuthContextValue['requestPasswordReset']> = async (email: string) => {
    if (!email) return false
    const { ok } = await post<unknown>(`${AUTH_BASE}/request-password-reset`, { email }, { skipUnauthorized: true })
    return ok
  }

  const value = useMemo<AuthContextValue>(() => ({
    isAuthenticated,
    isLoading,
    user,
    login,
    logout,
    register,
    changePassword,
    resetPassword,
    requestPasswordReset,
  }), [isAuthenticated, isLoading, user])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
