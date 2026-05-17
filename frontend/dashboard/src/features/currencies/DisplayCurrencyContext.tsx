import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react'

type DisplayCurrencyContextValue = {
  displayCurrencyId: number | null
  setDisplayCurrencyId: (id: number | null) => void
}

const DisplayCurrencyContext = createContext<DisplayCurrencyContextValue | undefined>(undefined)

export function DisplayCurrencyProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [displayCurrencyId, setDisplayCurrencyIdState] = useState<number | null>(null)

  const setDisplayCurrencyId = useCallback((id: number | null) => {
    setDisplayCurrencyIdState(id)
  }, [])

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
