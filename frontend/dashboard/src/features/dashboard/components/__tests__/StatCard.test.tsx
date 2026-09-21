import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { StatCard } from '../StatCard'

describe('StatCard', () => {
  it('renders label and value', () => {
    render(<StatCard label="Total spending" value="$100.00" />)
    expect(screen.getByText('Total spending')).toBeInTheDocument()
    expect(screen.getByText('$100.00')).toBeInTheDocument()
  })

  it('shows a skeleton when loading', () => {
    render(<StatCard label="Total spending" value="$100.00" isLoading />)
    expect(screen.getByRole('status')).toBeInTheDocument()
    expect(screen.queryByText('Total spending')).not.toBeInTheDocument()
  })

  it('renders a positive trend chip with title', () => {
    render(<StatCard label="vs prev." value="+8.0%" trend={{ text: '↑', positive: true, title: 'Compared to Aug 1 – Aug 31' }} />)
    const chip = screen.getByText('↑')
    expect(chip).toHaveClass('bg-sage-soft', 'text-sage')
    expect(chip).toHaveAttribute('title', 'Compared to Aug 1 – Aug 31')
  })

  it('renders a negative trend chip', () => {
    render(<StatCard label="vs prev." value="-3.0%" trend={{ text: '↓', positive: false }} />)
    expect(screen.getByText('↓')).toHaveClass('bg-berry-soft', 'text-berry')
  })

  it('renders subtext when provided', () => {
    render(<StatCard label="Avg per day" value="$5.00" subtext="over 30 days" />)
    expect(screen.getByText('over 30 days')).toBeInTheDocument()
  })
})
