import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { FormCombobox } from '../FormCombobox'

const options = [
  { value: 1, label: 'USD' },
  { value: 2, label: 'EUR' },
  { value: 3, label: 'GBP' },
]

describe('FormCombobox', () => {
  it('renders placeholder — when no value', () => {
    render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} />)
    expect(screen.getByPlaceholderText('—')).toBeInTheDocument()
    expect(screen.getByDisplayValue('')).toBeInTheDocument()
  })

  it('shows selected label when value matches option', () => {
    render(<FormCombobox value={2} onChange={vi.fn()} options={options} />)
    expect(screen.getByDisplayValue('EUR')).toBeInTheDocument()
  })

  it('opens dropdown on focus', () => {
    render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} />)
    const input = screen.getByPlaceholderText('—')
    fireEvent.focus(input)
    expect(screen.getByRole('listbox')).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'USD' })).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'EUR' })).toBeInTheDocument()
  })

  it('closes dropdown on outside click', () => {
    render(
      <div>
        <FormCombobox value={undefined} onChange={vi.fn()} options={options} />
        <button>outside</button>
      </div>
    )
    const input = screen.getByPlaceholderText('—')
    fireEvent.focus(input)
    expect(screen.getByRole('listbox')).toBeInTheDocument()
    fireEvent.mouseDown(screen.getByText('outside'))
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('filters options by search query (case-insensitive)', () => {
    render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} />)
    const input = screen.getByPlaceholderText('—')
    fireEvent.focus(input)
    fireEvent.change(input, { target: { value: 'eur' } })
    expect(screen.getByRole('option', { name: 'EUR' })).toBeInTheDocument()
    expect(screen.queryByRole('option', { name: 'USD' })).not.toBeInTheDocument()
    expect(screen.queryByRole('option', { name: 'GBP' })).not.toBeInTheDocument()
  })

  it('calls onChange(value) on option mousedown', () => {
    const onChange = vi.fn()
    render(<FormCombobox value={undefined} onChange={onChange} options={options} />)
    const input = screen.getByPlaceholderText('—')
    fireEvent.focus(input)
    fireEvent.mouseDown(screen.getByRole('option', { name: 'USD' }))
    expect(onChange).toHaveBeenCalledWith(1)
  })

  it('calls onChange(undefined) on — option mousedown', () => {
    const onChange = vi.fn()
    render(<FormCombobox value={1} onChange={onChange} options={options} />)
    const input = screen.getByDisplayValue('USD')
    fireEvent.focus(input)
    const clearOption = screen.getAllByRole('option').find(el => el.textContent === '—')!
    fireEvent.mouseDown(clearOption)
    expect(onChange).toHaveBeenCalledWith(undefined)
  })

  it('disabled: input has disabled attribute and dropdown does not open', () => {
    render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} disabled />)
    const input = screen.getByPlaceholderText('—')
    expect(input).toBeDisabled()
    fireEvent.focus(input)
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })
})
