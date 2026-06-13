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
    t: (key: string, fallback?: string) => {
      const map: Record<string, string> = {
        'dashboard.charts.categories': 'By category',
        'dashboard.empty': 'No data',
        'expenses.uncategorised': 'Uncategorised',
        'dashboard.charts.other': 'Other',
      }
      return map[key] ?? fallback ?? key
    },
  }),
}))

import { CategoryPieChart } from '../CategoryPieChart'
import type { CategoryBreakdownDto } from '@/features/dashboard/types/dashboard.type'

function makeCategory(id: number, name: string, amount: number, pct: number): CategoryBreakdownDto {
  return {
    category: { id, name, description: undefined, icon: undefined },
    totalAmount: amount,
    convertedTotal: null,
    percentage: pct,
    subcategories: [],
  }
}

const mockData: CategoryBreakdownDto[] = [
  makeCategory(1, 'Food', 500, 50),
  makeCategory(2, 'Transport', 300, 30),
  makeCategory(3, 'Health', 200, 20),
]

const manyData: CategoryBreakdownDto[] = [
  ...mockData,
  makeCategory(4, 'Entertainment', 100, 10),
  makeCategory(5, 'Shopping', 80, 8),
  makeCategory(6, 'Utilities', 60, 6),
  makeCategory(7, 'Other stuff', 40, 4),
]

describe('CategoryPieChart', () => {
  it('shows skeleton while loading', () => {
    render(<CategoryPieChart data={[]} isLoading={true} />)
    expect(screen.getByTestId('skeleton')).toBeDefined()
  })

  it('shows empty message when no data', () => {
    render(<CategoryPieChart data={[]} isLoading={false} />)
    expect(screen.getByText('No data')).toBeDefined()
  })

  it('renders category names', () => {
    render(<CategoryPieChart data={mockData} isLoading={false} />)
    expect(screen.getByText('Food')).toBeDefined()
    expect(screen.getByText('Transport')).toBeDefined()
    expect(screen.getByText('Health')).toBeDefined()
  })

  it('shows percentages', () => {
    render(<CategoryPieChart data={mockData} isLoading={false} />)
    expect(screen.getByText('50%')).toBeDefined()
    expect(screen.getByText('30%')).toBeDefined()
  })

  it('groups overflow into Other when more than 6 categories', () => {
    render(<CategoryPieChart data={manyData} isLoading={false} />)
    expect(screen.getByText('Other')).toBeDefined()
    // 7th category name should NOT appear
    expect(screen.queryByText('Other stuff')).toBeNull()
  })

  it('shows section title', () => {
    render(<CategoryPieChart data={mockData} isLoading={false} />)
    expect(screen.getByText('By category')).toBeDefined()
  })
})
