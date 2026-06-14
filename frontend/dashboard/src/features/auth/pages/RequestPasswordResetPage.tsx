import { useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/features/auth/AuthContext'
import { useToast } from '@/components/Toast'
import AuthCard from '@/features/auth/components/AuthCard'
import AuthPageHeader from '@/features/auth/components/AuthPageHeader'
import SubmitButton from '@/components/SubmitButton'
import EmailField from '@/features/auth/components/EmailField'
import BackLink from '@/components/BackLink'
import { usePageTitle } from '@/hooks/usePageTitle'
import { makeRequestPasswordResetSchema, type RequestPasswordResetFormData } from '@/features/auth/auth.schemas'

export default function RequestPasswordResetPage() {
  const { t } = useTranslation()
  usePageTitle(t('auth.requestPasswordReset.pageTitle'))
  const { requestPasswordReset } = useAuth()
  const { show } = useToast()

  const schema = useMemo(() => makeRequestPasswordResetSchema(t), [t])
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<RequestPasswordResetFormData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (data: RequestPasswordResetFormData) => {
    if (!requestPasswordReset) {
      show(t('auth.requestPasswordReset.notAvailable'), 'error')
      return
    }
    const { ok } = await requestPasswordReset(data.email)
    if (ok) {
      show(t('auth.requestPasswordReset.successToast'), 'success')
    } else {
      show(t('auth.requestPasswordReset.errorToast'), 'error')
    }
  }

  return (
    <AuthCard>
      <AuthPageHeader
        title={t('auth.requestPasswordReset.title')}
        subtitle={t('auth.requestPasswordReset.subtitle')}
      />

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <EmailField
          registration={register('email')}
          error={errors.email?.message}
          isSubmitting={isSubmitting}
          autoFocus
        />

        <SubmitButton isSubmitting={isSubmitting} label={t('auth.requestPasswordReset.submit')} loadingLabel={t('auth.requestPasswordReset.submitting')} />
      </form>

      <p className="mt-5 text-xs text-ink-faint text-center leading-relaxed">
        {t('auth.requestPasswordReset.hint')}
      </p>

      <div className="mt-5 text-center">
        <BackLink to="/login">{t('auth.requestPasswordReset.backToLogin')}</BackLink>
      </div>
    </AuthCard>
  )
}
