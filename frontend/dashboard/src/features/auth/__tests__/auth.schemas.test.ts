import { describe, it, expect } from 'vitest'
import {
  makeLoginSchema,
  makeRegisterSchema,
  makeChangePasswordSchema,
  makeResetPasswordSchema,
  makeRequestPasswordResetSchema,
} from '../auth.schemas'

// Minimal t stub — returns the key so error messages are predictable
const t = (key: string) => key

describe('makeLoginSchema', () => {
  const schema = makeLoginSchema(t as never)

  it('accepts valid credentials', () => {
    expect(schema.safeParse({ email: 'a@b.com', password: 'secret' }).success).toBe(true)
  })

  it('rejects empty email', () => {
    const r = schema.safeParse({ email: '', password: 'secret' })
    expect(r.success).toBe(false)
  })

  it('rejects invalid email format', () => {
    const r = schema.safeParse({ email: 'notanemail', password: 'secret' })
    expect(r.success).toBe(false)
  })

  it('rejects empty password', () => {
    const r = schema.safeParse({ email: 'a@b.com', password: '' })
    expect(r.success).toBe(false)
  })

  it('accepts optional rememberMe', () => {
    expect(schema.safeParse({ email: 'a@b.com', password: 'x', rememberMe: true }).success).toBe(true)
  })
})

describe('makeRegisterSchema', () => {
  const schema = makeRegisterSchema(t as never)
  const valid = { firstName: 'Alice', lastName: 'Smith', email: 'a@b.com' }

  it('accepts valid data', () => {
    expect(schema.safeParse(valid).success).toBe(true)
  })

  it('rejects empty firstName', () => {
    expect(schema.safeParse({ ...valid, firstName: '' }).success).toBe(false)
  })

  it('rejects firstName longer than 100 chars', () => {
    expect(schema.safeParse({ ...valid, firstName: 'a'.repeat(101) }).success).toBe(false)
  })

  it('rejects empty lastName', () => {
    expect(schema.safeParse({ ...valid, lastName: '' }).success).toBe(false)
  })

  it('rejects invalid email', () => {
    expect(schema.safeParse({ ...valid, email: 'bad' }).success).toBe(false)
  })

  it('rejects email longer than 100 chars', () => {
    expect(schema.safeParse({ ...valid, email: 'a'.repeat(95) + '@b.com' }).success).toBe(false)
  })
})

describe('makeChangePasswordSchema', () => {
  const schema = makeChangePasswordSchema(t as never)
  const valid = { oldPassword: 'old1234', newPassword: 'new1234!', repeatPassword: 'new1234!' }

  it('accepts valid data', () => {
    expect(schema.safeParse(valid).success).toBe(true)
  })

  it('rejects empty oldPassword', () => {
    expect(schema.safeParse({ ...valid, oldPassword: '' }).success).toBe(false)
  })

  it('rejects newPassword shorter than 8 chars', () => {
    expect(schema.safeParse({ ...valid, newPassword: 'short', repeatPassword: 'short' }).success).toBe(false)
  })

  it('rejects mismatched passwords', () => {
    const r = schema.safeParse({ ...valid, repeatPassword: 'different!' })
    expect(r.success).toBe(false)
    if (!r.success) {
      expect(r.error.issues.some(i => i.path.includes('repeatPassword'))).toBe(true)
    }
  })
})

describe('makeResetPasswordSchema', () => {
  const schema = makeResetPasswordSchema(t as never)
  const valid = { newPassword: 'newpass1', repeatPassword: 'newpass1' }

  it('accepts valid data', () => {
    expect(schema.safeParse(valid).success).toBe(true)
  })

  it('rejects newPassword shorter than 8 chars', () => {
    expect(schema.safeParse({ newPassword: 'short', repeatPassword: 'short' }).success).toBe(false)
  })

  it('rejects mismatched passwords', () => {
    const r = schema.safeParse({ ...valid, repeatPassword: 'other123' })
    expect(r.success).toBe(false)
  })
})

describe('makeRequestPasswordResetSchema', () => {
  const schema = makeRequestPasswordResetSchema(t as never)

  it('accepts valid email', () => {
    expect(schema.safeParse({ email: 'a@b.com' }).success).toBe(true)
  })

  it('rejects empty email', () => {
    expect(schema.safeParse({ email: '' }).success).toBe(false)
  })

  it('rejects invalid email', () => {
    expect(schema.safeParse({ email: 'notanemail' }).success).toBe(false)
  })
})
