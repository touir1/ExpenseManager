import { useEffect } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { AuthProvider } from '@/auth/AuthContext'
import { ToastProvider, useToast } from '@/components/Toast'
import { onError } from '@/api'
import ProtectedRoute from '@/components/ProtectedRoute'
import PublicOnlyRoute from '@/components/PublicOnlyRoute'
import NavBar from '@/components/NavBar'
import HomePublic from '@/pages/HomePublic'
import HomeDashboard from '@/pages/HomeDashboard'
import Login from '@/pages/Login'
import Register from '@/pages/Register'
import ChangePassword from '@/pages/ChangePassword'
import ResetPassword from '@/pages/ResetPassword'
import RequestPasswordReset from '@/pages/RequestPasswordReset'

export default function App() {
  return (
    <div className="min-h-screen flex flex-col bg-slate-50 font-sans">
      <BrowserRouter>
        <ToastProvider>
          <ErrorBinder />
          <AuthProvider>
            <NavBar />
            <main className="flex-1 flex flex-col">
              <Routes>
                {/* Public */}
                <Route path="/" element={<PublicOnlyRoute><HomePublic /></PublicOnlyRoute>} />
                <Route path="/login" element={<PublicOnlyRoute><Login /></PublicOnlyRoute>} />
                <Route path="/register" element={<PublicOnlyRoute><Register /></PublicOnlyRoute>} />
                <Route path="/reset-password" element={<ResetPassword />} />
                <Route path="/request-password-reset" element={<RequestPasswordReset />} />

                {/* Private */}
                <Route
                  path="/home"
                  element={
                    <ProtectedRoute>
                      <HomeDashboard />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/change-password"
                  element={
                    <ProtectedRoute>
                      <ChangePassword />
                    </ProtectedRoute>
                  }
                />

                {/* Fallback */}
                <Route path="*" element={<HomePublic />} />
              </Routes>
            </main>
          </AuthProvider>
        </ToastProvider>
      </BrowserRouter>
    </div>
  )
}

function ErrorBinder() {
  const { show } = useToast()
  useEffect(() => { onError((msg) => show(msg, 'error')) }, [show])
  return null
}
