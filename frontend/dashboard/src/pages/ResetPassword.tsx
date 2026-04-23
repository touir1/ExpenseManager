import { FormEvent, useState, useEffect } from 'react'
import { useAuth } from '@/auth/AuthContext'
import { useSearchParams, Link, useNavigate } from 'react-router-dom'
import PasswordStrength from '@/components/PasswordStrength'
import PasswordInput from '@/components/PasswordInput'
import { usePageTitle } from '@/hooks/usePageTitle'

export default function ResetPassword() {
  const [email, setEmail] = useState('')
  const [verificationHash, setVerificationHash] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [repeatPassword, setRepeatPassword] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const [isSuccess, setIsSuccess] = useState(false)
  const { resetPassword } = useAuth()
  const [params] = useSearchParams()
  const navigate = useNavigate()

  const isCreateMode = params.get('mode') === 'create'
  usePageTitle(isCreateMode ? 'Create Password' : 'Reset Password')

  useEffect(() => {
    const pEmail = params.get('email') || ''
    const pHash = params.get('h') || params.get('verificationHash') || ''
    if (pEmail) setEmail(pEmail)
    if (pHash) setVerificationHash(pHash)
  }, [params])

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!newPassword || !repeatPassword) {
      setIsSuccess(false)
      setMessage('All fields are required.')
      return
    }
    if (newPassword.length < 8) {
      setIsSuccess(false)
      setMessage('Password must be at least 8 characters.')
      return
    }
    if (newPassword !== repeatPassword) {
      setIsSuccess(false)
      setMessage('New passwords do not match.')
      return
    }
    const ok = await resetPassword(email, verificationHash, newPassword, repeatPassword)
    setIsSuccess(ok)
    if (ok) {
      setMessage(isCreateMode ? 'Password created successfully. Redirecting to home…' : 'Password reset successfully. Redirecting to home…')
    } else {
      setMessage(isCreateMode ? 'Password creation failed. Please try again.' : 'Password reset failed. Please try again.')
    }
    if (ok) {
      setNewPassword('')
      setRepeatPassword('')
      setTimeout(() => navigate('/'), 3000)
    }
  }

  const missingParams = !email || !verificationHash

  return (
    <div className="auth-page">
      <div className="auth-card">
        {/* Header */}
        <div className="mb-7">
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">
            {isCreateMode ? 'Create Password' : 'Reset Password'}
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            {isCreateMode ? 'Set a password for your new account.' : 'Enter a new password for your account.'}
          </p>
        </div>

        {missingParams && (
          <div className="msg-info mb-5" role="alert">
            Invalid or missing verification link. Please request a new reset link.
          </div>
        )}

        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          <div>
            <label htmlFor="newPassword" className="field-label">New password</label>
            <PasswordInput
              id="newPassword"
              autoComplete="new-password"
              value={newPassword}
              onChange={e => setNewPassword(e.target.value)}
              required
              disabled={missingParams}
              className="field-input"
            />
            <PasswordStrength password={newPassword} />
          </div>

          <div>
            <label htmlFor="repeatPassword" className="field-label">Repeat new password</label>
            <PasswordInput
              id="repeatPassword"
              autoComplete="new-password"
              value={repeatPassword}
              onChange={e => setRepeatPassword(e.target.value)}
              required
              disabled={missingParams}
              className="field-input"
            />
          </div>

          <button type="submit" disabled={missingParams || isSuccess} className="btn-primary mt-1">
            {isCreateMode ? 'Create' : 'Reset'}
          </button>
        </form>

        {message && (
          <p className={`mt-4 ${isSuccess ? 'msg-success' : 'msg-error'}`} role="alert">
            {message}
          </p>
        )}

        {missingParams && (
          <div className="mt-5 pt-5 border-t border-slate-100 text-center">
            <Link
              to="/request-password-reset"
              className="text-sm text-brand-600 hover:text-brand-700 font-medium transition-colors duration-150"
            >
              Request password reset
            </Link>
          </div>
        )}
      </div>
    </div>
  )
}
