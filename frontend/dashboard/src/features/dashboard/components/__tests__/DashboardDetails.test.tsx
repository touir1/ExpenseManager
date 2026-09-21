import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { DashboardDetails } from '../DashboardDetails'

function renderDetails() {
  return render(
    <DashboardDetails
      recent={<div>Recent panel content</div>}
      largest={<div>Largest panel content</div>}
      recurring={<div>Recurring panel content</div>}
    />
  )
}

describe('DashboardDetails', () => {
  it('renders three tabs', () => {
    renderDetails()
    expect(screen.getByRole('tab', { name: /recent expenses/i })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: /largest expenses/i })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: /upcoming recurring/i })).toBeInTheDocument()
  })

  it('defaults to the Recent tab, showing its panel and hiding the others', () => {
    renderDetails()
    expect(screen.getByRole('tab', { name: /recent expenses/i })).toHaveAttribute('aria-selected', 'true')
    expect(screen.getByText('Recent panel content')).toBeVisible()
    expect(screen.getByText('Largest panel content')).not.toBeVisible()
  })

  it('switches panels when a tab is clicked', async () => {
    const user = userEvent.setup()
    renderDetails()
    await user.click(screen.getByRole('tab', { name: /largest expenses/i }))
    expect(screen.getByRole('tab', { name: /largest expenses/i })).toHaveAttribute('aria-selected', 'true')
    expect(screen.getByText('Largest panel content')).toBeVisible()
    expect(screen.getByText('Recent panel content')).not.toBeVisible()
  })
})
