import { FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export default function Register() {
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const { register } = useAuth()
  const navigate = useNavigate()

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    const ok = await register(firstName, lastName, email)
    if (ok) {
      setMessage('Registered successfully. You can now log in.')
      setTimeout(() => navigate('/login'), 800)
    } else {
      setMessage('Please fill all fields correctly.')
    }
  }

  return (
    <div className="container">
      <h1>Register</h1>
      <form onSubmit={onSubmit} className="form">
        <label>First name<input value={firstName} onChange={e => setFirstName(e.target.value)} required /></label>
        <label>Last name<input value={lastName} onChange={e => setLastName(e.target.value)} required /></label>
        <label>Email<input type="email" value={email} onChange={e => setEmail(e.target.value)} required /></label>
        <button type="submit">Register</button>
      </form>
      {message && <p className="info">{message}</p>}
      <div className="links">
        <Link to="/login">Go to Login</Link>
      </div>
    </div>
  )
}
