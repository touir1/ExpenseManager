import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useAuth } from '@/features/auth/AuthContext'
import { useToast } from '@/components/Toast'
import AuthCard from '@/features/auth/components/AuthCard'
import AuthPageHeader from '@/features/auth/components/AuthPageHeader'
import SubmitButton from '@/components/SubmitButton'
import EmailField from '@/features/auth/components/EmailField'
import BackLink from '@/components/BackLink'
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
    <AuthCard>
      <AuthPageHeader
        title="Request Password Reset"
        subtitle="We'll send a reset link to your email if it's registered."
      />

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <EmailField
          registration={register('email')}
          error={errors.email?.message}
          isSubmitting={isSubmitting}
          autoFocus
        />

        <SubmitButton isSubmitting={isSubmitting} label="Send reset link" loadingLabel="Sending…" />
      </form>

      <p className="mt-5 text-xs text-slate-400 text-center leading-relaxed">
        You will receive an email with a verification link to reset your password.
      </p>

      <div className="mt-5 text-center">
        <BackLink to="/login">Back to login</BackLink>
      </div>
    </AuthCard>
  )
}
