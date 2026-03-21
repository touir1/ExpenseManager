import { FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export default function Register() {
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const [isSuccess, setIsSuccess] = useState(false)
  const { register } = useAuth()
  const navigate = useNavigate()

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    const ok = await register(firstName, lastName, email)
    if (ok) {
      setIsSuccess(true)
      setMessage('Registered successfully. You can now log in.')
      setTimeout(() => navigate('/login'), 800)
    } else {
      setIsSuccess(false)
      setMessage('Please fill all fields correctly.')
    }
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
              className="field-input"
              placeholder="you@example.com"
            />
          </div>

          <button type="submit" className="btn-primary mt-1">
            Register
          </button>
        </form>

        {message && (
          <p className={`mt-4 ${isSuccess ? 'msg-success' : 'msg-error'}`} role="alert">
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
