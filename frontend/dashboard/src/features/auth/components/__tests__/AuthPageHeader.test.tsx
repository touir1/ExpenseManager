import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import AuthPageHeader from '../AuthPageHeader'

describe('AuthPageHeader', () => {
  it('renders title', () => {
    render(<AuthPageHeader title="Sign in" subtitle="Welcome back" />)
    expect(screen.getByRole('heading', { level: 1, name: 'Sign in' })).toBeInTheDocument()
  })

  it('renders subtitle', () => {
    render(<AuthPageHeader title="Sign in" subtitle="Welcome back" />)
    expect(screen.getByText('Welcome back')).toBeInTheDocument()
  })

  it('renders different title and subtitle', () => {
    render(<AuthPageHeader title="Create account" subtitle="Get started today" />)
    expect(screen.getByRole('heading', { name: 'Create account' })).toBeInTheDocument()
    expect(screen.getByText('Get started today')).toBeInTheDocument()
  })
})
