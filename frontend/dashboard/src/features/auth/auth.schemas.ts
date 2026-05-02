import { z } from 'zod'
import type { TFunction } from 'i18next'

export function makeLoginSchema(t: TFunction) {
  return z.object({
    email: z.string().min(1, t('validation.emailRequired')).email(t('validation.emailInvalid')),
    password: z.string().min(1, t('validation.passwordRequired')),
    rememberMe: z.boolean().optional(),
  })
}

export function makeRegisterSchema(t: TFunction) {
  return z.object({
    firstName: z.string().min(1, t('validation.firstNameRequired')).max(100, t('validation.firstNameMax')),
    lastName:  z.string().min(1, t('validation.lastNameRequired')).max(100, t('validation.lastNameMax')),
    email:     z.string().min(1, t('validation.emailRequired')).max(100, t('validation.emailMax')).email(t('validation.emailInvalid')),
  })
}

export function makeChangePasswordSchema(t: TFunction) {
  return z.object({
    oldPassword:    z.string().min(1, t('validation.currentPasswordRequired')),
    newPassword:    z.string().min(1, t('validation.newPasswordRequired')).min(8, t('validation.passwordMin')),
    repeatPassword: z.string().min(1, t('validation.repeatPasswordRequired')),
  }).superRefine(({ newPassword, repeatPassword }, ctx) => {
    if (newPassword && repeatPassword && newPassword !== repeatPassword) {
      ctx.addIssue({ code: z.ZodIssueCode.custom, message: t('validation.passwordsNoMatch'), path: ['repeatPassword'] })
    }
  })
}

export function makeResetPasswordSchema(t: TFunction) {
  return z.object({
    newPassword:    z.string().min(1, t('validation.newPasswordRequired')).min(8, t('validation.passwordMin')),
    repeatPassword: z.string().min(1, t('validation.repeatPasswordRequired')),
  }).superRefine(({ newPassword, repeatPassword }, ctx) => {
    if (newPassword && repeatPassword && newPassword !== repeatPassword) {
      ctx.addIssue({ code: z.ZodIssueCode.custom, message: t('validation.passwordsNoMatch'), path: ['repeatPassword'] })
    }
  })
}

export function makeRequestPasswordResetSchema(t: TFunction) {
  return z.object({
    email: z.string().min(1, t('validation.emailRequired')).email(t('validation.emailInvalid')),
  })
}

export type LoginFormData           = z.infer<ReturnType<typeof makeLoginSchema>>
export type RegisterFormData        = z.infer<ReturnType<typeof makeRegisterSchema>>
export type ChangePasswordFormData  = z.infer<ReturnType<typeof makeChangePasswordSchema>>
export type ResetPasswordFormData   = z.infer<ReturnType<typeof makeResetPasswordSchema>>
export type RequestPasswordResetFormData = z.infer<ReturnType<typeof makeRequestPasswordResetSchema>>
