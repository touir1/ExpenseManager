import { Routes, Route } from 'react-router-dom'
import ProtectedRoute from '@/features/auth/ProtectedRoute'
import PublicOnlyRoute from '@/features/auth/PublicOnlyRoute'
import HomePublic from '@/pages/HomePublicPage'
import HomeDashboardPage from '@/pages/HomeDashboardPage'
import Login from '@/pages/LoginPage'
import Register from '@/pages/RegisterPage'
import Settings from '@/pages/SettingsPage'
import ChangePasswordPage from '@/pages/ChangePasswordPage'
import ResetPassword from '@/pages/ResetPasswordPage'
import RequestPasswordReset from '@/pages/RequestPasswordResetPage'
import NotFound from '@/pages/NotFoundPage'

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
      <Route path="/dashboard" element={<ProtectedRoute><HomeDashboardPage /></ProtectedRoute>} />
      <Route path="/settings" element={<ProtectedRoute><Settings /></ProtectedRoute>} />
      <Route path="/change-password" element={<ProtectedRoute><ChangePasswordPage /></ProtectedRoute>} />

      {/* Fallback */}
      <Route path="*" element={<NotFound />} />
    </Routes>
  )
}
