import { createContext, useCallback, useContext, useEffect, useMemo, useState, ReactNode } from 'react'
import { onUnauthorized } from '@/services/api.service'
import {
  sessionCheck,
  refreshRequest,
  loginRequest,
  logoutRequest,
  registerRequest,
  changePasswordRequest,
  resetPasswordRequest,
  requestPasswordResetRequest,
} from '@/features/auth/services/authApi.service'
import type { AuthContextValue, User } from '@/features/auth/types/auth.type'

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

/* c8 ignore next */
const APPLICATION_CODE = import.meta.env.VITE_APPLICATION_CODE ?? 'EXPENSES_MANAGER'

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [user, setUser] = useState<User | null>(null)

  useEffect(() => {
    async function restore() {
      const res = await sessionCheck()
      if (res.ok && res.data) {
        setUser(res.data)
        setIsAuthenticated(true)
        setIsLoading(false)
        return
      }
      // Access token may be expired — try refresh token
      const refreshed = await refreshRequest()
      if (refreshed.ok) {
        const retry = await sessionCheck()
        if (retry.ok && retry.data) {
          setUser(retry.data)
          setIsAuthenticated(true)
        }
      }
      setIsLoading(false)
    }
    restore()
  }, [])

  useEffect(() => {
    onUnauthorized(() => {
      setUser(null)
      setIsAuthenticated(false)
      window.location.assign('/login')
    })
    return () => onUnauthorized(null)
  }, [])

  const login = useCallback<AuthContextValue['login']>(async (email, password, rememberMe = false) => {
    const res = await loginRequest(email, password, APPLICATION_CODE, rememberMe)
    if (res.ok) {
      const userData = res.data?.user ?? { email }
      setUser(userData)
      setIsAuthenticated(true)
      return { ok: true }
    }
    return { ok: false, error: res.error }
  }, [])

  const logout = useCallback(() => {
    logoutRequest().catch(() => {})
    setUser(null)
    setIsAuthenticated(false)
  }, [])

  const register = useCallback<AuthContextValue['register']>(async (firstName, lastName, email) => {
    const { ok, error } = await registerRequest(firstName, lastName, email, APPLICATION_CODE)
    return { ok, error }
  }, [])

  const changePassword = useCallback<AuthContextValue['changePassword']>(async (oldPassword, newPassword, repeatPassword) => {
    if (!oldPassword || !newPassword || newPassword !== repeatPassword) return { ok: false }
    const { ok, error } = await changePasswordRequest(user?.email, oldPassword, newPassword, repeatPassword)
    return { ok, error }
  }, [user])

  const resetPassword = useCallback<AuthContextValue['resetPassword']>(async (email, verificationHash, newPassword, repeatPassword) => {
    if (!email || !verificationHash || !newPassword || newPassword !== repeatPassword) return { ok: false }
    const { ok, error } = await resetPasswordRequest(email, verificationHash, newPassword, repeatPassword)
    return { ok, error }
  }, [])

  const requestPasswordReset = useCallback<NonNullable<AuthContextValue['requestPasswordReset']>>(async (email) => {
    if (!email) return { ok: false }
    const { ok, error } = await requestPasswordResetRequest(email, APPLICATION_CODE)
    return { ok, error }
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
