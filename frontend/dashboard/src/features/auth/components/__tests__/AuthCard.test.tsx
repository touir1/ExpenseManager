import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import AuthCard from '../AuthCard'

describe('AuthCard', () => {
  it('renders children', () => {
    render(<AuthCard><span>content</span></AuthCard>)
    expect(screen.getByText('content')).toBeInTheDocument()
  })

  it('renders multiple children', () => {
    render(
      <AuthCard>
        <p>first</p>
        <p>second</p>
      </AuthCard>
    )
    expect(screen.getByText('first')).toBeInTheDocument()
    expect(screen.getByText('second')).toBeInTheDocument()
  })
})
