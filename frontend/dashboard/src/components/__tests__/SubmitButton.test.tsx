import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import SubmitButton from '../SubmitButton'

describe('SubmitButton', () => {
  it('renders label when not submitting', () => {
    render(<SubmitButton isSubmitting={false} label="Save" />)
    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument()
  })

  it('renders loadingLabel when submitting', () => {
    render(<SubmitButton isSubmitting={true} label="Save" loadingLabel="Saving…" />)
    expect(screen.getByText('Saving…')).toBeInTheDocument()
    expect(screen.queryByText('Save')).not.toBeInTheDocument()
  })

  it('renders default loadingLabel when not provided', () => {
    render(<SubmitButton isSubmitting={true} label="Save" />)
    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('is disabled when isSubmitting', () => {
    render(<SubmitButton isSubmitting={true} label="Save" />)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('is enabled when not submitting', () => {
    render(<SubmitButton isSubmitting={false} label="Save" />)
    expect(screen.getByRole('button')).not.toBeDisabled()
  })

  it('is disabled when disabled prop is true', () => {
    render(<SubmitButton isSubmitting={false} label="Save" disabled={true} />)
    expect(screen.getByRole('button')).toBeDisabled()
  })

  it('is type submit', () => {
    render(<SubmitButton isSubmitting={false} label="Save" />)
    expect(screen.getByRole('button')).toHaveAttribute('type', 'submit')
  })

  it('shows spinner svg when submitting', () => {
    const { container } = render(<SubmitButton isSubmitting={true} label="Save" />)
    expect(container.querySelector('svg')).toBeInTheDocument()
  })
})
