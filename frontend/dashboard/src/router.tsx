import { Routes, Route } from 'react-router-dom'
import ProtectedRoute from '@/features/auth/ProtectedRoute'
import PublicOnlyRoute from '@/features/auth/PublicOnlyRoute'
import HomePublic from '@/pages/HomePublic'
import HomeDashboard from '@/pages/HomeDashboard'
import Login from '@/pages/Login'
import Register from '@/pages/Register'
import Settings from '@/pages/Settings'
import ChangePassword from '@/pages/ChangePassword'
import ResetPassword from '@/pages/ResetPassword'
import RequestPasswordReset from '@/pages/RequestPasswordReset'
import NotFound from '@/pages/NotFound'

export default function AppRoutes() {
  return (
    <Routes>
      {/* Public */}
      <Route path="/" element={<PublicOnlyRoute><HomePublic /></PublicOnlyRoute>} />
      <Route path="/login" element={<PublicOnlyRoute><Login /></PublicOnlyRoute>} />
      <Route path="/register" element={<PublicOnlyRoute><Register /></PublicOnlyRoute>} />
      <Route path="/reset-password" element={<PublicOnlyRoute><ResetPassword /></PublicOnlyRoute>} />
      <Route path="/request-password-reset" element={<PublicOnlyRoute><RequestPasswordReset /></PublicOnlyRoute>} />

      {/* Private */}
      <Route path="/dashboard" element={<ProtectedRoute><HomeDashboard /></ProtectedRoute>} />
      <Route path="/settings" element={<ProtectedRoute><Settings /></ProtectedRoute>} />
      <Route path="/change-password" element={<ProtectedRoute><ChangePassword /></ProtectedRoute>} />

      {/* Fallback */}
      <Route path="*" element={<NotFound />} />
    </Routes>
  )
}
