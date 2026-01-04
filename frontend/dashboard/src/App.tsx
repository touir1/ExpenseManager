import { useEffect } from 'react'
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import { AuthProvider } from '@/auth/AuthContext'
import { ToastProvider, useToast } from '@/components/Toast'
import { onError } from '@/api'
import ProtectedRoute from '@/components/ProtectedRoute'
import HomePublic from '@/pages/HomePublic'
import HomeDashboard from '@/pages/HomeDashboard'
import Login from '@/pages/Login'
import Register from '@/pages/Register'
import ChangePassword from '@/pages/ChangePassword'
import ResetPassword from '@/pages/ResetPassword'
import RequestPasswordReset from '@/pages/RequestPasswordReset'

export default function App() {
  return (
    <div style={{ minHeight: '100vh', background: '#0f172a', color: 'white' }}>
      <BrowserRouter>
        <ToastProvider>
        <ErrorBinder />
        <AuthProvider>
          <header style={{ display: 'flex', gap: 12, alignItems: 'center', padding: 12, borderBottom: '1px solid rgba(255,255,255,0.12)' }}>
            <strong>Expenses Manager</strong>
            <nav style={{ display: 'flex', gap: 8 }}>
              <Link to="/" style={{ color: 'white' }}>Home</Link>
              <Link to="/login" style={{ color: 'white' }}>Login</Link>
              <Link to="/register" style={{ color: 'white' }}>Register</Link>
            </nav>
          </header>
          <main style={{ padding: 24 }}>
            <Routes>
              {/* Public */}
              <Route path="/" element={<HomePublic />} />
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/reset-password" element={<ResetPassword />} />
              <Route path="/request-password-reset" element={<RequestPasswordReset />} />

              {/* Private */}
              <Route path="/home-auth" element={
                <ProtectedRoute>
                  <HomeDashboard />
                </ProtectedRoute>
              } />
              <Route path="/change-password" element={
                <ProtectedRoute>
                  <ChangePassword />
                </ProtectedRoute>
              } />

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
  // bind once
  useEffect(() => { onError((msg) => show(msg, 'error')) }, [show])
  return null
}
