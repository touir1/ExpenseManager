import { FormEvent, useState } from 'react'
import { useAuth } from '@/auth/AuthContext'
import { useToast } from '@/components/Toast'

export default function RequestPasswordReset() {
  const [email, setEmail] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { requestPasswordReset } = useAuth()
  const { show } = useToast()

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!requestPasswordReset) {
      show('Reset request not available', 'error')
      return
    }
    setSubmitting(true)
    const ok = await requestPasswordReset(email)
    setSubmitting(false)
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

        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          <div>
            <label htmlFor="email" className="field-label">Email address</label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
              disabled={submitting}
              className="field-input"
              placeholder="you@example.com"
            />
          </div>

          <button type="submit" disabled={submitting} className="btn-primary mt-1">
            {submitting ? (
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
      </div>
    </div>
  )
}
