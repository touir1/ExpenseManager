import { useEffect } from 'react'
import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from '@/features/auth/AuthContext'
import { ToastProvider, useToast } from '@/components/Toast'
import { onError } from '@/services/api'
import NavBar from '@/layouts/NavBar'
import AppRoutes from '@/router'

export default function App() {
  return (
    <div className="min-h-screen flex flex-col bg-slate-50 font-sans">
      <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <ToastProvider>
          <ErrorBinder />
          <AuthProvider>
            <NavBar />
            <main className="flex-1 flex flex-col">
              <AppRoutes />
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
