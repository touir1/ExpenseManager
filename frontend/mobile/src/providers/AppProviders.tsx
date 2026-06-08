import { type ComponentType, type ReactNode } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AuthProvider } from '@/features/auth/AuthContext'
import { ExpensesDataProvider } from '@/features/expenses/ExpensesDataContext'
import { FamilyProvider } from '@/features/families/FamilyContext'
import { DisplayCurrencyProvider } from '@/features/currencies/DisplayCurrencyContext'
import { NotificationProvider } from '@/features/notifications/NotificationContext'

const queryClient = new QueryClient()

type ProviderComponent = ComponentType<Readonly<{ children: ReactNode }>>

function composeProviders(...providers: ProviderComponent[]): ProviderComponent {
  return ({ children }) =>
    providers.reduceRight<ReactNode>((acc, Provider) => <Provider>{acc}</Provider>, children) as JSX.Element
}

const InnerProviders = composeProviders(
  AuthProvider,
  ExpensesDataProvider,
  FamilyProvider,
  DisplayCurrencyProvider,
  NotificationProvider,
)

export function AppProviders({ children }: Readonly<{ children: ReactNode }>) {
  return (
    <QueryClientProvider client={queryClient}>
      <InnerProviders>{children}</InnerProviders>
    </QueryClientProvider>
  )
}
