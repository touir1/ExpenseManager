import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useAuth } from '@/features/auth/AuthContext'
import { getCategories } from '@/features/expenses/services/categoriesApi.service'
import { getCurrencies } from '@/features/expenses/services/currenciesApi.service'
import type { Category, Currency } from '@/features/expenses/types/expenses.type'

type ExpensesDataContextValue = {
  categories: Category[]
  currencies: Currency[]
  isLoading: boolean
  refresh: () => void
}

const ExpensesDataContext = createContext<ExpensesDataContextValue | undefined>(undefined)

export function ExpensesDataProvider({ children }: Readonly<{ children: ReactNode }>) {
  const { isAuthenticated } = useAuth()
  const [categories, setCategories] = useState<Category[]>([])
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [isLoading, setIsLoading] = useState(false)

  const load = useCallback(async () => {
    setIsLoading(true)
    const [catsRes, cursRes] = await Promise.all([getCategories(), getCurrencies()])
    if (catsRes.ok && catsRes.data) setCategories(catsRes.data)
    if (cursRes.ok && cursRes.data) setCurrencies(cursRes.data)
    setIsLoading(false)
  }, [])

  useEffect(() => {
    if (isAuthenticated) {
      load()
    } else {
      setCategories([])
      setCurrencies([])
    }
  }, [isAuthenticated, load])

  const value = useMemo(
    () => ({ categories, currencies, isLoading, refresh: load }),
    [categories, currencies, isLoading, load]
  )

  return <ExpensesDataContext.Provider value={value}>{children}</ExpensesDataContext.Provider>
}

export function useExpensesData(): ExpensesDataContextValue {
  const ctx = useContext(ExpensesDataContext)
  if (!ctx) throw new Error('useExpensesData must be used within ExpensesDataProvider')
  return ctx
}
