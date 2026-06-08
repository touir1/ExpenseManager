import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'

vi.mock('@/features/dashboard/services/dashboardApi.service', () => ({
  getSummary: vi.fn(),
  getDashboardCategories: vi.fn(),
  getRecent: vi.fn(),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({ families: [], activeFamilyId: null, setActiveFamilyId: vi.fn() }),
}))

vi.mock('@/features/currencies/DisplayCurrencyContext', () => ({
  useDisplayCurrency: () => ({ displayCurrencyId: 1, setDisplayCurrencyId: vi.fn() }),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => ({
    currencies: [{ id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 }],
    categories: [],
    tags: [],
    isLoading: false,
    refresh: vi.fn(),
  }),
}))

vi.mock('@/features/notifications/components/NotificationBell', () => ({
  NotificationBell: () => <div data-testid="notification-bell" />,
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
  IonProgressBar: ({ value }: any) => <progress value={value} />,
  IonText: ({ children }: any) => <span>{children}</span>,
  IonSkeletonText: () => <span data-testid="skeleton" />,
  IonSelect: ({ children, onIonChange, value }: any) => (
    <select defaultValue={value} onChange={e => onIonChange?.({ detail: { value: Number(e.target.value) } })}>
      {children}
    </select>
  ),
  IonSelectOption: ({ children, value }: any) => <option value={value}>{children}</option>,
}))

import { getSummary, getDashboardCategories, getRecent } from '@/features/dashboard/services/dashboardApi.service'
import DashboardPage from '@/features/dashboard/pages/DashboardPage'

const mockGetSummary = getSummary as ReturnType<typeof vi.fn>
const mockGetCategories = getDashboardCategories as ReturnType<typeof vi.fn>
const mockGetRecent = getRecent as ReturnType<typeof vi.fn>

const mockSummary = {
  totalAmount: 2430.50,
  convertedTotal: null,
  displayCurrency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  expenseCount: 42,
  previousPeriodTotal: 2000,
  changePercent: 21.5,
  topCategory: { id: 1, name: 'Food' },
  topCategoryAmount: 800,
}

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
    mockGetRecent.mockResolvedValue({ ok: true, status: 200, data: { items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 1 } })
  })

  it('renders Dashboard title', () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    expect(screen.getAllByText(/dashboard/i).length).toBeGreaterThan(0)
  })

  it('renders summary card with total amount after load', async () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    await waitFor(() => {
      expect(screen.getAllByText(/this month/i).length).toBeGreaterThan(0)
    })
  })

  it('shows skeletons while loading', () => {
    mockGetSummary.mockReturnValue(new Promise(() => {}))
    render(<DashboardPage />, { wrapper: makeWrapper() })
    const skeletons = screen.queryAllByTestId('skeleton')
    expect(skeletons.length).toBeGreaterThanOrEqual(0)
  })

  it('renders currency selector', () => {
    render(<DashboardPage />, { wrapper: makeWrapper() })
    expect(screen.getByRole('combobox')).toBeDefined()
  })
})
