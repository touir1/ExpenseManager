import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import UserMenu from '../UserMenu'

describe('UserMenu', () => {
  it('shows the language switcher', () => {
    render(<UserMenu onLogout={vi.fn()} />)
    expect(screen.getByText('Language')).toBeInTheDocument()
  })

  it('calls onLogout when Sign out is clicked', async () => {
    const onLogout = vi.fn()
    const user = userEvent.setup()
    render(<UserMenu onLogout={onLogout} />)
    await user.click(screen.getByRole('button', { name: /sign out/i }))
    expect(onLogout).toHaveBeenCalledOnce()
  })
})
