import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import HomeDashboardPage from '@/features/dashboard/pages/HomeDashboardPage'
import { getSummary, getCategories } from '@/features/dashboard/services/dashboardApi.service'

const useQuerySpy = vi.fn()
vi.mock('@tanstack/react-query', async () => {
  const actual = await vi.importActual<typeof import('@tanstack/react-query')>('@tanstack/react-query')
  return {
    ...actual,
    useQuery: (options: Parameters<typeof actual.useQuery>[0]) => {
      useQuerySpy(options)
      return actual.useQuery(options)
    },
  }
})

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => ({ user: { firstName: 'Ali', email: 'ali@test.com' }, isAuthenticated: true, isLoading: false }),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({ activeFamilyId: null, families: [], setActiveFamilyId: vi.fn(), isLoading: false }),
}))

vi.mock('@/features/currencies/DisplayCurrencyContext', () => ({
  useDisplayCurrency: () => ({ displayCurrencyId: null, setDisplayCurrencyId: vi.fn() }),
}))

vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: () => undefined }))

vi.mock('@/features/dashboard/services/dashboardApi.service', () => ({
  getSummary: vi.fn().mockResolvedValue({
    ok: true,
    data: {
      totalAmount: 2430,
      convertedTotal: null,
      displayCurrency: null,
      expenseCount: 42,
      previousPeriodTotal: 2250,
      changePercent: 8,
      topCategory: { id: 1, name: 'Food', description: null },
      topCategoryAmount: 890,
    },
  }),
  getMonthly: vi.fn().mockResolvedValue({
    ok: true,
    data: [{ year: 2024, month: 1, totalAmount: 1000, convertedTotal: null, byCategory: [] }],
  }),
  getCategories: vi.fn().mockResolvedValue({
    ok: true,
    data: [
      { category: { id: 1, name: 'Food', description: null }, totalAmount: 890, convertedTotal: null, percentage: 36.6, subcategories: [] },
    ],
  }),
  getSameMonthYearly: vi.fn().mockResolvedValue({
    ok: true,
    data: [{ year: 2023, totalAmount: 2100, convertedTotal: null }],
  }),
  getByCurrency: vi.fn().mockResolvedValue({ ok: true, data: [] }),
  getRecent: vi.fn().mockResolvedValue({ ok: true, data: { items: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0 } }),
}))

vi.mock('recharts', () => ({
  ComposedChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="composed-chart">{children}</div>
  ),
  BarChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="bar-chart">{children}</div>
  ),
  PieChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="pie-chart">{children}</div>
  ),
  Bar: () => null,
  Line: () => null,
  Pie: () => null,
  Cell: () => null,
  XAxis: () => null,
  YAxis: () => null,
  Tooltip: () => null,
  CartesianGrid: () => null,
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  Legend: () => null,
}))

function renderPage() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>
        <HomeDashboardPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('HomeDashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    useQuerySpy.mockClear()
  })

  it('keeps the summary query fresh longer than the default staleTime', () => {
    renderPage()
    const summaryCall = useQuerySpy.mock.calls.find(
      ([options]) => Array.isArray(options.queryKey) && options.queryKey[1] === 'summary',
    )
    expect(summaryCall?.[0].staleTime).toBe(60_000)
  })

  it('renders without crashing', () => {
    renderPage()
    expect(document.body).toBeTruthy()
  })

  it('renders DashboardFilters with preset buttons', () => {
    renderPage()
    expect(screen.getByRole('button', { name: /this month/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /this year/i })).toBeInTheDocument()
  })

  it('shows skeletons while loading', () => {
    renderPage()
    expect(screen.getAllByRole('status').length).toBeGreaterThan(0)
  })

  it('renders SpendChart after data loads', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByTestId('composed-chart')).toBeInTheDocument())
  })

  it('renders CategoryDonut after data loads', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByTestId('pie-chart')).toBeInTheDocument())
  })

  it('renders SameMonthChart after data loads', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByTestId('bar-chart')).toBeInTheDocument())
  })

  it('renders RecentExpenses section link', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByRole('link', { name: /view all/i })).toBeInTheDocument())
  })

  it('renders EmptyDashboard with a CTA when there is no data', async () => {
    vi.mocked(getSummary).mockResolvedValueOnce({
      ok: true,
      data: {
        totalAmount: 0,
        convertedTotal: null,
        displayCurrency: null,
        expenseCount: 0,
        previousPeriodTotal: 0,
        changePercent: 0,
        topCategory: null,
        topCategoryAmount: 0,
      },
    })
    vi.mocked(getCategories).mockResolvedValueOnce({ ok: true, data: [] })
    renderPage()
    await waitFor(() => expect(screen.getByRole('button', { name: /add/i })).toBeInTheDocument())
    expect(screen.getByText('💸')).toBeInTheDocument()
  })
})
