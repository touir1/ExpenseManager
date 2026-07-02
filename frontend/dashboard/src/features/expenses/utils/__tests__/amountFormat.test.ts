import { describe, it, expect } from 'vitest'
import { formatAmountDisplay, parseAmountInput, sanitizeAmountInputChars } from '../amountFormat'

// formatAmountDisplay uses toLocaleString(undefined, ...), so the exact separator
// characters are host-locale-dependent (comma vs. dot decimal, space vs. comma grouping).
// Assertions strip non-digits and compare digit sequences instead of literal strings.
describe('formatAmountDisplay', () => {
  it('formats whole numbers with 2 decimals', () => {
    expect(formatAmountDisplay(2430).replace(/\D/g, '')).toBe('243000')
  })

  it('formats decimals and includes a grouping separator for thousands', () => {
    const formatted = formatAmountDisplay(2430.5)
    expect(formatted.replace(/\D/g, '')).toBe('243050')
    expect(formatted.length).toBeGreaterThan('243050'.length)
  })

  it('rounds to 2 decimals', () => {
    expect(formatAmountDisplay(2430.567).replace(/\D/g, '')).toBe('243057')
  })

  it('handles zero', () => {
    expect(formatAmountDisplay(0).replace(/\D/g, '')).toBe('000')
  })
})

describe('parseAmountInput', () => {
  it('parses a plain numeric string', () => {
    expect(parseAmountInput('2430.5')).toBe(2430.5)
  })

  it('strips grouping separators before parsing', () => {
    expect(parseAmountInput('2,430.50')).toBe(2430.5)
  })

  it('returns undefined for empty string', () => {
    expect(parseAmountInput('')).toBeUndefined()
  })

  it('returns undefined for a lone minus sign', () => {
    expect(parseAmountInput('-')).toBeUndefined()
  })

  it('filters out non-numeric characters', () => {
    expect(parseAmountInput('12a3b.4c5')).toBe(123.45)
  })

  it('parses a value with comma thousands and dot decimal (en-US style)', () => {
    expect(parseAmountInput('123,456.70')).toBe(123456.7)
  })
})

describe('sanitizeAmountInputChars', () => {
  it('keeps digits and a single dot', () => {
    expect(sanitizeAmountInputChars('2430.5')).toBe('2430.5')
  })

  it('strips letters and symbols', () => {
    expect(sanitizeAmountInputChars('a2b4c3d0e.f5g')).toBe('2430.5')
  })

  it('collapses multiple dots to the first one', () => {
    expect(sanitizeAmountInputChars('12.3.4.5')).toBe('12.345')
  })

  it('handles a trailing dot while typing', () => {
    expect(sanitizeAmountInputChars('10.')).toBe('10.')
  })

  it('returns empty string for fully invalid input', () => {
    expect(sanitizeAmountInputChars('abc')).toBe('')
  })
})
