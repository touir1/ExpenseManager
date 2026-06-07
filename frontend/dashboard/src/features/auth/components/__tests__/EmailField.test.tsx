import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import EmailField from '../EmailField'

const noop = () => {}
const reg = { name: 'email', ref: noop, onChange: noop, onBlur: noop }

describe('EmailField', () => {
  it('renders email label', () => {
    render(<EmailField registration={reg} isSubmitting={false} />)
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
  })

  it('renders email input', () => {
    render(<EmailField registration={reg} isSubmitting={false} />)
    expect(screen.getByRole('textbox')).toHaveAttribute('type', 'email')
  })

  it('disables input when isSubmitting', () => {
    render(<EmailField registration={reg} isSubmitting={true} />)
    expect(screen.getByRole('textbox')).toBeDisabled()
  })

  it('shows error message when error prop provided', () => {
    render(<EmailField registration={reg} isSubmitting={false} error="Email required" />)
    expect(screen.getByRole('alert')).toHaveTextContent('Email required')
  })

  it('does not show error when error prop absent', () => {
    render(<EmailField registration={reg} isSubmitting={false} />)
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('sets aria-invalid when error present', () => {
    render(<EmailField registration={reg} isSubmitting={false} error="bad" />)
    expect(screen.getByRole('textbox')).toHaveAttribute('aria-invalid', 'true')
  })

  it('sets aria-describedby when error present', () => {
    render(<EmailField registration={reg} isSubmitting={false} error="bad" />)
    expect(screen.getByRole('textbox')).toHaveAttribute('aria-describedby', 'email-error')
  })

  it('does not set aria-describedby when no error', () => {
    render(<EmailField registration={reg} isSubmitting={false} />)
    expect(screen.getByRole('textbox')).not.toHaveAttribute('aria-describedby')
  })
})
