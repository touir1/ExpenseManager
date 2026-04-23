import { FormEvent, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'
import { usePageTitle } from '@/hooks/usePageTitle'

export default function Register() {
  usePageTitle('Register')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const [isSuccess, setIsSuccess] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const { register } = useAuth()

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!firstName.trim() || !lastName.trim() || !email.trim()) {
      setIsSuccess(false)
      setMessage('All fields are required.')
      return
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(email.trim())) {
      setIsSuccess(false)
      setMessage('Please enter a valid email address.')
      return
    }
    setSubmitting(true)
    const ok = await register(firstName, lastName, email)
    setSubmitting(false)
    if (ok) {
      setIsSuccess(true)
      setMessage('Registration successful! Check your inbox for a verification email. Click the link to verify your address and set your password — you will then be able to log in.')
    } else {
      setIsSuccess(false)
      setMessage('Registration failed. Please try again.')
    }
  }

  if (isSuccess) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <p className="msg-success" role="alert">{message}</p>
          <div className="mt-6 pt-5 border-t border-slate-100 text-center">
            <Link to="/login" className="text-sm text-brand-600 hover:text-brand-700 transition-colors duration-150 font-medium">
              Go to login →
            </Link>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        {/* Header */}
        <div className="mb-7">
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Create an account</h1>
          <p className="text-sm text-slate-500 mt-1">Get started — it only takes a moment.</p>
        </div>

        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label htmlFor="firstName" className="field-label">First name</label>
              <input
                id="firstName"
                value={firstName}
                onChange={e => setFirstName(e.target.value)}
                required
                disabled={submitting}
                className="field-input"
                placeholder="Jane"
              />
            </div>
            <div>
              <label htmlFor="lastName" className="field-label">Last name</label>
              <input
                id="lastName"
                value={lastName}
                onChange={e => setLastName(e.target.value)}
                required
                disabled={submitting}
                className="field-input"
                placeholder="Doe"
              />
            </div>
          </div>

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
                <svg className="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24" aria-hidden="true">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                </svg>
                Submitting…
              </span>
            ) : (
              'Register'
            )}
          </button>
        </form>

        {message && (
          <p className="mt-4 msg-error" role="alert">
            {message}
          </p>
        )}

        <div className="mt-6 pt-5 border-t border-slate-100 text-center">
          <Link to="/login" className="text-sm text-brand-600 hover:text-brand-700 transition-colors duration-150">
            Already have an account? <span className="font-medium">Go to login</span>
          </Link>
        </div>
      </div>
    </div>
  )
}
