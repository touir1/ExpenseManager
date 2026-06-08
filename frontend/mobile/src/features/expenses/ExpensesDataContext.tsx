import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useAuth } from '@/features/auth/AuthContext'
import { getCategories } from '@/features/expenses/services/categoriesApi.service'
import { getCurrencies } from '@/features/expenses/services/currenciesApi.service'
import { getTags } from '@/features/tags/services/tagsApi.service'
import type { Category, Currency } from '@/features/expenses/types/expenses.type'
import type { Tag } from '@/features/tags/types/tag.type'

type ExpensesDataContextValue = {
  categories: Category[]
  currencies: Currency[]
  tags: Tag[]
  isLoading: boolean
  refresh: () => void
}

const ExpensesDataContext = createContext<ExpensesDataContextValue | undefined>(undefined)

export function ExpensesDataProvider({ children }: Readonly<{ children: ReactNode }>) {
  const { isAuthenticated } = useAuth()
  const [categories, setCategories] = useState<Category[]>([])
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [tags, setTags] = useState<Tag[]>([])
  const [isLoading, setIsLoading] = useState(false)

  const load = useCallback(async () => {
    setIsLoading(true)
    const [catsRes, cursRes, tagsRes] = await Promise.all([getCategories(), getCurrencies(), getTags()])
    if (catsRes.ok && catsRes.data) setCategories(catsRes.data)
    if (cursRes.ok && cursRes.data) setCurrencies(cursRes.data)
    if (tagsRes.ok && tagsRes.data) {
      const seen = new Set<number>()
      const all: Tag[] = []
      for (const t of [...tagsRes.data.own, ...tagsRes.data.family]) {
        if (!seen.has(t.id)) { seen.add(t.id); all.push(t) }
      }
      setTags(all)
    }
    setIsLoading(false)
  }, [])

  useEffect(() => {
    if (isAuthenticated) {
      load()
    } else {
      setCategories([])
      setCurrencies([])
      setTags([])
    }
  }, [isAuthenticated, load])

  const value = useMemo(
    () => ({ categories, currencies, tags, isLoading, refresh: load }),
    [categories, currencies, tags, isLoading, load],
  )

  return <ExpensesDataContext.Provider value={value}>{children}</ExpensesDataContext.Provider>
}

export function useExpensesData(): ExpensesDataContextValue {
  const ctx = useContext(ExpensesDataContext)
  if (!ctx) throw new Error('useExpensesData must be used within ExpensesDataProvider')
  return ctx
}
