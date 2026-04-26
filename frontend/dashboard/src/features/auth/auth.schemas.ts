import { z } from 'zod'

export const loginSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Please enter a valid email address.'),
  password: z.string().min(1, 'Password is required.'),
  rememberMe: z.boolean().optional(),
})

export const registerSchema = z.object({
  firstName: z.string().min(1, 'First name is required.'),
  lastName: z.string().min(1, 'Last name is required.'),
  email: z.string().min(1, 'Email is required.').email('Please enter a valid email address.'),
})

export const changePasswordSchema = z.object({
  oldPassword: z.string().min(1, 'Current password is required.'),
  newPassword: z.string().min(1, 'New password is required.').min(8, 'Password must be at least 8 characters.'),
  repeatPassword: z.string().min(1, 'Please repeat your new password.'),
}).superRefine(({ newPassword, repeatPassword }, ctx) => {
  if (newPassword && repeatPassword && newPassword !== repeatPassword) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: 'New passwords do not match.', path: ['repeatPassword'] })
  }
})

export const resetPasswordSchema = z.object({
  newPassword: z.string().min(1, 'New password is required.').min(8, 'Password must be at least 8 characters.'),
  repeatPassword: z.string().min(1, 'Please repeat your new password.'),
}).superRefine(({ newPassword, repeatPassword }, ctx) => {
  if (newPassword && repeatPassword && newPassword !== repeatPassword) {
    ctx.addIssue({ code: z.ZodIssueCode.custom, message: 'New passwords do not match.', path: ['repeatPassword'] })
  }
})

export const requestPasswordResetSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Please enter a valid email address.'),
})

export type LoginFormData = z.infer<typeof loginSchema>
export type RegisterFormData = z.infer<typeof registerSchema>
export type ChangePasswordFormData = z.infer<typeof changePasswordSchema>
export type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>
export type RequestPasswordResetFormData = z.infer<typeof requestPasswordResetSchema>
