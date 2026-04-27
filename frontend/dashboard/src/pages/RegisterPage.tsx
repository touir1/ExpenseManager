import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'
import AuthCard from '@/components/AuthCard'
import AuthPageHeader from '@/components/AuthPageHeader'
import SubmitButton from '@/components/SubmitButton'
import FieldError from '@/components/FieldError'
import { usePageTitle } from '@/hooks/usePageTitle'
import { registerSchema, type RegisterFormData } from '@/features/auth/auth.schemas'

export default function RegisterPage() {
  usePageTitle('Register')
  const [isSuccess, setIsSuccess] = useState(false)
  const [successMsg, setSuccessMsg] = useState<string | null>(null)
  const { register: registerUser } = useAuth()

  const { register, handleSubmit, formState: { errors, isSubmitting }, setError } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  })

  const onSubmit = async (data: RegisterFormData) => {
    const { ok } = await registerUser(data.firstName, data.lastName, data.email)
    if (ok) {
      setIsSuccess(true)
      setSuccessMsg('Registration successful! Check your inbox for a verification email. Click the link to verify your address and set your password — you will then be able to log in.')
    } else {
      setError('root', { message: 'Registration failed. Please try again.' })
    }
  }

  if (isSuccess) {
    return (
      <AuthCard>
        <p className="msg-success" role="alert">{successMsg}</p>
        <div className="mt-6 pt-5 border-t border-slate-100 text-center">
          <Link to="/login" className="text-sm text-brand-600 hover:text-brand-700 transition-colors duration-150 font-medium">
            Go to login →
          </Link>
        </div>
      </AuthCard>
    )
  }

  return (
    <AuthCard>
      <AuthPageHeader title="Create an account" subtitle="Get started — it only takes a moment." />

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label htmlFor="firstName" className="field-label">First name</label>
            <input
              id="firstName"
              autoFocus
              {...register('firstName')}
              required
              disabled={isSubmitting}
              className="field-input"
              placeholder="Jane"
              aria-describedby={errors.firstName ? 'firstName-error' : undefined}
              aria-invalid={!!errors.firstName}
            />
            <FieldError id="firstName-error" message={errors.firstName?.message} />
          </div>
          <div>
            <label htmlFor="lastName" className="field-label">Last name</label>
            <input
              id="lastName"
              {...register('lastName')}
              required
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
          <label htmlFor="email" className="field-label">Email address</label>
          <input
            id="email"
            type="email"
            autoComplete="email"
            {...register('email')}
            required
            disabled={isSubmitting}
            className="field-input"
            placeholder="you@example.com"
            aria-describedby={errors.email ? 'email-error' : undefined}
            aria-invalid={!!errors.email}
          />
          <FieldError id="email-error" message={errors.email?.message} />
        </div>

        <SubmitButton isSubmitting={isSubmitting} label="Register" loadingLabel="Submitting…" />
      </form>

      {errors.root && (
        <p className="mt-4 msg-error" role="alert">
          {errors.root.message}
        </p>
      )}

      <div className="mt-6 pt-5 border-t border-slate-100 text-center">
        <Link to="/login" className="text-sm text-brand-600 hover:text-brand-700 transition-colors duration-150">
          Already have an account? <span className="font-medium">Go to login</span>
        </Link>
      </div>
    </AuthCard>
  )
}
