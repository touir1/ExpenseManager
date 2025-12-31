import { FormEvent, useState } from 'react'
import { useAuth } from '@/auth/AuthContext'

export default function ChangePassword() {
  const [oldPassword, setOldPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [repeatPassword, setRepeatPassword] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const { changePassword } = useAuth()

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    const ok = await changePassword(oldPassword, newPassword, repeatPassword)
    setMessage(ok ? 'Password changed.' : 'Please verify inputs.')
  }

  return (
    <div className="container">
      <h1>Change Password</h1>
      <form onSubmit={onSubmit} className="form">
        <label>Old password<input type="password" value={oldPassword} onChange={e => setOldPassword(e.target.value)} required /></label>
        <label>New password<input type="password" value={newPassword} onChange={e => setNewPassword(e.target.value)} required /></label>
        <label>Repeat new password<input type="password" value={repeatPassword} onChange={e => setRepeatPassword(e.target.value)} required /></label>
        <button type="submit">Change password</button>
      </form>
      {message && <p className="info">{message}</p>}
    </div>
  )
}
