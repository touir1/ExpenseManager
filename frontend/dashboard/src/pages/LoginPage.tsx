import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import { useToast } from '@/components/Toast'
import PasswordInput from '@/components/PasswordInput'
import { usePageTitle } from '@/hooks/usePageTitle'
import { loginSchema, type LoginFormData } from '@/features/auth/auth.schemas'

export default function LoginPage() {
  usePageTitle('Login')
  const navigate = useNavigate()
  const { login } = useAuth()
  const { show } = useToast()

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '', rememberMe: false },
  })

  const onSubmit = async (data: LoginFormData) => {
    const { ok } = await login(data.email, data.password, data.rememberMe ?? false)
    if (!ok) {
      show('Invalid credentials. Please try again.', 'error')
      return
    }
    navigate('/dashboard')
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        {/* Header */}
        <div className="mb-7">
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Welcome back</h1>
          <p className="text-sm text-slate-500 mt-1">Sign in to your account to continue.</p>
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

          <div>
            <label htmlFor="password" className="field-label">Password</label>
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
            {errors.password && (
              <p id="password-error" className="field-error" role="alert">
                {errors.password.message}
              </p>
            )}
          </div>

          <div className="flex items-center gap-2">
            <input
              id="rememberMe"
              type="checkbox"
              {...register('rememberMe')}
              disabled={isSubmitting}
              className="h-4 w-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500 cursor-pointer"
            />
            <label htmlFor="rememberMe" className="text-sm text-slate-600 cursor-pointer select-none">
              Remember me
            </label>
          </div>

          <button type="submit" disabled={isSubmitting} className="btn-primary mt-1">
            {isSubmitting ? (
              <span className="flex items-center justify-center gap-2">
                <svg className="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24" aria-hidden="true">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                </svg>
                Signing in…
              </span>
            ) : (
              'Login'
            )}
          </button>
        </form>

        {/* Footer links */}
        <div className="mt-6 pt-5 border-t border-slate-100 flex flex-col gap-2 text-center">
          <Link to="/register" className="text-sm text-brand-600 hover:text-brand-700 transition-colors duration-150">
            Don't have an account? <span className="font-medium">Register</span>
          </Link>
          <Link to="/request-password-reset" className="text-sm text-slate-500 hover:text-slate-700 transition-colors duration-150">
            Forgot your password?
          </Link>
          <Link to="/reset-password" className="text-sm text-slate-400 hover:text-slate-600 transition-colors duration-150">
            Have a verification link?
          </Link>
          <Link to="/" className="text-sm text-slate-400 hover:text-slate-600 transition-colors duration-150">
            ← Back to home
          </Link>
        </div>
      </div>
    </div>
  )
}
