import { BrowserRouter } from 'react-router-dom'
import { AppProviders } from '@/providers/AppProviders'
import NavBar from '@/layouts/NavBar'
import AppRoutes from '@/router'
import { onError } from '@/services/api.service'
import { useEffect } from 'react'
import { ToastProvider, useToast } from '@/components/Toast'

function ErrorBinder() {
  const { show } = useToast()
  useEffect(() => { onError((msg) => show(msg, 'error')) }, [show])
  return null
}

export default function App() {
  return (
    <div className="min-h-screen flex flex-col bg-slate-50 font-sans">
      <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
      <ToastProvider>
          <ErrorBinder />
          <AppProviders>
            <NavBar />
            <main className="flex-1 flex flex-col">
              <AppRoutes />
            </main>
          </AppProviders>
      </ToastProvider>
      </BrowserRouter>
    </div>
  )
}
