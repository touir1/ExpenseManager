import { describe, it, expect } from 'vitest'
import { formatExpenseDate } from '../dateFormat'

// toLocaleDateString(undefined, ...) is host-locale-dependent (month name, separator,
// word order), so assertions check for digit content instead of an exact literal string.
describe('formatExpenseDate', () => {
  it('formats an ISO date string with day and year present', () => {
    const formatted = formatExpenseDate('2026-05-01')
    expect(formatted).toMatch(/\b1\b/)
    expect(formatted).toMatch(/2026/)
  })

  it('does not shift the date backward due to timezone parsing (local midnight, not UTC)', () => {
    const formatted = formatExpenseDate('2026-01-01')
    expect(formatted).toMatch(/\b1\b/)
    expect(formatted).not.toMatch(/31/)
  })

  it('renders a different day/month/year correctly', () => {
    const formatted = formatExpenseDate('2026-12-25')
    expect(formatted).toMatch(/25/)
    expect(formatted).toMatch(/2026/)
  })
})
