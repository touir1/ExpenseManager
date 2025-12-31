import { Navigate } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export default function ProtectedRoute({ children }: { children: JSX.Element }) {
  const { isAuthenticated } = useAuth()
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }
  return children
}
