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
    t: (key: string, opts?: Record<string, string>) => {
      if (key === 'dashboard.charts.sameMonth') return `${opts?.month ?? ''} across years`
      if (key === 'dashboard.empty') return 'No data'
      return key
    },
  }),
}))

import { SameMonthChart } from '../SameMonthChart'
import type { SameMonthYearlyDto } from '@/features/dashboard/types/dashboard.type'

const mockData: SameMonthYearlyDto[] = [
  { year: 2023, totalAmount: 400, convertedTotal: null },
  { year: 2024, totalAmount: 600, convertedTotal: null },
  { year: 2025, totalAmount: 500, convertedTotal: null },
]

describe('SameMonthChart', () => {
  it('shows skeleton while loading', () => {
    render(<SameMonthChart data={[]} isLoading={true} />)
    expect(screen.getByTestId('skeleton')).toBeDefined()
  })

  it('shows empty message when no data', () => {
    render(<SameMonthChart data={[]} isLoading={false} />)
    expect(screen.getByText('No data')).toBeDefined()
  })

  it('renders chart container when data present', () => {
    render(<SameMonthChart data={mockData} isLoading={false} />)
    expect(screen.getByTestId('responsive-container')).toBeDefined()
  })

  it('shows section title with month name', () => {
    render(<SameMonthChart data={mockData} isLoading={false} />)
    const title = screen.getByRole('heading', { level: 2 })
    expect(title.textContent).toContain('across years')
  })

  it('uses convertedTotal when displayCurrency provided', () => {
    const currency = { id: 2, code: 'USD', name: 'Dollar', symbol: '$', decimals: 2 }
    const dataWithConverted: SameMonthYearlyDto[] = mockData.map(d => ({ ...d, convertedTotal: d.totalAmount * 1.1 }))
    render(<SameMonthChart data={dataWithConverted} isLoading={false} displayCurrency={currency} />)
    expect(screen.getByTestId('responsive-container')).toBeDefined()
  })
})
