import { z } from 'zod'
import type { TFunction } from 'i18next'

export function makeLoginSchema(t: TFunction) {
  return z.object({
    email: z.string().min(1, t('validation.required')).email(t('validation.email')),
    password: z.string().min(1, t('validation.required')),
    rememberMe: z.boolean().optional(),
  })
}

export type LoginFormData = z.infer<ReturnType<typeof makeLoginSchema>>
