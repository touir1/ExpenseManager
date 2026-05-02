import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/features/auth/AuthContext'
import PasswordStrength from '@/components/PasswordStrength'
import PasswordInput from '@/components/PasswordInput'
import AuthCard from '@/features/auth/components/AuthCard'
import AuthPageHeader from '@/features/auth/components/AuthPageHeader'
import SubmitButton from '@/components/SubmitButton'
import FieldError from '@/components/FieldError'
import BackLink from '@/components/BackLink'
import { usePageTitle } from '@/hooks/usePageTitle'
import { makeChangePasswordSchema, type ChangePasswordFormData } from '@/features/auth/auth.schemas'

export default function ChangePasswordPage() {
  const { t } = useTranslation()
  usePageTitle(t('auth.changePassword.pageTitle'))
  const [serverMsg, setServerMsg] = useState<{ text: string; ok: boolean } | null>(null)
  const { changePassword } = useAuth()

  const schema = useMemo(() => makeChangePasswordSchema(t), [t])
  const { register, handleSubmit, watch, formState: { errors, isSubmitting } } = useForm<ChangePasswordFormData>({
    resolver: zodResolver(schema),
  })

  const newPassword = watch('newPassword', '')

  const onSubmit = async (data: ChangePasswordFormData) => {
    setServerMsg(null)
    const { ok, error } = await changePassword(data.oldPassword, data.newPassword)
    setServerMsg({ text: ok ? t('auth.changePassword.success') : error ?? t('auth.changePassword.error'), ok })
  }

  return (
    <AuthCard>
      <BackLink to="/settings">{t('auth.changePassword.backToSettings')}</BackLink>

      <div className="mt-6">
        <AuthPageHeader title={t('auth.changePassword.title')} subtitle={t('auth.changePassword.subtitle')} />
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div>
          <label htmlFor="oldPassword" className="field-label">{t('auth.changePassword.oldPassword')}</label>
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
          <label htmlFor="newPassword" className="field-label">{t('auth.changePassword.newPassword')}</label>
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
          <label htmlFor="repeatPassword" className="field-label">{t('auth.changePassword.repeatPassword')}</label>
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

        <SubmitButton isSubmitting={isSubmitting} label={t('auth.changePassword.submit')} loadingLabel={t('auth.changePassword.submitting')} />
      </form>

      {serverMsg && (
        <p className={`mt-4 ${serverMsg.ok ? 'msg-success' : 'msg-error'}`} role="alert">
          {serverMsg.text}
        </p>
      )}
    </AuthCard>
  )
}
