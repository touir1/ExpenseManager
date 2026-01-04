import { FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export default function Login() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const navigate = useNavigate()
  const { login } = useAuth()

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    const ok = await login(email, password)
    if (!ok) return setError('Invalid credentials')
    navigate('/home-auth')
  }

  return (
    <div className="container">
      <h1>Login</h1>
      <form onSubmit={onSubmit} className="form">
        <label>Email<input type="email" value={email} onChange={e => setEmail(e.target.value)} required /></label>
        <label>Password<input type="password" value={password} onChange={e => setPassword(e.target.value)} required /></label>
        {error && <p className="error">{error}</p>}
        <button type="submit">Login</button>
      </form>
      <div className="links" style={{ display: 'grid', gap: 8 }}>
        <Link to="/register">Create account</Link>
        <Link to="/request-password-reset">Request password reset</Link>
        <Link to="/reset-password">Use verification link</Link>
        <Link to="/">Back to Home</Link>
      </div>
    </div>
  )
}
