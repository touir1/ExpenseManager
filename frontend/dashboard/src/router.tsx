import { createBrowserRouter, Navigate } from 'react-router-dom'
import type { RouteObject } from 'react-router-dom'
import RootLayout from '@/layouts/RootLayout'
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

export const routes: RouteObject[] = [
  {
    element: <RootLayout />,
    children: [
      { path: '/', element: <PublicOnlyRoute><HomePublic /></PublicOnlyRoute> },
      { path: '/login', element: <PublicOnlyRoute><Login /></PublicOnlyRoute> },
      { path: '/register', element: <PublicOnlyRoute><Register /></PublicOnlyRoute> },
      { path: '/reset-password', element: <PublicOnlyRoute><ResetPassword /></PublicOnlyRoute> },
      { path: '/request-password-reset', element: <PublicOnlyRoute><RequestPasswordReset /></PublicOnlyRoute> },

      { path: '/dashboard', element: <ProtectedRoute><HomeDashboardPage /></ProtectedRoute> },
      { path: '/settings', element: <ProtectedRoute><Settings /></ProtectedRoute> },
      { path: '/change-password', element: <ProtectedRoute><ChangePasswordPage /></ProtectedRoute> },
      { path: '/families', element: <ProtectedRoute><FamiliesPage /></ProtectedRoute> },
      { path: '/families/accept-invite', element: <ProtectedRoute><AcceptInvitePage /></ProtectedRoute> },
      { path: '/expenses', element: <ProtectedRoute><ExpensesPage /></ProtectedRoute> },
      { path: '/expenses/add', element: <ProtectedRoute><ExpensesPage /></ProtectedRoute> },
      { path: '/expenses/:id/edit', element: <ProtectedRoute><ExpensesPage /></ProtectedRoute> },
      { path: '/expenses/import', element: <ProtectedRoute><CsvImportPage /></ProtectedRoute> },

      {
        path: '/admin',
        element: <AdminRoute><AdminLayout /></AdminRoute>,
        children: [
          { index: true, element: <Navigate to="/admin/users" replace /> },
          { path: 'users', element: <AdminUsersPage /> },
          { path: 'categories', element: <AdminCategoriesPage /> },
          { path: 'currencies', element: <AdminCurrenciesPage /> },
          { path: 'rates', element: <AdminRatesPage /> },
          { path: 'rate-conflicts', element: <AdminRateConflictsPage /> },
        ],
      },

      { path: '/verify-error', element: <VerifyError /> },
      { path: '*', element: <NotFound /> },
    ],
  },
]

export const router = createBrowserRouter(routes, {
  future: { v7_relativeSplatPath: true },
})
