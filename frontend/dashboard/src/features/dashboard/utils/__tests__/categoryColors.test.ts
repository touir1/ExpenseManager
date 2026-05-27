import { describe, it, expect } from 'vitest'
import { CHART_COLORS, getCategoryColor } from '../categoryColors'

describe('CHART_COLORS', () => {
  it('has 6 entries', () => {
    expect(CHART_COLORS).toHaveLength(6)
  })

  it('contains only valid hex colors', () => {
    CHART_COLORS.forEach(c => {
      expect(c).toMatch(/^#[0-9A-Fa-f]{6}$/)
    })
  })

  it('all values are unique', () => {
    expect(new Set(CHART_COLORS).size).toBe(CHART_COLORS.length)
  })
})

describe('getCategoryColor', () => {
  it('returns fallback color for undefined', () => {
    const result = getCategoryColor(undefined)
    expect(result.bg).toBe('#64748B1A')
    expect(result.text).toBe('#64748B')
  })

  it('returns same color for same id', () => {
    expect(getCategoryColor(1)).toEqual(getCategoryColor(1))
  })

  it('returns different colors for consecutive ids', () => {
    const c0 = getCategoryColor(0)
    const c1 = getCategoryColor(1)
    expect(c0).not.toEqual(c1)
  })

  it('wraps around after 6 entries', () => {
    expect(getCategoryColor(0)).toEqual(getCategoryColor(6))
    expect(getCategoryColor(1)).toEqual(getCategoryColor(7))
    expect(getCategoryColor(5)).toEqual(getCategoryColor(11))
  })

  it('returns object with bg and text fields', () => {
    const result = getCategoryColor(0)
    expect(result).toHaveProperty('bg')
    expect(result).toHaveProperty('text')
  })

  it('text color is in CHART_COLORS', () => {
    for (let i = 0; i < 6; i++) {
      expect(CHART_COLORS).toContain(getCategoryColor(i).text)
    }
  })

  it('handles large ids via modulo', () => {
    expect(getCategoryColor(1000)).toEqual(getCategoryColor(1000 % 6))
  })

  it('handles id 0', () => {
    const result = getCategoryColor(0)
    expect(result.bg).toBeDefined()
    expect(result.text).toBeDefined()
  })
})
