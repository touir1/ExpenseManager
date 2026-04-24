import { createContext, useContext, useEffect, useMemo, useState, ReactNode } from 'react'
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

export type { AuthContextValue }

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [user, setUser] = useState<User | null>(null)

  const APPLICATION_CODE = import.meta.env.VITE_APPLICATION_CODE ?? 'EXPENSES_MANAGER'

  useEffect(() => {
    const storedUser = localStorage.getItem('auth:user')
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
        }
      } else {
        localStorage.removeItem('auth:user')
      }
      setIsLoading(false)
    })
  }, [])

  onUnauthorized(() => {
    setUser(null)
    setIsAuthenticated(false)
    localStorage.removeItem('auth:user')
    window.location.assign('/login')
  })

  const login: AuthContextValue['login'] = async (email, password) => {
    const { ok, data } = await loginRequest(email, password, APPLICATION_CODE)
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
    logoutRequest().catch(() => {})
    setUser(null)
    setIsAuthenticated(false)
    localStorage.removeItem('auth:user')
  }

  const register: AuthContextValue['register'] = async (firstName, lastName, email) => {
    const { ok } = await registerRequest(firstName, lastName, email, APPLICATION_CODE)
    return ok
  }

  const changePassword: AuthContextValue['changePassword'] = async (oldPassword, newPassword, repeatPassword) => {
    if (!oldPassword || !newPassword || newPassword !== repeatPassword) return false
    const { ok } = await changePasswordRequest(user?.email, oldPassword, newPassword, repeatPassword)
    return ok
  }

  const resetPassword: AuthContextValue['resetPassword'] = async (email, verificationHash, newPassword, repeatPassword) => {
    if (!email || !verificationHash || !newPassword || newPassword !== repeatPassword) return false
    const { ok } = await resetPasswordRequest(email, verificationHash, newPassword, repeatPassword)
    return ok
  }

  const requestPasswordReset: NonNullable<AuthContextValue['requestPasswordReset']> = async (email) => {
    if (!email) return false
    const { ok } = await requestPasswordResetRequest(email)
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
