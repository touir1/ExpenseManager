import { FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'
import PasswordInput from '@/components/PasswordInput'

export default function Login() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const navigate = useNavigate()
  const { login } = useAuth()

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    const ok = await login(email, password)
    if (!ok) return setError('Invalid credentials. Please try again.')
    navigate('/dashboard')
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        {/* Header */}
        <div className="mb-7">
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Welcome back</h1>
          <p className="text-sm text-slate-500 mt-1">Sign in to your account to continue.</p>
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
              className="field-input"
              placeholder="you@example.com"
            />
          </div>

          <div>
            <label htmlFor="password" className="field-label">Password</label>
            <PasswordInput
              id="password"
              autoComplete="current-password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
              className="field-input"
            />
          </div>

          {error && (
            <p className="msg-error" role="alert">{error}</p>
          )}

          <button type="submit" className="btn-primary mt-1">
            Login
          </button>
        </form>

        {/* Footer links */}
        <div className="mt-6 pt-5 border-t border-slate-100 flex flex-col gap-2 text-center">
          <Link to="/register" className="text-sm text-brand-600 hover:text-brand-700 transition-colors duration-150">
            Don't have an account? <span className="font-medium">Register</span>
          </Link>
          <Link to="/request-password-reset" className="text-sm text-slate-500 hover:text-slate-700 transition-colors duration-150">
            Forgot your password?
          </Link>
          <Link to="/reset-password" className="text-sm text-slate-400 hover:text-slate-600 transition-colors duration-150">
            Have a verification link?
          </Link>
          <Link to="/" className="text-sm text-slate-400 hover:text-slate-600 transition-colors duration-150">
            ← Back to home
          </Link>
        </div>
      </div>
    </div>
  )
}
