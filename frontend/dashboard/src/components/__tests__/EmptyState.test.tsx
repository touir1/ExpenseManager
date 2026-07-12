import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import EmptyState from '../EmptyState'

describe('EmptyState', () => {
  it('renders title and decorative icon', () => {
    render(<EmptyState title="Nothing here yet" />)
    expect(screen.getByText('Nothing here yet')).toBeInTheDocument()
    const icon = screen.getByText('📭')
    expect(icon).toHaveAttribute('aria-hidden', 'true')
  })

  it('renders a custom icon', () => {
    render(<EmptyState icon="🧾" title="No expenses" />)
    expect(screen.getByText('🧾')).toBeInTheDocument()
  })

  it('renders subtitle when provided', () => {
    render(<EmptyState title="Title" subtitle="Some helpful subtitle" />)
    expect(screen.getByText('Some helpful subtitle')).toBeInTheDocument()
  })

  it('does not render subtitle when absent', () => {
    render(<EmptyState title="Title" />)
    expect(screen.queryByText('Some helpful subtitle')).not.toBeInTheDocument()
  })

  it('renders an action button and fires onClick', () => {
    const onClick = vi.fn()
    render(<EmptyState title="Title" action={{ label: 'Add expense', onClick }} />)
    screen.getByRole('button', { name: 'Add expense' }).click()
    expect(onClick).toHaveBeenCalledOnce()
  })

  it('renders an action as a router Link when "to" is provided', () => {
    render(
      <MemoryRouter>
        <EmptyState title="Title" action={{ label: 'Add expense', to: '/expenses/add' }} />
      </MemoryRouter>
    )
    const link = screen.getByRole('link', { name: 'Add expense' })
    expect(link).toHaveAttribute('href', '/expenses/add')
  })

  it('does not render an action when absent', () => {
    render(<EmptyState title="Title" />)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
    expect(screen.queryByRole('link')).not.toBeInTheDocument()
  })

  it('applies compact sizing classes when compact is true', () => {
    const { container } = render(<EmptyState title="Title" compact />)
    expect(container.querySelector('.text-3xl')).toBeInTheDocument()
  })
})
