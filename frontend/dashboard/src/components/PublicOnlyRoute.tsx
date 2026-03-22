import { Navigate } from 'react-router-dom'
import { useAuth } from '@/auth/AuthContext'

export default function PublicOnlyRoute({ children }: Readonly<{ children: JSX.Element }>) {
  const { isAuthenticated, isLoading } = useAuth()
  if (isLoading) return null
  if (isAuthenticated) return <Navigate to="/home" replace />
  return children
}
