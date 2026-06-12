import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'

vi.mock('@ionic/react', () => ({
  IonCard: ({ children }: any) => <div>{children}</div>,
  IonCardHeader: ({ children }: any) => <div>{children}</div>,
  IonCardTitle: ({ children }: any) => <h2>{children}</h2>,
  IonList: ({ children }: any) => <ul>{children}</ul>,
  IonItem: ({ children }: any) => <li>{children}</li>,
  IonLabel: ({ children }: any) => <span>{children}</span>,
  IonText: ({ children, slot }: any) => <span data-slot={slot}>{children}</span>,
  IonSkeletonText: () => <span data-testid="skeleton" />,
}))

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'dashboard.charts.currencies': 'By currency',
        'dashboard.noCurrencies': 'No currencies',
        'dashboard.summary.expenses': 'expenses',
      }
      return map[key] ?? key
    },
  }),
}))

import { CurrenciesPanel } from '../CurrenciesPanel'
import type { CurrencyBreakdownDto } from '@/features/dashboard/types/dashboard.type'

const mockData: CurrencyBreakdownDto[] = [
  {
    currency: { id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
    totalAmount: 1200,
    convertedAmount: null,
    expenseCount: 15,
  },
  {
    currency: { id: 2, code: 'USD', name: 'Dollar', symbol: '$', decimals: 2 },
    totalAmount: 300,
    convertedAmount: null,
    expenseCount: 5,
  },
]

describe('CurrenciesPanel', () => {
  it('shows skeletons while loading', () => {
    render(<CurrenciesPanel data={[]} isLoading={true} />)
    expect(screen.getAllByTestId('skeleton').length).toBeGreaterThan(0)
  })

  it('shows empty message when no data', () => {
    render(<CurrenciesPanel data={[]} isLoading={false} />)
    expect(screen.getByText('No currencies')).toBeDefined()
  })

  it('renders currency codes', () => {
    render(<CurrenciesPanel data={mockData} isLoading={false} />)
    expect(screen.getByText('EUR')).toBeDefined()
    expect(screen.getByText('USD')).toBeDefined()
  })

  it('renders expense counts', () => {
    render(<CurrenciesPanel data={mockData} isLoading={false} />)
    expect(screen.getByText('15 expenses')).toBeDefined()
    expect(screen.getByText('5 expenses')).toBeDefined()
  })

  it('shows section title', () => {
    render(<CurrenciesPanel data={mockData} isLoading={false} />)
    expect(screen.getByText('By currency')).toBeDefined()
  })

  it('shows converted amount when displayCurrency provided', () => {
    const displayCurrency = { id: 3, code: 'TND', name: 'Dinar', symbol: 'DT', decimals: 3 }
    const dataWithConverted: CurrencyBreakdownDto[] = mockData.map(d => ({ ...d, convertedAmount: d.totalAmount * 3 }))
    render(<CurrenciesPanel data={dataWithConverted} isLoading={false} displayCurrency={displayCurrency} />)
    // convertedAmount used: 1200*3 = 3600
    expect(screen.getByText(/3600/)).toBeDefined()
  })
})
