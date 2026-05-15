import { z } from 'zod'
import type { TFunction } from 'i18next'

export function makeCreateFamilySchema(t: TFunction) {
  return z.object({
    name: z
      .string()
      .min(1, t('validation.familyNameRequired'))
      .max(30, t('validation.familyNameMax')),
  })
}

export function makeRenameFamilySchema(t: TFunction) {
  return makeCreateFamilySchema(t)
}

export function makeInviteMemberSchema(t: TFunction) {
  return z.object({
    email: z
      .string()
      .min(1, t('validation.emailRequired'))
      .email(t('validation.emailInvalid')),
  })
}

export type CreateFamilyData = z.infer<ReturnType<typeof makeCreateFamilySchema>>
export type InviteMemberData = z.infer<ReturnType<typeof makeInviteMemberSchema>>
