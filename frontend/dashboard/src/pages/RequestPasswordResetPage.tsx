import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import { useToast } from '@/components/Toast'
import { usePageTitle } from '@/hooks/usePageTitle'
import { requestPasswordResetSchema, type RequestPasswordResetFormData } from '@/features/auth/auth.schemas'

export default function RequestPasswordResetPage() {
  usePageTitle('Request Password Reset')
  const { requestPasswordReset } = useAuth()
  const { show } = useToast()

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<RequestPasswordResetFormData>({
    resolver: zodResolver(requestPasswordResetSchema),
  })

  const onSubmit = async (data: RequestPasswordResetFormData) => {
    if (!requestPasswordReset) {
      show('Reset request not available', 'error')
      return
    }
    const { ok } = await requestPasswordReset(data.email)
    if (ok) {
      show('If the email exists, a reset link has been sent.', 'success')
    } else {
      show('Please enter a valid email.', 'error')
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        {/* Header */}
        <div className="mb-7">
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Request Password Reset</h1>
          <p className="text-sm text-slate-500 mt-1">
            We'll send a reset link to your email if it's registered.
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <div>
            <label htmlFor="email" className="field-label">Email address</label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              autoFocus
              {...register('email')}
              required
              disabled={isSubmitting}
              className="field-input"
              placeholder="you@example.com"
              aria-describedby={errors.email ? 'email-error' : undefined}
              aria-invalid={!!errors.email}
            />
            {errors.email && (
              <p id="email-error" className="field-error" role="alert">
                {errors.email.message}
              </p>
            )}
          </div>

          <button type="submit" disabled={isSubmitting} className="btn-primary mt-1">
            {isSubmitting ? (
              <span className="flex items-center justify-center gap-2">
                <svg
                  className="h-4 w-4 animate-spin"
                  fill="none"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                </svg>
                Sending…
              </span>
            ) : (
              'Send reset link'
            )}
          </button>
        </form>

        <p className="mt-5 text-xs text-slate-400 text-center leading-relaxed">
          You will receive an email with a verification link to reset your password.
        </p>

        <div className="mt-5 text-center">
          <Link
            to="/login"
            className="inline-flex items-center gap-1 text-sm text-slate-500 hover:text-slate-700 transition-colors duration-150"
          >
            <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
            </svg>
            Back to login
          </Link>
        </div>
      </div>
    </div>
  )
}
