import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import { useToast } from '@/components/Toast'
import PasswordInput from '@/components/PasswordInput'
import AuthCard from '@/features/auth/components/AuthCard'
import AuthPageHeader from '@/features/auth/components/AuthPageHeader'
import EmailField from '@/features/auth/components/EmailField'
import SubmitButton from '@/components/SubmitButton'
import FieldError from '@/components/FieldError'
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
    <AuthCard>
      <AuthPageHeader title="Welcome back" subtitle="Sign in to your account to continue." />

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <EmailField
          registration={register('email')}
          error={errors.email?.message}
          isSubmitting={isSubmitting}
          autoFocus
        />

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
          <FieldError id="password-error" message={errors.password?.message} />
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

        <SubmitButton isSubmitting={isSubmitting} label="Login" loadingLabel="Signing in…" />
      </form>

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
    </AuthCard>
  )
}
