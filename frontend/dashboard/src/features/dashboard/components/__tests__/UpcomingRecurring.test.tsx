import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { UpcomingRecurring } from '../UpcomingRecurring'
import type { RecurringExpenseDto } from '@/features/dashboard/types/dashboard.type'
import { confirm } from '@/features/recurring-expenses/services/recurringExpenseApi.service'

const showMock = vi.fn()
vi.mock('@/components/Toast', () => ({
  useToast: () => ({ show: showMock }),
}))

vi.mock('@/features/recurring-expenses/services/recurringExpenseApi.service', () => ({
  confirm: vi.fn(),
}))

const makeItem = (id: number, overrides: Partial<RecurringExpenseDto> = {}): RecurringExpenseDto => ({
  id,
  description: `Recurring ${id}`,
  amount: 15.99,
  currencyId: 1,
  currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  categoryId: 1,
  category: { id: 1, name: 'Subscriptions', description: undefined },
  subcategoryId: null,
  subcategory: null,
  familyId: null,
  frequencyId: 2,
  nextDueDate: '2026-08-06',
  frequency: 'Monthly',
  isActive: true,
  autoCreate: false,
  ...overrides,
} as RecurringExpenseDto)

function renderPanel(data: RecurringExpenseDto[] = [], isLoading = false) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={qc}>
      <UpcomingRecurring data={data} isLoading={isLoading} />
    </QueryClientProvider>,
  )
}

describe('UpcomingRecurring', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-08-06T12:00:00'))
    showMock.mockClear()
    vi.mocked(confirm).mockReset()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('shows skeleton when loading', () => {
    renderPanel([], true)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('does not show skeleton when not loading', () => {
    renderPanel([makeItem(1)])
    expect(screen.queryByRole('status')).not.toBeInTheDocument()
  })

  it('shows empty state when no upcoming items', () => {
    renderPanel([])
    expect(screen.getByText(/no upcoming recurring/i)).toBeInTheDocument()
  })

  it('renders description and amount for each item', () => {
    renderPanel([makeItem(1, { description: 'Netflix', amount: 15.99 })])
    expect(screen.getByText('Netflix')).toBeInTheDocument()
    expect(screen.getByText('€ 15.99')).toBeInTheDocument()
  })

  it('shows "Due today" for an item due on the current date', () => {
    renderPanel([makeItem(1, { nextDueDate: '2026-08-06' })])
    expect(screen.getByText(/due today/i)).toBeInTheDocument()
  })

  it('shows "Due tomorrow" for an item due the next day', () => {
    renderPanel([makeItem(1, { nextDueDate: '2026-08-07' })])
    expect(screen.getByText(/due tomorrow/i)).toBeInTheDocument()
  })

  it('shows "In N days" for items further out', () => {
    renderPanel([makeItem(1, { nextDueDate: '2026-08-10' })])
    expect(screen.getByText(/in 4 days/i)).toBeInTheDocument()
  })

  it('renders category pill when category is present', () => {
    renderPanel([makeItem(1, { category: { id: 1, name: 'Subscriptions', description: undefined } })])
    expect(screen.getByText('Subscriptions')).toBeInTheDocument()
  })

  it('does not render category pill when category is null', () => {
    renderPanel([makeItem(1, { category: null })])
    expect(screen.queryByText('Subscriptions')).not.toBeInTheDocument()
  })

  it('renders multiple items in the order given', () => {
    renderPanel([
      makeItem(1, { description: 'Rent' }),
      makeItem(2, { description: 'Netflix' }),
    ])
    const rows = screen.getAllByText(/^(Rent|Netflix)$/)
    expect(rows.map(r => r.textContent)).toEqual(['Rent', 'Netflix'])
  })

  it('shows a Confirm button for a due, non-auto-create item', () => {
    renderPanel([makeItem(1, { autoCreate: false, nextDueDate: '2026-08-06' })])
    expect(screen.getByRole('button', { name: /confirm/i })).toBeInTheDocument()
  })

  it('does not show a Confirm button for an item not yet due', () => {
    renderPanel([makeItem(1, { autoCreate: false, nextDueDate: '2026-08-10' })])
    expect(screen.queryByRole('button', { name: /confirm/i })).not.toBeInTheDocument()
  })

  it('shows an Auto badge instead of a Confirm button for autoCreate items', () => {
    renderPanel([makeItem(1, { autoCreate: true, nextDueDate: '2026-08-06' })])
    expect(screen.getByText('Auto')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /confirm/i })).not.toBeInTheDocument()
  })

  it('calls confirm API and invalidates dashboard queries on Confirm click', async () => {
    vi.mocked(confirm).mockResolvedValue({ ok: true, status: 200, data: { id: 99 } as any })
    renderPanel([makeItem(1, { autoCreate: false, nextDueDate: '2026-08-06' })])

    fireEvent.click(screen.getByRole('button', { name: /confirm/i }))
    vi.useRealTimers()

    await waitFor(() => expect(confirm).toHaveBeenCalledWith(1))
  })

  it('shows an error toast when confirm fails', async () => {
    vi.mocked(confirm).mockResolvedValue({ ok: false, status: 400, error: 'x' })
    renderPanel([makeItem(1, { autoCreate: false, nextDueDate: '2026-08-06' })])

    fireEvent.click(screen.getByRole('button', { name: /confirm/i }))
    vi.useRealTimers()

    await waitFor(() => expect(showMock).toHaveBeenCalled())
  })
})
