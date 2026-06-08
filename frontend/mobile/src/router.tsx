import { useState } from 'react'
import { Redirect, Route } from 'react-router-dom'
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
  if (!isAuthenticated) return <Redirect to="/login" />
  return <>{children}</>
}

export function AppRouter() {
  const [showAddModal, setShowAddModal] = useState(false)

  return (
    <>
      <IonRouterOutlet id="main">
        <Route path="/login" exact>
          <LoginPage />
        </Route>
        <Route path="/">
          <AuthGuard>
            <IonTabs>
              <IonRouterOutlet>
                <Route path="/dashboard" exact>
                  <DashboardPage />
                </Route>
                <Route path="/expenses" exact>
                  <ExpensesListPage />
                </Route>
                <Route path="/families" exact>
                  <FamiliesPage />
                </Route>
                <Route path="/settings" exact>
                  <SettingsPage />
                </Route>
                <Redirect exact from="/" to="/dashboard" />
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

                <IonTabButton tab="add" onClick={() => setShowAddModal(true)}>
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
          </AuthGuard>
        </Route>
      </IonRouterOutlet>

      <QuickAddModal isOpen={showAddModal} onClose={() => setShowAddModal(false)} />
    </>
  )
}
