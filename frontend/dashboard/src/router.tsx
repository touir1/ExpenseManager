import { Routes, Route } from 'react-router-dom'
import ProtectedRoute from '@/features/auth/components/ProtectedRoute'
import PublicOnlyRoute from '@/features/auth/components/PublicOnlyRoute'
import HomePublic from '@/features/public/pages/HomePublicPage'
import HomeDashboardPage from '@/features/dashboard/pages/HomeDashboardPage'
import Login from '@/features/auth/pages/LoginPage'
import Register from '@/features/auth/pages/RegisterPage'
import Settings from '@/features/dashboard/pages/SettingsPage'
import ChangePasswordPage from '@/features/auth/pages/ChangePasswordPage'
import ResetPassword from '@/features/auth/pages/ResetPasswordPage'
import RequestPasswordReset from '@/features/auth/pages/RequestPasswordResetPage'
import NotFound from '@/features/public/pages/NotFoundPage'

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
