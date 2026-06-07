import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import FieldError from '../FieldError'

describe('FieldError', () => {
  it('renders nothing when message is undefined', () => {
    const { container } = render(<FieldError id="x-error" />)
    expect(container).toBeEmptyDOMElement()
  })

  it('renders nothing when message is empty string', () => {
    const { container } = render(<FieldError id="x-error" message="" />)
    expect(container).toBeEmptyDOMElement()
  })

  it('renders the error message', () => {
    render(<FieldError id="x-error" message="Field is required" />)
    expect(screen.getByText('Field is required')).toBeInTheDocument()
  })

  it('has role alert', () => {
    render(<FieldError id="x-error" message="Error" />)
    expect(screen.getByRole('alert')).toBeInTheDocument()
  })

  it('has the provided id', () => {
    render(<FieldError id="email-error" message="Invalid email" />)
    expect(screen.getByRole('alert')).toHaveAttribute('id', 'email-error')
  })
})
