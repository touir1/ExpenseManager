import { useState, useEffect } from 'react'
import { Outlet } from 'react-router-dom'
import { AppProviders } from '@/providers/AppProviders'
import { useAuth } from '@/features/auth/AuthContext'
import MarketingHeader from '@/layouts/MarketingHeader'
import SideNav from '@/layouts/SideNav'
import AppHeader from '@/layouts/AppHeader'
import { onError } from '@/services/api.service'
import { ToastProvider, useToast } from '@/components/Toast'

function ErrorBinder() {
  const { show } = useToast()
  useEffect(() => { onError((msg) => show(msg, 'error')) }, [show])
  return null
}

function Shell() {
  const { isAuthenticated } = useAuth()
  const [mobileNavOpen, setMobileNavOpen] = useState(false)

  if (!isAuthenticated) {
    return (
      <>
        <MarketingHeader />
        <main className="flex-1 flex flex-col">
          <Outlet />
        </main>
      </>
    )
  }

  return (
    <div className="flex min-h-screen">
      <SideNav isMobileOpen={mobileNavOpen} onMobileClose={() => setMobileNavOpen(false)} />
      <div className="flex-1 flex flex-col min-w-0">
        <AppHeader onOpenMobileNav={() => setMobileNavOpen(true)} />
        <main className="flex-1 flex flex-col">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

export default function RootLayout() {
  return (
    <ToastProvider>
      <ErrorBinder />
      <AppProviders>
        <Shell />
      </AppProviders>
    </ToastProvider>
  )
}
