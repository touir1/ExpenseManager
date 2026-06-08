import { useState } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import {
  IonIcon,
  IonLabel,
  IonRouterOutlet,
  IonTabBar,
  IonTabButton,
  IonTabs,
} from '@ionic/react'
import { homeOutline, receiptOutline, addCircle, peopleOutline, settingsOutline } from 'ionicons/icons'
import { useAuth } from '@/features/auth/AuthContext'
import LoginPage from '@/features/auth/pages/LoginPage'
import DashboardPage from '@/features/dashboard/pages/DashboardPage'
import ExpensesListPage from '@/features/expenses/pages/ExpensesListPage'
import QuickAddModal from '@/features/expenses/pages/QuickAddModal'
import FamiliesPage from '@/features/families/pages/FamiliesPage'
import SettingsPage from '@/features/settings/pages/SettingsPage'

function AuthGuard({ children }: Readonly<{ children: React.ReactNode }>) {
  const { isAuthenticated, isLoading } = useAuth()
  if (isLoading) return null
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return <>{children}</>
}

function TabsLayout({ onAdd }: { onAdd: () => void }) {
  return (
    <IonTabs>
      <IonRouterOutlet>
        <Routes>
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="expenses" element={<ExpensesListPage />} />
          <Route path="families" element={<FamiliesPage />} />
          <Route path="settings" element={<SettingsPage />} />
          <Route index element={<Navigate to="dashboard" replace />} />
        </Routes>
      </IonRouterOutlet>

      <IonTabBar slot="bottom">
        <IonTabButton tab="dashboard" href="/dashboard">
          <IonIcon icon={homeOutline} />
          <IonLabel>Dashboard</IonLabel>
        </IonTabButton>

        <IonTabButton tab="expenses" href="/expenses">
          <IonIcon icon={receiptOutline} />
          <IonLabel>Expenses</IonLabel>
        </IonTabButton>

        <IonTabButton tab="add" onClick={onAdd}>
          <IonIcon icon={addCircle} style={{ fontSize: '2.2rem', color: 'var(--ion-color-primary)' }} />
        </IonTabButton>

        <IonTabButton tab="families" href="/families">
          <IonIcon icon={peopleOutline} />
          <IonLabel>Family</IonLabel>
        </IonTabButton>

        <IonTabButton tab="settings" href="/settings">
          <IonIcon icon={settingsOutline} />
          <IonLabel>Settings</IonLabel>
        </IonTabButton>
      </IonTabBar>
    </IonTabs>
  )
}

export function AppRouter() {
  const [showAddModal, setShowAddModal] = useState(false)

  return (
    <>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/*"
          element={
            <AuthGuard>
              <TabsLayout onAdd={() => setShowAddModal(true)} />
            </AuthGuard>
          }
        />
      </Routes>

      <QuickAddModal isOpen={showAddModal} onClose={() => setShowAddModal(false)} />
    </>
  )
}
