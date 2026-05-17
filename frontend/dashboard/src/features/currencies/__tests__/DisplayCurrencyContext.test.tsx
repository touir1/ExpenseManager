import { describe, it, expect } from 'vitest'
import { render, screen, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { DisplayCurrencyProvider, useDisplayCurrency } from '../DisplayCurrencyContext'

function Consumer() {
  const { displayCurrencyId, setDisplayCurrencyId } = useDisplayCurrency()
  return (
    <>
      <span data-testid="value">{displayCurrencyId ?? 'null'}</span>
      <button onClick={() => setDisplayCurrencyId(3)}>set-3</button>
      <button onClick={() => setDisplayCurrencyId(null)}>clear</button>
    </>
  )
}

function renderWithProvider() {
  return render(
    <DisplayCurrencyProvider>
      <Consumer />
    </DisplayCurrencyProvider>
  )
}

describe('DisplayCurrencyContext', () => {
  it('default value is null', () => {
    renderWithProvider()
    expect(screen.getByTestId('value').textContent).toBe('null')
  })

  it('setDisplayCurrencyId updates value', async () => {
    renderWithProvider()
    await userEvent.click(screen.getByText('set-3'))
    expect(screen.getByTestId('value').textContent).toBe('3')
  })

  it('setDisplayCurrencyId(null) clears value', async () => {
    renderWithProvider()
    await userEvent.click(screen.getByText('set-3'))
    await userEvent.click(screen.getByText('clear'))
    expect(screen.getByTestId('value').textContent).toBe('null')
  })

  it('persists within same render (re-render keeps value)', async () => {
    const { rerender } = renderWithProvider()
    await userEvent.click(screen.getByText('set-3'))
    rerender(
      <DisplayCurrencyProvider>
        <Consumer />
      </DisplayCurrencyProvider>
    )
    expect(screen.getByTestId('value').textContent).toBe('3')
  })

  it('throws when used outside provider', () => {
    const consoleError = console.error
    console.error = () => {}
    expect(() => render(<Consumer />)).toThrow('useDisplayCurrency must be used within DisplayCurrencyProvider')
    console.error = consoleError
  })
})
