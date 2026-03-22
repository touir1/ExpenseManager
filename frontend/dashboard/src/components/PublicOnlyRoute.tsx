import { Navigate } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export default function PublicOnlyRoute({ children }: { children: JSX.Element }) {
  const { isAuthenticated } = useAuth()
  if (isAuthenticated) {
    return <Navigate to="/home" replace />
  }
  return children
}
