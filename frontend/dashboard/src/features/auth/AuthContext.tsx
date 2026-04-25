import { createContext, useCallback, useContext, useEffect, useMemo, useState, ReactNode } from 'react'
import { onUnauthorized } from '@/services/api'
import {
  sessionCheck,
  loginRequest,
  logoutRequest,
  registerRequest,
  changePasswordRequest,
  resetPasswordRequest,
  requestPasswordResetRequest,
} from '@/services/authApi'
import type { AuthContextValue, User } from '@/types/auth'

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

const APPLICATION_CODE = import.meta.env.VITE_APPLICATION_CODE ?? 'EXPENSES_MANAGER'

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [user, setUser] = useState<User | null>(null)

  useEffect(() => {
    const storedUser = localStorage.getItem('auth:user') ?? sessionStorage.getItem('auth:user')
    if (!storedUser) {
      setIsLoading(false)
      return
    }
    sessionCheck().then(({ ok }) => {
      if (ok) {
        try {
          setUser(JSON.parse(storedUser))
          setIsAuthenticated(true)
        } catch {
          localStorage.removeItem('auth:user')
          sessionStorage.removeItem('auth:user')
        }
      } else {
        localStorage.removeItem('auth:user')
        sessionStorage.removeItem('auth:user')
      }
      setIsLoading(false)
    })
  }, [])

  useEffect(() => {
    onUnauthorized(() => {
      setUser(null)
      setIsAuthenticated(false)
      localStorage.removeItem('auth:user')
      sessionStorage.removeItem('auth:user')
      window.location.assign('/login')
    })
    return () => onUnauthorized(null)
  }, [])

  const login = useCallback<AuthContextValue['login']>(async (email, password, rememberMe = false) => {
    const { ok, data } = await loginRequest(email, password, APPLICATION_CODE)
    if (ok) {
      const userData = data?.user ?? { email }
      setUser(userData)
      setIsAuthenticated(true)
      const storage = rememberMe ? localStorage : sessionStorage
      storage.setItem('auth:user', JSON.stringify(userData))
      return true
    }
    return false
  }, [])

  const logout = useCallback(() => {
    logoutRequest().catch(() => {})
    setUser(null)
    setIsAuthenticated(false)
    localStorage.removeItem('auth:user')
    sessionStorage.removeItem('auth:user')
  }, [])

  const register = useCallback<AuthContextValue['register']>(async (firstName, lastName, email) => {
    const { ok } = await registerRequest(firstName, lastName, email, APPLICATION_CODE)
    return ok
  }, [])

  const changePassword = useCallback<AuthContextValue['changePassword']>(async (oldPassword, newPassword, repeatPassword) => {
    if (!oldPassword || !newPassword || newPassword !== repeatPassword) return false
    const { ok } = await changePasswordRequest(user?.email, oldPassword, newPassword, repeatPassword)
    return ok
  }, [user])

  const resetPassword = useCallback<AuthContextValue['resetPassword']>(async (email, verificationHash, newPassword, repeatPassword) => {
    if (!email || !verificationHash || !newPassword || newPassword !== repeatPassword) return false
    const { ok } = await resetPasswordRequest(email, verificationHash, newPassword, repeatPassword)
    return ok
  }, [])

  const requestPasswordReset = useCallback<NonNullable<AuthContextValue['requestPasswordReset']>>(async (email) => {
    if (!email) return false
    const { ok } = await requestPasswordResetRequest(email)
    return ok
  }, [])

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
  }), [isAuthenticated, isLoading, user, login, logout, register, changePassword, resetPassword, requestPasswordReset])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
