import { describe, it, expect } from 'vitest'
import { makeExpenseSchema } from '../expense.schemas'

const t = (key: string) => key

const schema = makeExpenseSchema(t as never)

const valid = {
  amount: 42.5,
  currencyId: 1,
  date: '2026-05-19',
}

describe('expense.schemas', () => {
  it('accepts a minimal valid request', () => {
    expect(schema.safeParse(valid).success).toBe(true)
  })

  it('accepts a fully populated request', () => {
    const result = schema.safeParse({
      ...valid,
      categoryId: 1,
      subcategoryId: 2,
      description: 'Lunch',
      familyIds: [1, 2],
      tagIds: [3],
    })
    expect(result.success).toBe(true)
  })

  it('rejects missing amount', () => {
    const { success, error } = schema.safeParse({ ...valid, amount: undefined })
    expect(success).toBe(false)
    expect(error?.issues.some(i => i.path[0] === 'amount')).toBe(true)
  })

  it('rejects non-positive amount', () => {
    const { success, error } = schema.safeParse({ ...valid, amount: 0 })
    expect(success).toBe(false)
    expect(error?.issues.some(i => i.path[0] === 'amount')).toBe(true)
  })

  it('rejects negative amount', () => {
    const { success, error } = schema.safeParse({ ...valid, amount: -5 })
    expect(success).toBe(false)
    expect(error?.issues.some(i => i.path[0] === 'amount')).toBe(true)
  })

  it('rejects missing currencyId', () => {
    const { success, error } = schema.safeParse({ ...valid, currencyId: undefined })
    expect(success).toBe(false)
    expect(error?.issues.some(i => i.path[0] === 'currencyId')).toBe(true)
  })

  it('rejects missing date', () => {
    const { success, error } = schema.safeParse({ ...valid, date: '' })
    expect(success).toBe(false)
    expect(error?.issues.some(i => i.path[0] === 'date')).toBe(true)
  })

  it('rejects malformed date', () => {
    const { success, error } = schema.safeParse({ ...valid, date: '19-05-2026' })
    expect(success).toBe(false)
    expect(error?.issues.some(i => i.path[0] === 'date')).toBe(true)
  })

  it('rejects subcategoryId when categoryId absent', () => {
    const { success, error } = schema.safeParse({ ...valid, subcategoryId: 5 })
    expect(success).toBe(false)
    expect(error?.issues.some(i => i.path[0] === 'subcategoryId')).toBe(true)
  })

  it('accepts subcategoryId when categoryId present', () => {
    const result = schema.safeParse({ ...valid, categoryId: 1, subcategoryId: 5 })
    expect(result.success).toBe(true)
  })

  it('rejects description over 500 chars', () => {
    const { success, error } = schema.safeParse({ ...valid, description: 'a'.repeat(501) })
    expect(success).toBe(false)
    expect(error?.issues.some(i => i.path[0] === 'description')).toBe(true)
  })

  it('accepts description of exactly 500 chars', () => {
    expect(schema.safeParse({ ...valid, description: 'a'.repeat(500) }).success).toBe(true)
  })

  it('accepts optional fields absent', () => {
    expect(schema.safeParse({ amount: 1, currencyId: 1, date: '2026-01-01' }).success).toBe(true)
  })
})
