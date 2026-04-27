import { describe, it, expect } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import SettingsPage from '@/features/dashboard/pages/SettingsPage'

function renderSettings() {
  return render(
    <MemoryRouter>
      <SettingsPage />
    </MemoryRouter>
  )
}

describe('Settings page', () => {
  it('renders Settings heading', () => {
    renderSettings()
    expect(screen.getByRole('heading', { name: /settings/i })).toBeInTheDocument()
  })

  it('renders the Password section heading', () => {
    renderSettings()
    expect(screen.getByRole('heading', { name: /password/i })).toBeInTheDocument()
  })

  it('renders a Change Password link pointing to /change-password', () => {
    renderSettings()
    const link = screen.getByRole('link', { name: /change password/i })
    expect(link).toBeInTheDocument()
    expect(link).toHaveAttribute('href', '/change-password')
  })
})
