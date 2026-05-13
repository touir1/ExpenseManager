import { useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '@/features/auth/AuthContext'
import { useToast } from '@/components/Toast'
import PasswordInput from '@/components/PasswordInput'
import AuthBrandPanel from '@/features/auth/components/AuthBrandPanel'
import AuthPageHeader from '@/features/auth/components/AuthPageHeader'
import EmailField from '@/features/auth/components/EmailField'
import SubmitButton from '@/components/SubmitButton'
import FieldError from '@/components/FieldError'
import { usePageTitle } from '@/hooks/usePageTitle'
import { makeLoginSchema, type LoginFormData } from '@/features/auth/auth.schemas'

export default function LoginPage() {
  const { t } = useTranslation()
  usePageTitle(t('auth.login.pageTitle'))
  const navigate = useNavigate()
  const { login } = useAuth()
  const { show } = useToast()

  const schema = useMemo(() => makeLoginSchema(t), [t])
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginFormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', password: '', rememberMe: false },
  })

  const onSubmit = async (data: LoginFormData) => {
    const { ok } = await login(data.email, data.password, Boolean(data.rememberMe))
    if (!ok) {
      show(t('auth.login.invalidCredentials'), 'error')
      return
    }
    navigate('/dashboard')
  }

  return (
    <div className="flex-1 flex">
      <AuthBrandPanel />

      {/* Form area */}
      <div className="flex-1 flex items-center justify-center p-6 bg-surface-page">
        <div className="w-full max-w-[400px]">
          <AuthPageHeader title={t('auth.login.title')} subtitle={t('auth.login.subtitle')} />

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <EmailField
              registration={register('email')}
              error={errors.email?.message}
              isSubmitting={isSubmitting}
              autoFocus
            />

            <div>
              <label htmlFor="password" className="field-label">{t('auth.login.password')}</label>
              <PasswordInput
                id="password"
                autoComplete="current-password"
                {...register('password')}
                required
                disabled={isSubmitting}
                className="field-input"
                aria-describedby={errors.password ? 'password-error' : undefined}
                aria-invalid={!!errors.password}
              />
              <FieldError id="password-error" message={errors.password?.message} />
            </div>

            <div className="flex items-center gap-2">
              <input
                id="rememberMe"
                type="checkbox"
                {...register('rememberMe')}
                disabled={isSubmitting}
                className="h-4 w-4 rounded border-surface-border text-brand-500 focus:ring-brand-500 cursor-pointer accent-brand-500"
              />
              <label htmlFor="rememberMe" className="text-sm text-ink-body cursor-pointer select-none">
                {t('auth.login.rememberMe')}
              </label>
            </div>

            <SubmitButton isSubmitting={isSubmitting} label={t('auth.login.submit')} loadingLabel={t('auth.login.submitting')} />
          </form>

          <div className="mt-6 pt-5 border-t border-surface-border flex flex-col gap-2 text-center">
            <Link to="/register" className="text-sm text-brand-600 hover:text-brand-500 transition-colors duration-150">
              {t('auth.login.noAccount')} <span className="font-semibold">{t('auth.login.register')}</span>
            </Link>
            <Link to="/request-password-reset" className="text-sm text-ink-mute hover:text-ink-body transition-colors duration-150">
              {t('auth.login.forgotPassword')}
            </Link>
            <Link to="/reset-password" className="text-sm text-ink-faint hover:text-ink-mute transition-colors duration-150">
              {t('auth.login.haveVerificationLink')}
            </Link>
            <Link to="/" className="text-sm text-ink-faint hover:text-ink-mute transition-colors duration-150">
              {t('auth.login.backToHome')}
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}
