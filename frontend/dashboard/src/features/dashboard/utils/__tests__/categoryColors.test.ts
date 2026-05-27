import { describe, it, expect } from 'vitest'
import { getCategoryColor } from '../categoryColors'

const PALETTE_SIZE = 17

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

  it('wraps around after palette size', () => {
    expect(getCategoryColor(0)).toEqual(getCategoryColor(PALETTE_SIZE))
    expect(getCategoryColor(1)).toEqual(getCategoryColor(PALETTE_SIZE + 1))
    expect(getCategoryColor(5)).toEqual(getCategoryColor(PALETTE_SIZE + 5))
  })

  it('returns object with bg and text fields', () => {
    const result = getCategoryColor(0)
    expect(result).toHaveProperty('bg')
    expect(result).toHaveProperty('text')
  })

  it('all palette slots return valid 6-digit hex text colors', () => {
    for (let i = 0; i < PALETTE_SIZE; i++) {
      expect(getCategoryColor(i).text).toMatch(/^#[0-9A-Fa-f]{6}$/)
    }
  })

  it('all palette slots return unique text colors', () => {
    const texts = Array.from({ length: PALETTE_SIZE }, (_, i) => getCategoryColor(i).text)
    expect(new Set(texts).size).toBe(PALETTE_SIZE)
  })

  it('handles large ids via modulo', () => {
    expect(getCategoryColor(1000)).toEqual(getCategoryColor(1000 % PALETTE_SIZE))
  })

  it('handles id 0', () => {
    const result = getCategoryColor(0)
    expect(result.bg).toBeDefined()
    expect(result.text).toBeDefined()
  })
})
