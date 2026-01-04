import { Link } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export default function HomePrivate() {
  const { user, logout } = useAuth()
  return (
    <div className="container">
      <h1>Dashboard</h1>
      <p>Welcome {user?.email || 'user'}! This is your private home page.</p>
      <div className="links">
        <Link to="/change-password">Change Password</Link>
        <button onClick={logout}>Logout</button>
      </div>
    </div>
  )
}
