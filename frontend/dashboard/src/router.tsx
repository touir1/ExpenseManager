import { Navigate, Routes, Route } from 'react-router-dom'
import ProtectedRoute from '@/features/auth/components/ProtectedRoute'
import PublicOnlyRoute from '@/features/auth/components/PublicOnlyRoute'
import AdminRoute from '@/features/admin/components/AdminRoute'
import AdminLayout from '@/features/admin/components/AdminLayout'
import AdminUsersPage from '@/features/admin/pages/AdminUsersPage'
import AdminCategoriesPage from '@/features/admin/pages/AdminCategoriesPage'
import AdminCurrenciesPage from '@/features/admin/pages/AdminCurrenciesPage'
import AdminRatesPage from '@/features/admin/pages/AdminRatesPage'
import AdminRateConflictsPage from '@/features/admin/pages/AdminRateConflictsPage'
import HomePublic from '@/features/public/pages/HomePublicPage'
import HomeDashboardPage from '@/features/dashboard/pages/HomeDashboardPage'
import Login from '@/features/auth/pages/LoginPage'
import Register from '@/features/auth/pages/RegisterPage'
import Settings from '@/features/dashboard/pages/SettingsPage'
import ChangePasswordPage from '@/features/auth/pages/ChangePasswordPage'
import ResetPassword from '@/features/auth/pages/ResetPasswordPage'
import RequestPasswordReset from '@/features/auth/pages/RequestPasswordResetPage'
import NotFound from '@/features/public/pages/NotFoundPage'
import VerifyError from '@/features/public/pages/VerifyErrorPage'
import FamiliesPage from '@/features/families/pages/FamiliesPage'
import AcceptInvitePage from '@/features/families/pages/AcceptInvitePage'
import ExpensesPage from '@/features/expenses/pages/ExpensesPage'
import CsvImportPage from '@/features/expenses/pages/CsvImportPage'

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
      <Route path="/families" element={<ProtectedRoute><FamiliesPage /></ProtectedRoute>} />
      <Route path="/families/accept-invite" element={<ProtectedRoute><AcceptInvitePage /></ProtectedRoute>} />
      <Route path="/expenses" element={<ProtectedRoute><ExpensesPage /></ProtectedRoute>} />
      <Route path="/expenses/add" element={<ProtectedRoute><ExpensesPage /></ProtectedRoute>} />
      <Route path="/expenses/:id/edit" element={<ProtectedRoute><ExpensesPage /></ProtectedRoute>} />
      <Route path="/expenses/import" element={<ProtectedRoute><CsvImportPage /></ProtectedRoute>} />

      {/* Admin */}
      <Route path="/admin" element={<AdminRoute><AdminLayout /></AdminRoute>}>
        <Route index element={<Navigate to="/admin/users" replace />} />
        <Route path="users" element={<AdminUsersPage />} />
        <Route path="categories" element={<AdminCategoriesPage />} />
        <Route path="currencies" element={<AdminCurrenciesPage />} />
        <Route path="rates" element={<AdminRatesPage />} />
        <Route path="rate-conflicts" element={<AdminRateConflictsPage />} />
      </Route>

      {/* Standalone public pages */}
      <Route path="/verify-error" element={<VerifyError />} />

      {/* Fallback */}
      <Route path="*" element={<NotFound />} />
    </Routes>
  )
}
