import { Navigate } from 'react-router-dom'
import { useAuth } from '@/features/auth/AuthContext'

export default function AdminRoute({ children }: Readonly<{ children: JSX.Element }>) {
  const { isAuthenticated, isLoading, user } = useAuth()
  if (isLoading) return null
  if (!isAuthenticated) return <Navigate to="/login" replace />
  if (!user?.isAdmin) return <Navigate to="/dashboard" replace />
  return children
}
