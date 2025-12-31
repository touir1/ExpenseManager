import { FormEvent, useState, useEffect } from 'react'
import { useAuth } from '@/auth/AuthContext'
import { useSearchParams, Link } from 'react-router-dom'

export default function ResetPassword() {
  const [email, setEmail] = useState('')
  const [verificationHash, setVerificationHash] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [repeatPassword, setRepeatPassword] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const { resetPassword } = useAuth()
  const [params] = useSearchParams()

  useEffect(() => {
    const pEmail = params.get('email') || ''
    const pHash = params.get('h') || params.get('verificationHash') || ''
    if (pEmail) setEmail(pEmail)
    if (pHash) setVerificationHash(pHash)
  }, [params])

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    const ok = await resetPassword(email, verificationHash, newPassword, repeatPassword)
    setMessage(ok ? 'Password reset.' : 'Please fill all fields correctly and ensure passwords match.')
  }

  return (
    <div className="container">
      <h1>Reset Password</h1>
      {!email || !verificationHash ? (
        <p className="info">Invalid or missing verification link. Please request a new reset link.</p>
      ) : null}
      <form onSubmit={onSubmit} className="form">
        <label>New password<input type="password" value={newPassword} onChange={e => setNewPassword(e.target.value)} required /></label>
        <label>Repeat new password<input type="password" value={repeatPassword} onChange={e => setRepeatPassword(e.target.value)} required /></label>
        <button type="submit" disabled={!email || !verificationHash}>Reset</button>
      </form>
      {!email || !verificationHash ? (
        <div className="links" style={{ marginTop: 8 }}>
          <Link to="/request-password-reset">Request password reset</Link>
        </div>
      ) : null}
      {message && <p className="info">{message}</p>}
    </div>
  )
}
