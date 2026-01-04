import { Link } from 'react-router-dom'

export default function HomePublic() {
  return (
    <div className="container">
      <h1>Welcome</h1>
      <p>This is the public home page.</p>
      <div className="links" style={{ display: 'grid', gap: 8 }}>
        <Link to="/login">Login</Link>
        <Link to="/register">Register</Link>
      </div>
    </div>
  )
}
