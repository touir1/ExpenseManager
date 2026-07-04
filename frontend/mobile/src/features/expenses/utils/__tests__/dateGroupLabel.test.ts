import { describe, it, expect } from 'vitest'
import { dateGroupLabel } from '@/features/expenses/utils/dateGroupLabel'

const t = (_key: string, fallback: string) => fallback

describe('dateGroupLabel', () => {
  const now = new Date('2024-06-15T12:00:00Z')

  it('returns Today for the current date', () => {
    expect(dateGroupLabel('2024-06-15', t, now)).toBe('Today')
  })

  it('returns Yesterday for the previous date', () => {
    expect(dateGroupLabel('2024-06-14', t, now)).toBe('Yesterday')
  })

  it('returns a localized weekday string for older dates', () => {
    const result = dateGroupLabel('2024-06-12', t, now)
    expect(result).not.toBe('Today')
    expect(result).not.toBe('Yesterday')
    expect(result.length).toBeGreaterThan(0)
  })
})
