import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'

vi.mock('@/features/dashboard/services/dashboardApi.service', () => ({
  getSummary: vi.fn(),
  getDashboardCategories: vi.fn(),
  getMonthly: vi.fn(),
  getByCurrency: vi.fn(),
  getSameMonthYearly: vi.fn(),
  getRecent: vi.fn(),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({ families: [], activeFamilyId: null, setActiveFamilyId: vi.fn() }),
}))

vi.mock('@/features/currencies/DisplayCurrencyContext', () => ({
  useDisplayCurrency: () => ({ displayCurrencyId: 1, setDisplayCurrencyId: vi.fn() }),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => {
  const data = {
    currencies: [{ id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 }],
    categories: [],
    tags: [],
    isLoading: false,
    refresh: vi.fn(),
  }
  return { useExpensesData: () => data }
})

vi.mock('@/features/notifications/components/NotificationBell', () => ({
  NotificationBell: () => <div data-testid="notification-bell" />,
}))

vi.mock('@/features/dashboard/components/DashboardDateFilter', () => ({
  DashboardDateFilter: ({ value, onChange }: any) => (
    <div data-testid="date-filter" data-value={value}>
      <button onClick={() => onChange({ period: 'year', dateFrom: '2025-01-01', dateTo: '2025-12-31' })}>
        year
      </button>
    </div>
  ),
  getPeriodDates: (p: string) => ({
    period: p,
    dateFrom: '2025-01-01',
    dateTo: '2025-12-31',
  }),
}))

vi.mock('@/features/dashboard/components/SpendTrendChart', () => ({
  SpendTrendChart: ({ data, isLoading }: any) => (
    <div data-testid="spend-trend" data-loading={String(isLoading)} data-count={data.length} />
  ),
}))

vi.mock('@/features/dashboard/components/CategoryPieChart', () => ({
  CategoryPieChart: ({ data, isLoading }: any) => (
    <div data-testid="category-pie" data-loading={String(isLoading)} data-count={data.length} />
  ),
}))

vi.mock('@/features/dashboard/components/SameMonthChart', () => ({
  SameMonthChart: ({ data, isLoading }: any) => (
    <div data-testid="same-month" data-loading={String(isLoading)} data-count={data.length} />
  ),
}))

vi.mock('@/features/dashboard/components/CurrenciesPanel', () => ({
  CurrenciesPanel: ({ data, isLoading }: any) => (
    <div data-testid="currencies-panel" data-loading={String(isLoading)} data-count={data.length} />
  ),
}))

vi.mock('@ionic/react', async () => ({
  IonPage: ({ children }: any) => <div>{children}</div>,
  IonHeader: ({ children }: any) => <div>{children}</div>,
  IonToolbar: ({ children }: any) => <div>{children}</div>,
  IonTitle: ({ children }: any) => <h1>{children}</h1>,
  IonContent: ({ children }: any) => <div>{children}</div>,
  IonCard: ({ children }: any) => <div>{children}</div>,
  IonCardHeader: ({ children }: any) => <div>{children}</div>,
  IonCardTitle: ({ children }: any) => <h2>{children}</h2>,
  IonCardSubtitle: ({ children }: any) => <p>{children}</p>,
  IonCardContent: ({ children }: any) => <div>{children}</div>,
  IonList: ({ children }: any) => <ul>{children}</ul>,
  IonItem: ({ children }: any) => <li>{children}</li>,
  IonLabel: ({ children }: any) => <span>{children}</span>,
  IonText: ({ children }: any) => <span>{children}</span>,
  IonSkeletonText: () => <span data-testid="skeleton" />,
  IonSelect: ({ children, onIonChange, value }: any) => (
    <select defaultValue={value} onChange={e => onIonChange?.({ detail: { value: Number(e.target.value) } })}>
      {children}
    </select>
  ),
  IonSelectOption: ({ children, value }: any) => <option value={value}>{children}</option>,
}))

import {
  getSummary,
  getDashboardCategories,
  getMonthly,
  getByCurrency,
  getSameMonthYearly,
  getRecent,
} from '@/features/dashboard/services/dashboardApi.service'
import DashboardPage from '@/features/dashboard/pages/DashboardPage'

const mockGetSummary = getSummary as ReturnType<typeof vi.fn>
const mockGetCategories = getDashboardCategories as ReturnType<typeof vi.fn>
const mockGetMonthly = getMonthly as ReturnType<typeof vi.fn>
const mockGetByCurrency = getByCurrency as ReturnType<typeof vi.fn>
const mockGetSameMonth = getSameMonthYearly as ReturnType<typeof vi.fn>
const mockGetRecent = getRecent as ReturnType<typeof vi.fn>

const mockSummary = {
  totalAmount: 2430.50,
  convertedTotal: null,
  displayCurrency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  expenseCount: 42,
  previousPeriodTotal: 2000,
  changePercent: 21.5,
  topCategory: { id: 1, name: 'Food', description: null, icon: null },
  topCategoryAmount: 800,
}

const emptyPage = { items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 1 }

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return ({ children }: any) => (
    <MemoryRouter>
      <QueryClientProvider client={qc}>{children}</QueryClientProvider>
    </MemoryRouter>
  )
}

describe('DashboardPage', () => {
  beforeEach(() => {
    mockGetSummary.mockResolvedValue({ ok: true, status: 200, data: mockSummary })
    mockGetCategories.mockResolvedValue({ ok: true, status: 200, data: [] })
    mockGetMonthly.mockResolvedValue({ ok: true, status: 200, data: [] })
    mockGetByCurrency.mockResolvedValue({ ok: true, status: 200, data: [] })
    mockGetSameMonth.mockResolvedValue({ ok: true, status: 200, data: [] })
    mockGetRecent.mockResolvedValue({ ok: true, status: 200, data: emptyPage })
  })

  it('renders Dashboard title', () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    expect(screen.getAllByText(/dashboard/i).length).toBeGreaterThan(0)
  })

  it('renders date filter', () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    expect(screen.getByTestId('date-filter')).toBeDefined()
  })

  it('renders all chart section placeholders', () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    expect(screen.getByTestId('spend-trend')).toBeDefined()
    expect(screen.getByTestId('category-pie')).toBeDefined()
    expect(screen.getByTestId('same-month')).toBeDefined()
    expect(screen.getByTestId('currencies-panel')).toBeDefined()
  })

  it('shows skeletons in hero while summary is loading', () => {
    mockGetSummary.mockReturnValue(new Promise(() => {}))
    render(<DashboardPage />, { wrapper: makeWrapper() })
    expect(screen.queryAllByTestId('skeleton').length).toBeGreaterThan(0)
  })

  it('renders summary total after load', async () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getByText(/2430/)).toBeDefined()
    })
  })

  it('renders top category after load', async () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getByText(/Food/)).toBeDefined()
    })
  })

  it('passes data to SpendTrendChart after load', async () => {
    const monthlyData = [{ year: 2025, month: 1, totalAmount: 500, convertedTotal: null, byCategory: [] }]
    mockGetMonthly.mockResolvedValue({ ok: true, status: 200, data: monthlyData })
    render(<DashboardPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getByTestId('spend-trend').getAttribute('data-count')).toBe('1')
    })
  })

  it('passes data to CategoryPieChart after load', async () => {
    const catData = [{ category: { id: 1, name: 'Food' }, totalAmount: 500, convertedTotal: null, percentage: 100, subcategories: [] }]
    mockGetCategories.mockResolvedValue({ ok: true, status: 200, data: catData })
    render(<DashboardPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getByTestId('category-pie').getAttribute('data-count')).toBe('1')
    })
  })

  it('renders currency selector', () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    expect(screen.getByRole('combobox')).toBeDefined()
  })

  it('date filter change triggers period state update', async () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    const yearBtn = screen.getByRole('button', { name: 'year' })
    fireEvent.click(yearBtn)
    await waitFor(() => {
      expect(screen.getByTestId('date-filter').getAttribute('data-value')).toBe('year')
    })
  })

  it('fires all 6 queries on mount', async () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(mockGetSummary).toHaveBeenCalled()
      expect(mockGetCategories).toHaveBeenCalled()
      expect(mockGetMonthly).toHaveBeenCalled()
      expect(mockGetByCurrency).toHaveBeenCalled()
      expect(mockGetSameMonth).toHaveBeenCalled()
      expect(mockGetRecent).toHaveBeenCalled()
    })
  })
})
