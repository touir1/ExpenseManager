import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useAuth } from '@/features/auth/AuthContext'
import PasswordStrength from '@/components/PasswordStrength'
import PasswordInput from '@/components/PasswordInput'
import AuthCard from '@/components/AuthCard'
import AuthPageHeader from '@/components/AuthPageHeader'
import SubmitButton from '@/components/SubmitButton'
import FieldError from '@/components/FieldError'
import BackLink from '@/components/BackLink'
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
    <AuthCard>
      <BackLink to="/settings">Back to settings</BackLink>

      <div className="mt-6">
        <AuthPageHeader title="Change Password" subtitle="Update your account password below." />
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
          <FieldError id="oldPassword-error" message={errors.oldPassword?.message} />
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
          <FieldError id="newPassword-error" message={errors.newPassword?.message} />
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
          <FieldError id="repeatPassword-error" message={errors.repeatPassword?.message} />
        </div>

        <SubmitButton isSubmitting={isSubmitting} label="Change password" loadingLabel="Saving…" />
      </form>

      {serverMsg && (
        <p className={`mt-4 ${serverMsg.ok ? 'msg-success' : 'msg-error'}`} role="alert">
          {serverMsg.text}
        </p>
      )}
    </AuthCard>
  )
}
