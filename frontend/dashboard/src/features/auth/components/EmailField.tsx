import { useTranslation } from 'react-i18next'
import type { UseFormRegisterReturn } from 'react-hook-form'
import FieldError from '@/components/FieldError'

interface EmailFieldProps {
  readonly registration: UseFormRegisterReturn
  readonly error?: string
  readonly isSubmitting: boolean
  readonly autoFocus?: boolean
}

export default function EmailField({ registration, error, isSubmitting, autoFocus }: EmailFieldProps) {
  const { t } = useTranslation()
  return (
    <div>
      <label htmlFor="email" className="field-label">{t('auth.email.label')}</label>
      <input
        id="email"
        type="email"
        autoComplete="email"
        autoFocus={autoFocus}
        {...registration}
        required
        disabled={isSubmitting}
        className="field-input"
        placeholder={t('auth.email.placeholder')}
        aria-describedby={error ? 'email-error' : undefined}
        aria-invalid={!!error}
      />
      <FieldError id="email-error" message={error} />
    </div>
  )
}
