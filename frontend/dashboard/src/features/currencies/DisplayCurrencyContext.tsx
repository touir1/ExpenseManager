import { createContext, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@/features/auth/AuthContext'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { getConfig } from '@/features/settings/services/userConfigApi.service'

type DisplayCurrencyContextValue = {
  displayCurrencyId: number | null
  setDisplayCurrencyId: (id: number | null) => void
}

const DisplayCurrencyContext = createContext<DisplayCurrencyContextValue | undefined>(undefined)

const EUR_CODE = 'EUR'

export function DisplayCurrencyProvider({ children }: Readonly<{ children: ReactNode }>) {
  const { isAuthenticated } = useAuth()
  const { currencies } = useExpensesData()
  const [displayCurrencyId, setDisplayCurrencyId] = useState<number | null>(null)
  const initialized = useRef(false)

  const { data: configData } = useQuery({
    queryKey: ['userConfig'],
    queryFn: async () => {
      const res = await getConfig()
      return res.ok ? res.data ?? null : null
    },
    enabled: isAuthenticated,
  })

  useEffect(() => {
    if (initialized.current) return
    if (!isAuthenticated || currencies.length === 0 || configData === undefined) return

    const fromConfig = configData?.defaultCurrencyId ?? null
    if (fromConfig && currencies.some(c => c.id === fromConfig)) {
      setDisplayCurrencyId(fromConfig)
    } else {
      const eur = currencies.find(c => c.code === EUR_CODE)
      setDisplayCurrencyId(eur?.id ?? currencies[0].id)
    }
    initialized.current = true
  }, [isAuthenticated, currencies, configData])

  useEffect(() => {
    if (!isAuthenticated) {
      initialized.current = false
      setDisplayCurrencyId(null)
    }
  }, [isAuthenticated])

  const value = useMemo(
    () => ({ displayCurrencyId, setDisplayCurrencyId }),
    [displayCurrencyId, setDisplayCurrencyId]
  )

  return <DisplayCurrencyContext.Provider value={value}>{children}</DisplayCurrencyContext.Provider>
}

export function useDisplayCurrency(): DisplayCurrencyContextValue {
  const ctx = useContext(DisplayCurrencyContext)
  if (!ctx) throw new Error('useDisplayCurrency must be used within DisplayCurrencyProvider')
  return ctx
}
