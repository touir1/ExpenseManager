import { Outlet } from 'react-router-dom'
import { AppProviders } from '@/providers/AppProviders'
import NavBar from '@/layouts/NavBar'
import { onError } from '@/services/api.service'
import { useEffect } from 'react'
import { ToastProvider, useToast } from '@/components/Toast'

function ErrorBinder() {
  const { show } = useToast()
  useEffect(() => { onError((msg) => show(msg, 'error')) }, [show])
  return null
}

export default function RootLayout() {
  return (
    <ToastProvider>
      <ErrorBinder />
      <AppProviders>
        <NavBar />
        <main className="flex-1 flex flex-col">
          <Outlet />
        </main>
      </AppProviders>
    </ToastProvider>
  )
}
