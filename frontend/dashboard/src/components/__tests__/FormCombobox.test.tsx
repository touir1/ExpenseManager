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

  describe('accessibility', () => {
    it('exposes combobox role and aria attributes', () => {
      render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} />)
      const input = screen.getByRole('combobox')
      expect(input).toHaveAttribute('aria-expanded', 'false')
      expect(input).toHaveAttribute('aria-autocomplete', 'list')
      fireEvent.focus(input)
      expect(input).toHaveAttribute('aria-expanded', 'true')
      expect(input).toHaveAttribute('aria-controls', screen.getByRole('listbox').id)
    })
  })

  describe('keyboard navigation', () => {
    it('ArrowDown opens the dropdown and highlights the first option', () => {
      render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} />)
      const input = screen.getByRole('combobox')
      fireEvent.keyDown(input, { key: 'ArrowDown' })
      expect(screen.getByRole('listbox')).toBeInTheDocument()
    })

    it('ArrowDown/ArrowUp move aria-activedescendant through options in order', () => {
      render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} />)
      const input = screen.getByRole('combobox')
      fireEvent.focus(input)
      fireEvent.keyDown(input, { key: 'ArrowDown' }) // clear -> USD
      const usdOption = screen.getByRole('option', { name: 'USD' })
      expect(input).toHaveAttribute('aria-activedescendant', usdOption.id)
      fireEvent.keyDown(input, { key: 'ArrowDown' }) // USD -> EUR
      const eurOption = screen.getByRole('option', { name: 'EUR' })
      expect(input).toHaveAttribute('aria-activedescendant', eurOption.id)
      fireEvent.keyDown(input, { key: 'ArrowUp' }) // EUR -> USD
      expect(input).toHaveAttribute('aria-activedescendant', usdOption.id)
    })

    it('Enter selects the highlighted option and closes the dropdown', () => {
      const onChange = vi.fn()
      render(<FormCombobox value={undefined} onChange={onChange} options={options} />)
      const input = screen.getByRole('combobox')
      fireEvent.focus(input)
      fireEvent.keyDown(input, { key: 'ArrowDown' }) // -> USD
      fireEvent.keyDown(input, { key: 'Enter' })
      expect(onChange).toHaveBeenCalledWith(1)
      expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    })

    it('Escape closes the dropdown without changing the value', () => {
      const onChange = vi.fn()
      render(<FormCombobox value={2} onChange={onChange} options={options} />)
      const input = screen.getByRole('combobox')
      fireEvent.focus(input)
      expect(screen.getByRole('listbox')).toBeInTheDocument()
      fireEvent.keyDown(input, { key: 'Escape' })
      expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
      expect(onChange).not.toHaveBeenCalled()
      expect(screen.getByDisplayValue('EUR')).toBeInTheDocument()
    })

    it('Home/End jump to the first/last option', () => {
      render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} />)
      const input = screen.getByRole('combobox')
      fireEvent.focus(input)
      fireEvent.keyDown(input, { key: 'End' })
      const gbpOption = screen.getByRole('option', { name: 'GBP' })
      expect(input).toHaveAttribute('aria-activedescendant', gbpOption.id)
      fireEvent.keyDown(input, { key: 'Home' })
      const clearOption = screen.getAllByRole('option').find(el => el.textContent === '—')!
      expect(input).toHaveAttribute('aria-activedescendant', clearOption.id)
    })

    it('type-ahead jumps to the option starting with the typed character', () => {
      render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} />)
      const input = screen.getByRole('combobox')
      fireEvent.focus(input)
      fireEvent.keyDown(input, { key: 'g' })
      const gbpOption = screen.getByRole('option', { name: 'GBP' })
      expect(input).toHaveAttribute('aria-activedescendant', gbpOption.id)
    })
  })

  describe('large option lists (virtualization cap)', () => {
    const manyOptions = Array.from({ length: 120 }, (_, i) => ({
      value: i + 1,
      label: `Currency ${String(i + 1).padStart(3, '0')}`,
    }))

    it('renders at most 50 options plus a clear option, with an overflow hint', () => {
      render(<FormCombobox value={undefined} onChange={vi.fn()} options={manyOptions} />)
      fireEvent.focus(screen.getByRole('combobox'))
      const listOptions = screen.getAllByRole('option')
      expect(listOptions.length).toBe(50) // capped window, includes the clear option
      expect(screen.getByText(/71 more — keep typing to narrow/i)).toBeInTheDocument()
    })

    it('does not show an overflow hint for 50 or fewer options', () => {
      render(<FormCombobox value={undefined} onChange={vi.fn()} options={options} />)
      fireEvent.focus(screen.getByRole('combobox'))
      expect(screen.queryByText(/more — keep typing to narrow/i)).not.toBeInTheDocument()
    })

    it('keyboard navigation can reach and select an option beyond the initial 50-item window', () => {
      const onChange = vi.fn()
      render(<FormCombobox value={undefined} onChange={onChange} options={manyOptions} />)
      const input = screen.getByRole('combobox')
      fireEvent.focus(input)
      // clear-option + first 100 items -> lands on "Currency 100" (option index 100)
      for (let i = 0; i < 100; i++) fireEvent.keyDown(input, { key: 'ArrowDown' })
      const target = screen.getByRole('option', { name: 'Currency 100' })
      expect(input).toHaveAttribute('aria-activedescendant', target.id)
      fireEvent.keyDown(input, { key: 'Enter' })
      expect(onChange).toHaveBeenCalledWith(100)
    })
  })
})
