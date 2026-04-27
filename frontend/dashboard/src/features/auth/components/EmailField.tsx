import type { UseFormRegisterReturn } from 'react-hook-form'
import FieldError from '@/components/FieldError'

interface EmailFieldProps {
  readonly registration: UseFormRegisterReturn
  readonly error?: string
  readonly isSubmitting: boolean
  readonly autoFocus?: boolean
}

export default function EmailField({ registration, error, isSubmitting, autoFocus }: EmailFieldProps) {
  return (
    <div>
      <label htmlFor="email" className="field-label">Email address</label>
      <input
        id="email"
        type="email"
        autoComplete="email"
        autoFocus={autoFocus}
        {...registration}
        required
        disabled={isSubmitting}
        className="field-input"
        placeholder="you@example.com"
        aria-describedby={error ? 'email-error' : undefined}
        aria-invalid={!!error}
      />
      <FieldError id="email-error" message={error} />
    </div>
  )
}
