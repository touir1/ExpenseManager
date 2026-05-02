import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/features/auth/AuthContext'
import AuthCard from '@/features/auth/components/AuthCard'
import AuthPageHeader from '@/features/auth/components/AuthPageHeader'
import SubmitButton from '@/components/SubmitButton'
import FieldError from '@/components/FieldError'
import { usePageTitle } from '@/hooks/usePageTitle'
import { makeRegisterSchema, type RegisterFormData } from '@/features/auth/auth.schemas'

export default function RegisterPage() {
  const { t } = useTranslation()
  usePageTitle(t('auth.register.pageTitle'))
  const [isSuccess, setIsSuccess] = useState(false)
  const [successMsg, setSuccessMsg] = useState<string | null>(null)
  const { register: registerUser } = useAuth()

  const schema = useMemo(() => makeRegisterSchema(t), [t])
  const { register, handleSubmit, formState: { errors, isSubmitting }, setError } = useForm<RegisterFormData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (data: RegisterFormData) => {
    const { ok } = await registerUser(data.firstName, data.lastName, data.email)
    if (ok) {
      setIsSuccess(true)
      setSuccessMsg(t('auth.register.successMessage'))
    } else {
      setError('root', { message: t('auth.register.failedMessage') })
    }
  }

  if (isSuccess) {
    return (
      <AuthCard>
        <p className="msg-success" role="alert">{successMsg}</p>
        <div className="mt-6 pt-5 border-t border-slate-100 text-center">
          <Link to="/login" className="text-sm text-brand-600 hover:text-brand-700 transition-colors duration-150 font-medium">
            {t('auth.register.goToLogin')}
          </Link>
        </div>
      </AuthCard>
    )
  }

  return (
    <AuthCard>
      <AuthPageHeader title={t('auth.register.title')} subtitle={t('auth.register.subtitle')} />

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label htmlFor="firstName" className="field-label">{t('auth.register.firstName')}</label>
            <input
              id="firstName"
              autoFocus
              {...register('firstName')}
              required
              maxLength={100}
              disabled={isSubmitting}
              className="field-input"
              placeholder="Jane"
              aria-describedby={errors.firstName ? 'firstName-error' : undefined}
              aria-invalid={!!errors.firstName}
            />
            <FieldError id="firstName-error" message={errors.firstName?.message} />
          </div>
          <div>
            <label htmlFor="lastName" className="field-label">{t('auth.register.lastName')}</label>
            <input
              id="lastName"
              {...register('lastName')}
              required
              maxLength={100}
              disabled={isSubmitting}
              className="field-input"
              placeholder="Doe"
              aria-describedby={errors.lastName ? 'lastName-error' : undefined}
              aria-invalid={!!errors.lastName}
            />
            <FieldError id="lastName-error" message={errors.lastName?.message} />
          </div>
        </div>

        <div>
          <label htmlFor="email" className="field-label">{t('auth.register.emailAddress')}</label>
          <input
            id="email"
            type="email"
            autoComplete="email"
            {...register('email')}
            required
            maxLength={100}
            disabled={isSubmitting}
            className="field-input"
            placeholder="you@example.com"
            aria-describedby={errors.email ? 'email-error' : undefined}
            aria-invalid={!!errors.email}
          />
          <FieldError id="email-error" message={errors.email?.message} />
        </div>

        <SubmitButton isSubmitting={isSubmitting} label={t('auth.register.submit')} loadingLabel={t('auth.register.submitting')} />
      </form>

      {errors.root && (
        <p className="mt-4 msg-error" role="alert">
          {errors.root.message}
        </p>
      )}

      <div className="mt-6 pt-5 border-t border-slate-100 text-center">
        <Link to="/login" className="text-sm text-brand-600 hover:text-brand-700 transition-colors duration-150">
          {t('auth.register.alreadyHaveAccount')} <span className="font-medium">{t('auth.register.goToLoginLink')}</span>
        </Link>
      </div>
    </AuthCard>
  )
}
