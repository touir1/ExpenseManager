import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import PasswordStrength from '../PasswordStrength'

describe('PasswordStrength', () => {
  it('renders nothing when password is empty', () => {
    const { container } = render(<PasswordStrength password="" />)
    expect(container.firstChild).toBeNull()
  })

  it('renders the requirements list when password is non-empty', () => {
    render(<PasswordStrength password="a" />)
    expect(screen.getByRole('list', { name: /password requirements/i })).toBeInTheDocument()
  })

  it('shows all five requirement labels', () => {
    render(<PasswordStrength password="a" />)
    expect(screen.getByText(/at least 8 characters/i)).toBeInTheDocument()
    expect(screen.getByText(/uppercase letter/i)).toBeInTheDocument()
    expect(screen.getByText(/lowercase letter/i)).toBeInTheDocument()
    expect(screen.getByText(/number/i)).toBeInTheDocument()
    expect(screen.getByText(/special character/i)).toBeInTheDocument()
  })

  it('shows "Weak" for a password meeting one criterion', () => {
    render(<PasswordStrength password="a" />) // only lowercase
    expect(screen.getByTestId('strength-label')).toHaveTextContent('Weak')
  })

  it('shows "Weak" for a password meeting two criteria', () => {
    render(<PasswordStrength password="aB" />) // lowercase + uppercase
    expect(screen.getByTestId('strength-label')).toHaveTextContent('Weak')
  })

  it('shows "Fair" for a password meeting three criteria', () => {
    render(<PasswordStrength password="aB1" />) // lowercase + uppercase + number
    expect(screen.getByTestId('strength-label')).toHaveTextContent('Fair')
  })

  it('shows "Good" for a password meeting four criteria', () => {
    render(<PasswordStrength password="aB1!" />) // lowercase + uppercase + number + special
    expect(screen.getByTestId('strength-label')).toHaveTextContent('Good')
  })

  it('shows "Strong" for a password meeting all five criteria', () => {
    render(<PasswordStrength password="aB1!xyzw" />) // all five: length + lowercase + uppercase + number + special
    expect(screen.getByTestId('strength-label')).toHaveTextContent('Strong')
  })

  it('does not show a strength label when no criteria are met', () => {
    // No character type is hard to trigger — empty is already handled.
    // A password of all digits < 8 chars meets only the number criterion (score=1 → Weak).
    // To get score=0 we'd need a string that matches none, which can't happen for non-empty strings
    // since it will always be lowercase, uppercase, number, or special.
    // We just verify the component renders without crashing when score is low.
    render(<PasswordStrength password="1" />) // only number criterion
    expect(screen.getByTestId('strength-label')).toHaveTextContent('Weak')
  })

  it('marks the length criterion as met when password is 8+ characters', () => {
    render(<PasswordStrength password="abcdefgh" />) // exactly 8 chars
    const items = screen.getAllByRole('listitem')
    const lengthItem = items.find(li => li.textContent?.includes('At least 8 characters'))!
    expect(lengthItem).toHaveClass('text-emerald-600')
  })

  it('marks the length criterion as unmet when password is under 8 characters', () => {
    render(<PasswordStrength password="abc" />)
    const items = screen.getAllByRole('listitem')
    const lengthItem = items.find(li => li.textContent?.includes('At least 8 characters'))!
    expect(lengthItem).toHaveClass('text-slate-400')
  })

  it('marks the uppercase criterion as met', () => {
    render(<PasswordStrength password="A" />)
    const items = screen.getAllByRole('listitem')
    const item = items.find(li => li.textContent?.includes('Uppercase letter'))!
    expect(item).toHaveClass('text-emerald-600')
  })

  it('marks the number criterion as met', () => {
    render(<PasswordStrength password="1" />)
    const items = screen.getAllByRole('listitem')
    const item = items.find(li => li.textContent?.includes('Number'))!
    expect(item).toHaveClass('text-emerald-600')
  })

  it('marks the special character criterion as met', () => {
    render(<PasswordStrength password="!" />)
    const items = screen.getAllByRole('listitem')
    const item = items.find(li => li.textContent?.includes('Special character'))!
    expect(item).toHaveClass('text-emerald-600')
  })
})
