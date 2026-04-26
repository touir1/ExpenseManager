import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useAuth } from '@/features/auth/AuthContext'
import { useSearchParams, Link, useNavigate } from 'react-router-dom'
import PasswordStrength from '@/components/PasswordStrength'
import PasswordInput from '@/components/PasswordInput'
import { usePageTitle } from '@/hooks/usePageTitle'
import { resetPasswordSchema, type ResetPasswordFormData } from '@/features/auth/auth.schemas'

export default function ResetPasswordPage() {
  const [serverMsg, setServerMsg] = useState<{ text: string; ok: boolean } | null>(null)
  const { resetPassword } = useAuth()
  const [params] = useSearchParams()
  const navigate = useNavigate()

  const email = params.get('email') || ''
  const verificationHash = params.get('h') || params.get('verificationHash') || ''
  const isCreateMode = params.get('mode') === 'create'
  const missingParams = !email || !verificationHash

  usePageTitle(isCreateMode ? 'Create Password' : 'Reset Password')

  const { register, handleSubmit, watch, formState: { errors, isSubmitting } } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
  })

  const newPassword = watch('newPassword', '')

  const onSubmit = async (data: ResetPasswordFormData) => {
    setServerMsg(null)
    const { ok } = await resetPassword(email, verificationHash, data.newPassword, data.repeatPassword)
    if (ok) {
      setServerMsg({ text: isCreateMode ? 'Password created successfully. Redirecting to home…' : 'Password reset successfully. Redirecting to home…', ok: true })
      setTimeout(() => navigate('/'), 3000)
    } else {
      setServerMsg({ text: isCreateMode ? 'Password creation failed. Please try again.' : 'Password reset failed. Please try again.', ok: false })
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        {/* Header */}
        <div className="mb-7">
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">
            {isCreateMode ? 'Create Password' : 'Reset Password'}
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            {isCreateMode ? 'Set a password for your new account.' : 'Enter a new password for your account.'}
          </p>
        </div>

        {missingParams && (
          <div className="msg-info mb-5" role="alert">
            Invalid or missing verification link. Please request a new reset link.
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <div>
            <label htmlFor="newPassword" className="field-label">New password</label>
            <PasswordInput
              id="newPassword"
              autoComplete="new-password"
              {...register('newPassword')}
              required
              disabled={missingParams || isSubmitting}
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
              disabled={missingParams || isSubmitting}
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

          <button type="submit" disabled={missingParams || !!serverMsg?.ok || isSubmitting} className="btn-primary mt-1">
            {isCreateMode ? 'Create' : 'Reset'}
          </button>
        </form>

        {serverMsg && (
          <p className={`mt-4 ${serverMsg.ok ? 'msg-success' : 'msg-error'}`} role="alert">
            {serverMsg.text}
          </p>
        )}

        {missingParams && (
          <div className="mt-5 pt-5 border-t border-slate-100 text-center">
            <Link
              to="/request-password-reset"
              className="text-sm text-brand-600 hover:text-brand-700 font-medium transition-colors duration-150"
            >
              Request password reset
            </Link>
          </div>
        )}
      </div>
    </div>
  )
}
