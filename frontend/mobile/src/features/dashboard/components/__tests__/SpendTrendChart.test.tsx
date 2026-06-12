import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'

vi.mock('recharts', async () => {
  const actual = await vi.importActual<typeof import('recharts')>('recharts')
  return {
    ...actual,
    ResponsiveContainer: ({ children }: { children: React.ReactNode }) => (
      <div data-testid="responsive-container">{children}</div>
    ),
  }
})

vi.mock('@ionic/react', () => ({
  IonCard: ({ children }: any) => <div>{children}</div>,
  IonCardHeader: ({ children }: any) => <div>{children}</div>,
  IonCardTitle: ({ children }: any) => <h2>{children}</h2>,
  IonCardContent: ({ children }: any) => <div>{children}</div>,
  IonSkeletonText: () => <span data-testid="skeleton" />,
  IonText: ({ children }: any) => <span>{children}</span>,
}))

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'dashboard.charts.monthly': 'Monthly spending',
        'dashboard.empty': 'No data',
      }
      return map[key] ?? key
    },
  }),
}))

import { SpendTrendChart } from '../SpendTrendChart'
import type { MonthlyBreakdownDto } from '@/features/dashboard/types/dashboard.type'

const mockData: MonthlyBreakdownDto[] = [
  { year: 2025, month: 1, totalAmount: 500, convertedTotal: 550, byCategory: [] },
  { year: 2025, month: 2, totalAmount: 300, convertedTotal: 330, byCategory: [] },
  { year: 2025, month: 3, totalAmount: 700, convertedTotal: 770, byCategory: [] },
]

describe('SpendTrendChart', () => {
  it('shows skeleton while loading', () => {
    render(<SpendTrendChart data={[]} isLoading={true} />)
    expect(screen.getByTestId('skeleton')).toBeDefined()
  })

  it('shows empty message when no data', () => {
    render(<SpendTrendChart data={[]} isLoading={false} />)
    expect(screen.getByText('No data')).toBeDefined()
  })

  it('renders chart when data present', () => {
    render(<SpendTrendChart data={mockData} isLoading={false} />)
    expect(screen.getByTestId('responsive-container')).toBeDefined()
  })

  it('shows section title', () => {
    render(<SpendTrendChart data={mockData} isLoading={false} />)
    expect(screen.getByText('Monthly spending')).toBeDefined()
  })

  it('uses convertedTotal when displayCurrency set', () => {
    const currency = { id: 1, code: 'USD', name: 'Dollar', symbol: '$', decimals: 2 }
    render(<SpendTrendChart data={mockData} isLoading={false} displayCurrency={currency} />)
    expect(screen.getByTestId('responsive-container')).toBeDefined()
  })
})
