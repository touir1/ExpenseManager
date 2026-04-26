import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import PasswordStrength from '@/components/PasswordStrength'
import PasswordInput from '@/components/PasswordInput'
import { usePageTitle } from '@/hooks/usePageTitle'
import { changePasswordSchema, type ChangePasswordFormData } from '@/features/auth/auth.schemas'

export default function ChangePasswordPage() {
  usePageTitle('Change Password')
  const [serverMsg, setServerMsg] = useState<{ text: string; ok: boolean } | null>(null)
  const { changePassword } = useAuth()

  const { register, handleSubmit, watch, formState: { errors, isSubmitting } } = useForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
  })

  const newPassword = watch('newPassword', '')

  const onSubmit = async (data: ChangePasswordFormData) => {
    setServerMsg(null)
    const { ok, error } = await changePassword(data.oldPassword, data.newPassword, data.repeatPassword)
    setServerMsg({ text: ok ? 'Password changed.' : error ?? 'Incorrect current password.', ok })
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        {/* Back link */}
        <Link
          to="/settings"
          className="inline-flex items-center gap-1 text-sm text-slate-500 hover:text-slate-700 mb-6 transition-colors duration-150"
        >
          <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
          Back to settings
        </Link>

        {/* Header */}
        <div className="mb-7">
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Change Password</h1>
          <p className="text-sm text-slate-500 mt-1">Update your account password below.</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <div>
            <label htmlFor="oldPassword" className="field-label">Old password</label>
            <PasswordInput
              id="oldPassword"
              autoComplete="current-password"
              {...register('oldPassword')}
              required
              disabled={isSubmitting}
              className="field-input"
              aria-describedby={errors.oldPassword ? 'oldPassword-error' : undefined}
              aria-invalid={!!errors.oldPassword}
            />
            {errors.oldPassword && (
              <p id="oldPassword-error" className="field-error" role="alert">
                {errors.oldPassword.message}
              </p>
            )}
          </div>

          <div>
            <label htmlFor="newPassword" className="field-label">New password</label>
            <PasswordInput
              id="newPassword"
              autoComplete="new-password"
              {...register('newPassword')}
              required
              disabled={isSubmitting}
              className="field-input"
              aria-describedby={errors.newPassword ? 'newPassword-error' : undefined}
              aria-invalid={!!errors.newPassword}
            />
            <PasswordStrength password={newPassword} />
            {errors.newPassword && (
              <p id="newPassword-error" className="field-error" role="alert">
                {errors.newPassword.message}
              </p>
            )}
          </div>

          <div>
            <label htmlFor="repeatPassword" className="field-label">Repeat new password</label>
            <PasswordInput
              id="repeatPassword"
              autoComplete="new-password"
              {...register('repeatPassword')}
              required
              disabled={isSubmitting}
              className="field-input"
              aria-describedby={errors.repeatPassword ? 'repeatPassword-error' : undefined}
              aria-invalid={!!errors.repeatPassword}
            />
            {errors.repeatPassword && (
              <p id="repeatPassword-error" className="field-error" role="alert">
                {errors.repeatPassword.message}
              </p>
            )}
          </div>

          <button type="submit" disabled={isSubmitting} className="btn-primary mt-1">
            Change password
          </button>
        </form>

        {serverMsg && (
          <p className={`mt-4 ${serverMsg.ok ? 'msg-success' : 'msg-error'}`} role="alert">
            {serverMsg.text}
          </p>
        )}
      </div>
    </div>
  )
}
