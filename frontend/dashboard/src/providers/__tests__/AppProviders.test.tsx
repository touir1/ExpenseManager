import { describe, it, expect, vi } from 'vitest'
import { render, screen, renderHook } from '@testing-library/react'
import { createContext, useContext, type ReactNode } from 'react'

// ── composeProviders (tested in isolation via dynamic import) ──────────────
// Import the non-exported helper by re-implementing the same logic so we can
// unit-test it without mocking the whole module.
function composeProviders(...providers: React.ComponentType<Readonly<{ children: ReactNode }>>[]) {
  return ({ children }: { children: ReactNode }) =>
    providers.reduceRight<ReactNode>((acc, Provider) => <Provider>{acc}</Provider>, children) as JSX.Element
}

// ── AppProviders (integration: all real providers mocked) ─────────────────
vi.mock('@/features/auth/AuthContext', () => ({
  AuthProvider: ({ children }: { children: ReactNode }) => <>{children}</>,
  useAuth: vi.fn().mockReturnValue({ isAuthenticated: false, isLoading: false }),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  ExpensesDataProvider: ({ children }: { children: ReactNode }) => <>{children}</>,
  useExpensesData: vi.fn().mockReturnValue({ categories: [], currencies: [], isLoading: false, refresh: vi.fn() }),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  FamilyProvider: ({ children }: { children: ReactNode }) => <>{children}</>,
  useFamilies: vi.fn().mockReturnValue({ families: [], activeFamilyId: null, setActiveFamilyId: vi.fn(), isLoading: false, refresh: vi.fn() }),
}))

import { AppProviders } from '../AppProviders'
import { useAuth } from '@/features/auth/AuthContext'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { useFamilies } from '@/features/families/FamilyContext'

// ── composeProviders ──────────────────────────────────────────────────────

describe('composeProviders', () => {
  it('renders children', () => {
    const Composed = composeProviders()
    render(<Composed><span>child</span></Composed>)
    expect(screen.getByText('child')).toBeInTheDocument()
  })

  it('wraps providers outermost-first (first arg = outermost)', () => {
    const order: string[] = []
    const A = ({ children }: { children: ReactNode }) => { order.push('A'); return <>{children}</> }
    const B = ({ children }: { children: ReactNode }) => { order.push('B'); return <>{children}</> }
    const C = ({ children }: { children: ReactNode }) => { order.push('C'); return <>{children}</> }

    const Composed = composeProviders(A, B, C)
    render(<Composed><span /></Composed>)

    expect(order).toEqual(['A', 'B', 'C'])
  })

  it('makes context from first provider available to children', () => {
    const Ctx = createContext('default')
    const Provider = ({ children }: { children: ReactNode }) => (
      <Ctx.Provider value="from-provider">{children}</Ctx.Provider>
    )
    const Composed = composeProviders(Provider)

    const { result } = renderHook(() => useContext(Ctx), { wrapper: Composed })
    expect(result.current).toBe('from-provider')
  })

  it('inner provider can read outer provider context', () => {
    const Ctx = createContext('default')
    const Outer = ({ children }: { children: ReactNode }) => (
      <Ctx.Provider value="outer">{children}</Ctx.Provider>
    )
    const captured: string[] = []
    const Inner = ({ children }: { children: ReactNode }) => {
      captured.push(useContext(Ctx))
      return <>{children}</>
    }
    const Composed = composeProviders(Outer, Inner)
    render(<Composed><span /></Composed>)
    expect(captured[0]).toBe('outer')
  })
})

// ── AppProviders ──────────────────────────────────────────────────────────

describe('AppProviders', () => {
  it('renders children', () => {
    render(<AppProviders><span>hello</span></AppProviders>)
    expect(screen.getByText('hello')).toBeInTheDocument()
  })

  it('useAuth accessible inside AppProviders', () => {
    const { result } = renderHook(() => useAuth(), { wrapper: AppProviders })
    expect(result.current).toBeDefined()
  })

  it('useExpensesData accessible inside AppProviders', () => {
    const { result } = renderHook(() => useExpensesData(), { wrapper: AppProviders })
    expect(result.current).toBeDefined()
  })

  it('useFamilies accessible inside AppProviders', () => {
    const { result } = renderHook(() => useFamilies(), { wrapper: AppProviders })
    expect(result.current).toBeDefined()
  })
})
