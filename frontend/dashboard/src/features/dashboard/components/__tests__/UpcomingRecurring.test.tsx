import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { UpcomingRecurring } from '../UpcomingRecurring'
import type { RecurringExpenseDto } from '@/features/dashboard/types/dashboard.type'

const makeItem = (id: number, overrides: Partial<RecurringExpenseDto> = {}): RecurringExpenseDto => ({
  id,
  description: `Recurring ${id}`,
  amount: 15.99,
  currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
  category: { id: 1, name: 'Subscriptions', description: undefined },
  subcategory: null,
  nextDueDate: '2026-08-06',
  frequency: 'Monthly',
  ...overrides,
})

function renderPanel(data: RecurringExpenseDto[] = [], isLoading = false) {
  render(<UpcomingRecurring data={data} isLoading={isLoading} />)
}

describe('UpcomingRecurring', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-08-06T12:00:00'))
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
})
