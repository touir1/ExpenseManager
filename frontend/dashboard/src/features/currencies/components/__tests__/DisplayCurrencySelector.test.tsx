import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import DisplayCurrencySelector from '../DisplayCurrencySelector'

const mockUseExpensesData = vi.fn()
const mockSetDisplayCurrencyId = vi.fn()
const mockUseDisplayCurrency = vi.fn()

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => mockUseExpensesData(),
}))

vi.mock('@/features/currencies/DisplayCurrencyContext', () => ({
  useDisplayCurrency: () => mockUseDisplayCurrency(),
}))

const currencies = [
  { id: 1, code: 'USD', name: 'US Dollar', symbol: '$', decimals: 2 },
  { id: 2, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 },
]

function renderSelector() {
  return render(<DisplayCurrencySelector />)
}

describe('DisplayCurrencySelector', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseExpensesData.mockReturnValue({ currencies, isLoading: false, refresh: vi.fn() })
    mockUseDisplayCurrency.mockReturnValue({ displayCurrencyId: null, setDisplayCurrencyId: mockSetDisplayCurrencyId })
  })

  it('renders nothing when no currencies', () => {
    mockUseExpensesData.mockReturnValue({ currencies: [], isLoading: false, refresh: vi.fn() })
    const { container } = renderSelector()
    expect(container).toBeEmptyDOMElement()
  })

  it('renders trigger button with first currency label when displayCurrencyId is null', () => {
    renderSelector()
    expect(screen.getByRole('button', { name: /display currency/i })).toBeInTheDocument()
    expect(screen.getByText('USD $')).toBeInTheDocument()
  })

  it('renders trigger button with currency label when one is selected', () => {
    mockUseDisplayCurrency.mockReturnValue({ displayCurrencyId: 1, setDisplayCurrencyId: mockSetDisplayCurrencyId })
    renderSelector()
    expect(screen.getByText('USD $')).toBeInTheDocument()
  })

  it('dropdown is hidden initially', () => {
    renderSelector()
    expect(screen.queryByRole('menu')).not.toBeInTheDocument()
  })

  it('opens dropdown on click and shows all currencies', async () => {
    renderSelector()
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    expect(screen.getByRole('menu')).toBeInTheDocument()
    expect(screen.getByRole('menuitemradio', { name: /USD/i })).toBeInTheDocument()
    expect(screen.getByRole('menuitemradio', { name: /EUR/i })).toBeInTheDocument()
  })

  it('selecting a currency calls setDisplayCurrencyId with its id', async () => {
    renderSelector()
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    await userEvent.click(screen.getByRole('menuitemradio', { name: /EUR/i }))
    expect(mockSetDisplayCurrencyId).toHaveBeenCalledWith(2)
  })

  it('closes dropdown after selecting a currency', async () => {
    renderSelector()
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    await userEvent.click(screen.getByRole('menuitemradio', { name: /USD/i }))
    expect(screen.queryByRole('menu')).not.toBeInTheDocument()
  })

  it('active currency item has aria-checked=true', async () => {
    mockUseDisplayCurrency.mockReturnValue({ displayCurrencyId: 1, setDisplayCurrencyId: mockSetDisplayCurrencyId })
    renderSelector()
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    const usdItem = screen.getByRole('menuitemradio', { name: /USD/i })
    expect(usdItem).toHaveAttribute('aria-checked', 'true')
    const eurItem = screen.getByRole('menuitemradio', { name: /EUR/i })
    expect(eurItem).toHaveAttribute('aria-checked', 'false')
  })

  it('renders search input when dropdown is open', async () => {
    renderSelector()
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    expect(screen.getByRole('textbox')).toBeInTheDocument()
  })

  it('filters currencies by code when searching', async () => {
    renderSelector()
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    await userEvent.type(screen.getByRole('textbox'), 'USD')
    expect(screen.getByRole('menuitemradio', { name: /USD/i })).toBeInTheDocument()
    expect(screen.queryByRole('menuitemradio', { name: /EUR/i })).not.toBeInTheDocument()
  })

  it('filters currencies by name when searching', async () => {
    renderSelector()
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    await userEvent.type(screen.getByRole('textbox'), 'euro')
    expect(screen.getByRole('menuitemradio', { name: /EUR/i })).toBeInTheDocument()
    expect(screen.queryByRole('menuitemradio', { name: /USD/i })).not.toBeInTheDocument()
  })

  it('clears search when dropdown closes', async () => {
    renderSelector()
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    await userEvent.type(screen.getByRole('textbox'), 'USD')
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    const input = screen.getByRole('textbox')
    expect(input).toHaveValue('')
  })

  it('closes dropdown when clicking outside the component', async () => {
    renderSelector()
    await userEvent.click(screen.getByRole('button', { name: /display currency/i }))
    expect(screen.getByRole('menu')).toBeInTheDocument()
    fireEvent.mouseDown(document.body)
    expect(screen.queryByRole('menu')).not.toBeInTheDocument()
  })
})
