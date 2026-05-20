import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { DashboardFilters } from '../DashboardFilters'
import type { DashboardFilter } from '../../types/dashboard.type'

const defaultFilter: DashboardFilter = { dateFrom: '2024-01-01', dateTo: '2024-01-31' }

describe('DashboardFilters', () => {
  it('renders "This month" and "This year" buttons', () => {
    render(<DashboardFilters filter={defaultFilter} onChange={vi.fn()} />)
    expect(screen.getByRole('button', { name: /this month/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /this year/i })).toBeInTheDocument()
  })

  it('"This month" calls onChange with dateFrom matching YYYY-MM-01 pattern', async () => {
    const onChange = vi.fn()
    render(<DashboardFilters filter={defaultFilter} onChange={onChange} />)
    await userEvent.setup().click(screen.getByRole('button', { name: /this month/i }))
    expect(onChange).toHaveBeenCalledOnce()
    const [called] = onChange.mock.calls[0]
    expect(called.dateFrom).toMatch(/^\d{4}-\d{2}-01$/)
  })

  it('"This year" calls onChange with dateFrom matching YYYY-01-01 pattern', async () => {
    const onChange = vi.fn()
    render(<DashboardFilters filter={defaultFilter} onChange={onChange} />)
    await userEvent.setup().click(screen.getByRole('button', { name: /this year/i }))
    expect(onChange).toHaveBeenCalledOnce()
    const [called] = onChange.mock.calls[0]
    expect(called.dateFrom).toMatch(/^\d{4}-01-01$/)
  })

  it('renders date inputs with current filter values', () => {
    render(<DashboardFilters filter={defaultFilter} onChange={vi.fn()} />)
    expect(screen.getByDisplayValue('2024-01-01')).toBeInTheDocument()
    expect(screen.getByDisplayValue('2024-01-31')).toBeInTheDocument()
  })

  it('calls onChange when dateFrom input changes', () => {
    const onChange = vi.fn()
    render(<DashboardFilters filter={defaultFilter} onChange={onChange} />)
    const input = screen.getByDisplayValue('2024-01-01')
    fireEvent.change(input, { target: { value: '2024-03-01' } })
    expect(onChange).toHaveBeenCalled()
  })

  it('"This month" button has aria-pressed true when active', () => {
    const today = new Date()
    const dateFrom = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-01`
    const dateTo = today.toISOString().slice(0, 10)
    render(<DashboardFilters filter={{ dateFrom, dateTo }} onChange={vi.fn()} />)
    expect(screen.getByRole('button', { name: /this month/i })).toHaveAttribute('aria-pressed', 'true')
  })

  it('"This year" button has aria-pressed false when not active', () => {
    render(<DashboardFilters filter={defaultFilter} onChange={vi.fn()} />)
    expect(screen.getByRole('button', { name: /this year/i })).toHaveAttribute('aria-pressed', 'false')
  })
})
