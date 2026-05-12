import { describe, it, expect } from 'vitest'
import { makeCreateFamilySchema, makeRenameFamilySchema, makeInviteMemberSchema } from '../family.schemas'

const t = (key: string) => key

describe('family.schemas', () => {
  describe('makeCreateFamilySchema', () => {
    const schema = makeCreateFamilySchema(t as any)

    it('accepts valid name', () => {
      expect(schema.safeParse({ name: 'Smith household' }).success).toBe(true)
    })

    it('rejects empty name', () => {
      const result = schema.safeParse({ name: '' })
      expect(result.success).toBe(false)
      expect(result.error?.issues[0].message).toBe('validation.familyNameRequired')
    })

    it('rejects name over 100 characters', () => {
      const result = schema.safeParse({ name: 'a'.repeat(101) })
      expect(result.success).toBe(false)
      expect(result.error?.issues[0].message).toBe('validation.familyNameMax')
    })

    it('accepts name exactly 100 characters', () => {
      expect(schema.safeParse({ name: 'a'.repeat(100) }).success).toBe(true)
    })
  })

  describe('makeRenameFamilySchema', () => {
    const schema = makeRenameFamilySchema(t as any)

    it('accepts valid name', () => {
      expect(schema.safeParse({ name: 'Holiday' }).success).toBe(true)
    })

    it('rejects empty name', () => {
      expect(schema.safeParse({ name: '' }).success).toBe(false)
    })

    it('rejects name over 100 characters', () => {
      expect(schema.safeParse({ name: 'b'.repeat(101) }).success).toBe(false)
    })
  })

  describe('makeInviteMemberSchema', () => {
    const schema = makeInviteMemberSchema(t as any)

    it('accepts valid email', () => {
      expect(schema.safeParse({ email: 'user@example.com' }).success).toBe(true)
    })

    it('rejects empty email', () => {
      const result = schema.safeParse({ email: '' })
      expect(result.success).toBe(false)
      expect(result.error?.issues[0].message).toBe('validation.emailRequired')
    })

    it('rejects invalid email format', () => {
      const result = schema.safeParse({ email: 'not-an-email' })
      expect(result.success).toBe(false)
      expect(result.error?.issues[0].message).toBe('validation.emailInvalid')
    })
  })
})
