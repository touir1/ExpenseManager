import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/features/auth/AuthContext'
import { useSearchParams, Link, useNavigate } from 'react-router-dom'
import PasswordStrength from '@/components/PasswordStrength'
import PasswordInput from '@/components/PasswordInput'
import AuthCard from '@/features/auth/components/AuthCard'
import AuthPageHeader from '@/features/auth/components/AuthPageHeader'
import SubmitButton from '@/components/SubmitButton'
import FieldError from '@/components/FieldError'
import { usePageTitle } from '@/hooks/usePageTitle'
import { makeResetPasswordSchema, type ResetPasswordFormData } from '@/features/auth/auth.schemas'

export default function ResetPasswordPage() {
  const { t } = useTranslation()
  const [serverMsg, setServerMsg] = useState<{ text: string; ok: boolean } | null>(null)
  const { createPassword, resetPassword } = useAuth()
  const [params] = useSearchParams()
  const navigate = useNavigate()

  const email = params.get('email') || ''
  const verificationHash = params.get('h') || params.get('verificationHash') || ''
  const isCreateMode = params.get('mode') === 'create'
  const missingParams = !email || !verificationHash

  usePageTitle(isCreateMode ? t('auth.resetPassword.createPageTitle') : t('auth.resetPassword.pageTitle'))

  const schema = useMemo(() => makeResetPasswordSchema(t), [t])
  const { register, handleSubmit, watch, formState: { errors, isSubmitting } } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(schema),
  })

  const newPassword = watch('newPassword', '')

  const onSubmit = async (data: ResetPasswordFormData) => {
    setServerMsg(null)
    const { ok } = await (isCreateMode ? createPassword : resetPassword)(email, verificationHash, data.newPassword)
    if (ok) {
      setServerMsg({ text: isCreateMode ? t('auth.resetPassword.createSuccessMessage') : t('auth.resetPassword.successMessage'), ok: true })
      setTimeout(() => navigate('/'), 3000)
    } else {
      setServerMsg({ text: isCreateMode ? t('auth.resetPassword.createErrorMessage') : t('auth.resetPassword.errorMessage'), ok: false })
    }
  }

  return (
    <AuthCard>
      <AuthPageHeader
        title={isCreateMode ? t('auth.resetPassword.createTitle') : t('auth.resetPassword.title')}
        subtitle={isCreateMode ? t('auth.resetPassword.createSubtitle') : t('auth.resetPassword.subtitle')}
      />

      {missingParams && (
        <div className="msg-info mb-5" role="alert">
          {t('auth.resetPassword.invalidLink')}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div>
          <label htmlFor="newPassword" className="field-label">{t('auth.resetPassword.newPassword')}</label>
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
          <FieldError id="newPassword-error" message={errors.newPassword?.message} />
        </div>

        <div>
          <label htmlFor="repeatPassword" className="field-label">{t('auth.resetPassword.repeatPassword')}</label>
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
          <FieldError id="repeatPassword-error" message={errors.repeatPassword?.message} />
        </div>

        <SubmitButton
          isSubmitting={isSubmitting}
          label={isCreateMode ? t('auth.resetPassword.createSubmit') : t('auth.resetPassword.submit')}
          loadingLabel={isCreateMode ? t('auth.resetPassword.createSubmitting') : t('auth.resetPassword.submitting')}
          disabled={missingParams || !!serverMsg?.ok || isSubmitting}
        />
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
            {t('auth.resetPassword.requestReset')}
          </Link>
        </div>
      )}
    </AuthCard>
  )
}
