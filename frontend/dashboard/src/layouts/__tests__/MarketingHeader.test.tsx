import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import MarketingHeader from '@/layouts/MarketingHeader'

function renderHeader(path = '/') {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <MarketingHeader />
    </MemoryRouter>
  )
}

describe('MarketingHeader', () => {
  it('shows Sign in and Get started links', () => {
    renderHeader('/')
    const nav = screen.getByRole('navigation')
    expect(nav).toHaveTextContent('Sign in')
    expect(nav).toHaveTextContent('Get started')
  })

  it('links logo to /', () => {
    renderHeader('/')
    const logo = screen.getByRole('link', { name: /expensemanager/i })
    expect(logo).toHaveAttribute('href', '/')
  })

  it('uses the serif wordmark font for brand distinction', () => {
    renderHeader('/')
    const logoLink = screen.getByRole('link', { name: /ExpenseManager\.$/i })
    const wordmark = Array.from(logoLink.querySelectorAll('span')).find(el => el.textContent?.includes('ExpenseManager'))
    expect(wordmark).toHaveClass('font-serif')
  })

  it('applies active class to Sign in when on /login', () => {
    renderHeader('/login')
    const nav = screen.getByRole('navigation')
    const signInLink = Array.from(nav.querySelectorAll('a')).find(a => a.textContent === 'Sign in')
    expect(signInLink).toHaveClass('bg-brand-100', 'text-brand-600')
  })

  describe('mobile menu', () => {
    it('toggles open when hamburger button is clicked', async () => {
      const user = userEvent.setup()
      renderHeader('/')

      const hamburger = screen.getByRole('button', { name: /toggle menu/i })
      expect(screen.getAllByText('Sign in')).toHaveLength(1)

      await user.click(hamburger)
      expect(screen.getAllByText('Sign in')).toHaveLength(2)
    })

    it('closes when Escape is pressed', async () => {
      const user = userEvent.setup()
      renderHeader('/')
      await user.click(screen.getByRole('button', { name: /toggle menu/i }))
      expect(screen.getAllByText('Sign in')).toHaveLength(2)
      await user.keyboard('{Escape}')
      expect(screen.getAllByText('Sign in')).toHaveLength(1)
    })

    it('restores focus to hamburger button after menu closes', async () => {
      const user = userEvent.setup()
      renderHeader('/')
      const hamburger = screen.getByRole('button', { name: /toggle menu/i })
      await user.click(hamburger)
      await user.keyboard('{Escape}')
      expect(document.activeElement).toBe(hamburger)
    })

    it('mobile menu panel has role=navigation and aria-label', async () => {
      const user = userEvent.setup()
      renderHeader('/')
      await user.click(screen.getByRole('button', { name: /toggle menu/i }))
      expect(screen.getByRole('navigation', { name: /mobile navigation/i })).toBeInTheDocument()
    })

    it('closes mobile menu when a link is clicked', async () => {
      const user = userEvent.setup()
      renderHeader('/')
      await user.click(screen.getByRole('button', { name: /toggle menu/i }))
      expect(screen.getAllByText('Sign in')).toHaveLength(2)
      const [, mobileSignIn] = screen.getAllByText('Sign in')
      await user.click(mobileSignIn)
      expect(screen.getAllByText('Sign in')).toHaveLength(1)
    })
  })
})
