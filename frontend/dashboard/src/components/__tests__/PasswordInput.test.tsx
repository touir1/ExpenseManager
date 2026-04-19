import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import PasswordInput from '@/components/PasswordInput'

function renderInput(props: Record<string, unknown> = {}) {
  return render(<PasswordInput id="pwd" {...props} />)
}

describe('PasswordInput', () => {
  it('renders input with type="password" by default', () => {
    renderInput()
    const input = document.getElementById('pwd') as HTMLInputElement
    expect(input).toHaveAttribute('type', 'password')
  })

  it('show button has aria-label "Show password" initially', () => {
    renderInput()
    expect(screen.getByRole('button', { name: /show password/i })).toBeInTheDocument()
  })

  it('clicking show button changes input type to "text"', async () => {
    const user = userEvent.setup()
    renderInput()

    await user.click(screen.getByRole('button', { name: /show password/i }))

    const input = document.getElementById('pwd') as HTMLInputElement
    expect(input).toHaveAttribute('type', 'text')
  })

  it('clicking show button changes aria-label to "Hide password"', async () => {
    const user = userEvent.setup()
    renderInput()

    await user.click(screen.getByRole('button', { name: /show password/i }))

    expect(screen.getByRole('button', { name: /hide password/i })).toBeInTheDocument()
  })

  it('clicking hide button restores input type to "password"', async () => {
    const user = userEvent.setup()
    renderInput()

    await user.click(screen.getByRole('button', { name: /show password/i }))
    await user.click(screen.getByRole('button', { name: /hide password/i }))

    const input = document.getElementById('pwd') as HTMLInputElement
    expect(input).toHaveAttribute('type', 'password')
  })

  it('passes className to the input element', () => {
    renderInput({ className: 'field-input' })
    const input = document.getElementById('pwd') as HTMLInputElement
    expect(input).toHaveClass('field-input')
  })

  it('passes additional props (disabled) to the input', () => {
    renderInput({ disabled: true })
    const input = document.getElementById('pwd') as HTMLInputElement
    expect(input).toBeDisabled()
  })

  it('passes value and onChange to the input', async () => {
    const user = userEvent.setup()
    const { rerender } = render(<PasswordInput id="pwd" value="" onChange={() => {}} />)
    rerender(<PasswordInput id="pwd" value="hello" onChange={() => {}} />)

    const input = document.getElementById('pwd') as HTMLInputElement
    expect(input).toHaveValue('hello')
  })
})
