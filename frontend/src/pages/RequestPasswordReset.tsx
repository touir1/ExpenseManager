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
    <div className="container">
      <h1>Request Password Reset</h1>
      <form onSubmit={onSubmit} className="form">
        <label>Email<input type="email" value={email} onChange={e => setEmail(e.target.value)} required /></label>
        <button type="submit" disabled={submitting}>{submitting ? 'Sending…' : 'Send reset link'}</button>
      </form>
      <p className="info">You will receive an email with a verification link to reset your password.</p>
    </div>
  )
}
