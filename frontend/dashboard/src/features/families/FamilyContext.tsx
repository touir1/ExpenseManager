import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useAuth } from '@/features/auth/AuthContext'
import { getFamilies } from '@/features/families/services/familyApi.service'
import type { Family } from '@/features/families/types/family.type'

const ACTIVE_FAMILY_KEY = 'activeFamilyId'

type FamilyContextValue = {
  families: Family[]
  activeFamilyId: number | null
  setActiveFamilyId: (id: number | null) => void
  isLoading: boolean
  refresh: () => void
}

const FamilyContext = createContext<FamilyContextValue | undefined>(undefined)

export function FamilyProvider({ children }: Readonly<{ children: ReactNode }>) {
  const { isAuthenticated } = useAuth()
  const [families, setFamilies] = useState<Family[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [activeFamilyId, setActiveFamilyIdState] = useState<number | null>(() => {
    const stored = localStorage.getItem(ACTIVE_FAMILY_KEY)
    return stored ? parseInt(stored, 10) : null
  })

  const load = useCallback(async () => {
    setIsLoading(true)
    const res = await getFamilies()
    if (res.ok && res.data) {
      setFamilies(res.data)
      // If stored active family is no longer valid, clear it
      const stored = localStorage.getItem(ACTIVE_FAMILY_KEY)
      if (stored) {
        const storedId = parseInt(stored, 10)
        const stillMember = res.data.some(f => f.id === storedId && !f.isArchived)
        if (!stillMember) {
          localStorage.removeItem(ACTIVE_FAMILY_KEY)
          setActiveFamilyIdState(null)
        }
      }
    }
    setIsLoading(false)
  }, [])

  useEffect(() => {
    if (isAuthenticated) {
      load()
    } else {
      setFamilies([])
      setActiveFamilyIdState(null)
      localStorage.removeItem(ACTIVE_FAMILY_KEY)
    }
  }, [isAuthenticated, load])

  const setActiveFamilyId = useCallback((id: number | null) => {
    setActiveFamilyIdState(id)
    if (id === null) {
      localStorage.removeItem(ACTIVE_FAMILY_KEY)
    } else {
      localStorage.setItem(ACTIVE_FAMILY_KEY, String(id))
    }
  }, [])

  const value = useMemo<FamilyContextValue>(
    () => ({ families, activeFamilyId, setActiveFamilyId, isLoading, refresh: load }),
    [families, activeFamilyId, setActiveFamilyId, isLoading, load]
  )

  return <FamilyContext.Provider value={value}>{children}</FamilyContext.Provider>
}

export function useFamilies(): FamilyContextValue {
  const ctx = useContext(FamilyContext)
  if (!ctx) throw new Error('useFamilies must be used within FamilyProvider')
  return ctx
}
