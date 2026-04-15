import { FormEvent, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export default function ChangePassword() {
  const [oldPassword, setOldPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [repeatPassword, setRepeatPassword] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const [isSuccess, setIsSuccess] = useState(false)
  const { changePassword } = useAuth()

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!oldPassword || !newPassword || !repeatPassword) {
      setIsSuccess(false)
      setMessage('All fields are required.')
      return
    }
    if (newPassword !== repeatPassword) {
      setIsSuccess(false)
      setMessage('New passwords do not match.')
      return
    }
    const ok = await changePassword(oldPassword, newPassword, repeatPassword)
    setIsSuccess(ok)
    setMessage(ok ? 'Password changed.' : 'Incorrect current password.')
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        {/* Back link */}
        <Link
          to="/settings"
          className="inline-flex items-center gap-1 text-sm text-slate-500 hover:text-slate-700 mb-6 transition-colors duration-150"
        >
          <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
            <path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" />
          </svg>
          Back to settings
        </Link>

        {/* Header */}
        <div className="mb-7">
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Change Password</h1>
          <p className="text-sm text-slate-500 mt-1">Update your account password below.</p>
        </div>

        <form onSubmit={onSubmit} className="space-y-4" noValidate>
          <div>
            <label htmlFor="oldPassword" className="field-label">Old password</label>
            <input
              id="oldPassword"
              type="password"
              autoComplete="current-password"
              value={oldPassword}
              onChange={e => setOldPassword(e.target.value)}
              required
              className="field-input"
              placeholder="••••••••"
            />
          </div>

          <div>
            <label htmlFor="newPassword" className="field-label">New password</label>
            <input
              id="newPassword"
              type="password"
              autoComplete="new-password"
              value={newPassword}
              onChange={e => setNewPassword(e.target.value)}
              required
              className="field-input"
              placeholder="••••••••"
            />
          </div>

          <div>
            <label htmlFor="repeatPassword" className="field-label">Repeat new password</label>
            <input
              id="repeatPassword"
              type="password"
              autoComplete="new-password"
              value={repeatPassword}
              onChange={e => setRepeatPassword(e.target.value)}
              required
              className="field-input"
              placeholder="••••••••"
            />
          </div>

          <button type="submit" className="btn-primary mt-1">
            Change password
          </button>
        </form>

        {message && (
          <p className={`mt-4 ${isSuccess ? 'msg-success' : 'msg-error'}`} role="alert">
            {message}
          </p>
        )}
      </div>
    </div>
  )
}
