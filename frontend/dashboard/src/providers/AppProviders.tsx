import { type ComponentType, type ReactNode } from 'react'
import { AuthProvider } from '@/features/auth/AuthContext'
import { ExpensesDataProvider } from '@/features/expenses/ExpensesDataContext'
import { FamilyProvider } from '@/features/families/FamilyContext'

type ProviderComponent = ComponentType<Readonly<{ children: ReactNode }>>

function composeProviders(...providers: ProviderComponent[]): ProviderComponent {
  return ({ children }) =>
    providers.reduceRight<ReactNode>((acc, Provider) => <Provider>{acc}</Provider>, children) as JSX.Element
}

export const AppProviders = composeProviders(
  AuthProvider,
  ExpensesDataProvider,
  FamilyProvider,
)
