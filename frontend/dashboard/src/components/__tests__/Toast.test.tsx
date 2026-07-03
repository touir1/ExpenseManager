import { describe, it, expect, vi, afterEach } from 'vitest'
import { render, screen, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ToastProvider, useToast } from '@/components/Toast'

function TestTrigger({ message = 'Test Message', type = 'info' }: { message?: string; type?: 'info' | 'success' | 'error' }) {
  const { show } = useToast()
  return <button onClick={() => show(message, type)}>Show Toast</button>
}

describe('ToastProvider', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('shows a toast with the correct message', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestTrigger message="Hello World" />
      </ToastProvider>
    )

    await user.click(screen.getByRole('button', { name: /show toast/i }))
    expect(screen.getByText('Hello World')).toBeInTheDocument()
  })

  it('shows a success toast with correct styling', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestTrigger message="Success!" type="success" />
      </ToastProvider>
    )

    await user.click(screen.getByRole('button', { name: /show toast/i }))
    const toast = screen.getByText('Success!').closest('div')
    expect(toast).toBeInTheDocument()
    expect(toast).toHaveClass('bg-sage-soft')
  })

  it('shows an error toast with correct styling', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestTrigger message="Error!" type="error" />
      </ToastProvider>
    )

    await user.click(screen.getByRole('button', { name: /show toast/i }))
    const toast = screen.getByText('Error!').closest('div')
    expect(toast).toBeInTheDocument()
    expect(toast).toHaveClass('bg-berry-soft')
  })

  it('shows an info toast with correct styling', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <TestTrigger message="Info!" type="info" />
      </ToastProvider>
    )

    await user.click(screen.getByRole('button', { name: /show toast/i }))
    const toast = screen.getByText('Info!').closest('div')
    expect(toast).toBeInTheDocument()
    expect(toast).toHaveClass('bg-sky-50')
  })

  it('auto-dismisses a toast after 4 seconds', () => {
    vi.useFakeTimers()

    render(
      <ToastProvider>
        <TestTrigger message="Auto Dismiss" />
      </ToastProvider>
    )

    const button = screen.getByRole('button', { name: /show toast/i })

    act(() => {
      button.click()
    })

    expect(screen.getByText('Auto Dismiss')).toBeInTheDocument()

    act(() => {
      vi.advanceTimersByTime(4000)
    })

    expect(screen.queryByText('Auto Dismiss')).not.toBeInTheDocument()
  })

  it('shows multiple toasts at the same time', async () => {
    const user = userEvent.setup()

    function MultipleTriggers() {
      const { show } = useToast()
      return (
        <>
          <button onClick={() => show('Toast One', 'info')}>First</button>
          <button onClick={() => show('Toast Two', 'success')}>Second</button>
        </>
      )
    }

    render(
      <ToastProvider>
        <MultipleTriggers />
      </ToastProvider>
    )

    await user.click(screen.getByRole('button', { name: /first/i }))
    await user.click(screen.getByRole('button', { name: /second/i }))

    expect(screen.getByText('Toast One')).toBeInTheDocument()
    expect(screen.getByText('Toast Two')).toBeInTheDocument()
  })

  it('throws error when useToast is used outside ToastProvider', () => {
    function BadComponent() {
      useToast()
      return null
    }

    expect(() => render(<BadComponent />)).toThrow('useToast must be used within ToastProvider')
  })

  it('defaults to error type when type is not specified', async () => {
    const user = userEvent.setup()

    function DefaultTypeTrigger() {
      const { show } = useToast()
      return <button onClick={() => show('Default Type')}>Show</button>
    }

    render(
      <ToastProvider>
        <DefaultTypeTrigger />
      </ToastProvider>
    )

    await user.click(screen.getByRole('button', { name: /show/i }))
    const toast = screen.getByText('Default Type').closest('div')
    expect(toast).toBeInTheDocument()
    expect(toast).toHaveClass('bg-berry-soft')
  })

  it('dismisses toasts independently', () => {
    vi.useFakeTimers()

    function MultipleTriggers() {
      const { show } = useToast()
      return (
        <>
          <button onClick={() => show('First Toast', 'info')}>First</button>
          <button onClick={() => show('Second Toast', 'success')}>Second</button>
        </>
      )
    }

    render(
      <ToastProvider>
        <MultipleTriggers />
      </ToastProvider>
    )

    act(() => {
      screen.getByRole('button', { name: /first/i }).click()
    })

    act(() => {
      vi.advanceTimersByTime(2000)
    })

    act(() => {
      screen.getByRole('button', { name: /second/i }).click()
    })

    expect(screen.getByText('First Toast')).toBeInTheDocument()
    expect(screen.getByText('Second Toast')).toBeInTheDocument()

    act(() => {
      vi.advanceTimersByTime(2000)
    })

    expect(screen.queryByText('First Toast')).not.toBeInTheDocument()
    expect(screen.getByText('Second Toast')).toBeInTheDocument()

    act(() => {
      vi.advanceTimersByTime(2000)
    })

    expect(screen.queryByText('First Toast')).not.toBeInTheDocument()
    expect(screen.queryByText('Second Toast')).not.toBeInTheDocument()
  })

  it('handles rapid successive toast triggers', async () => {
    const user = userEvent.setup()

    function RapidTrigger() {
      const { show } = useToast()
      return (
        <button onClick={() => {
          show('Toast 1', 'info')
          show('Toast 2', 'success')
          show('Toast 3', 'error')
        }}>
          Trigger Multiple
        </button>
      )
    }

    render(
      <ToastProvider>
        <RapidTrigger />
      </ToastProvider>
    )

    await user.click(screen.getByRole('button', { name: /trigger multiple/i }))

    expect(screen.getByText('Toast 1')).toBeInTheDocument()
    expect(screen.getByText('Toast 2')).toBeInTheDocument()
    expect(screen.getByText('Toast 3')).toBeInTheDocument()
  })

  it('handles empty message', async () => {
    const user = userEvent.setup()

    function EmptyMessageTrigger() {
      const { show } = useToast()
      return <button onClick={() => show('', 'info')}>Empty</button>
    }

    render(
      <ToastProvider>
        <EmptyMessageTrigger />
      </ToastProvider>
    )

    await user.click(screen.getByRole('button', { name: /empty/i }))

    // Toast notifications region is always rendered
    const container = screen.getByRole('region', { name: /notifications/i })
    expect(container).toBeInTheDocument()
  })

  it('collapses rapid successive same-type toasts into one with an incrementing count', () => {
    vi.useFakeTimers()

    function RepeatTrigger() {
      const { show } = useToast()
      let n = 0
      return <button onClick={() => show(`Notification ${++n}`, 'info')}>Notify</button>
    }

    render(
      <ToastProvider>
        <RepeatTrigger />
      </ToastProvider>
    )

    const button = screen.getByRole('button', { name: /notify/i })
    act(() => { button.click() })
    act(() => { vi.advanceTimersByTime(500); button.click() })
    act(() => { vi.advanceTimersByTime(500); button.click() })

    expect(screen.getAllByText(/Notification \d/).length).toBe(1)
    expect(screen.getByText('3')).toBeInTheDocument()
  })

  it('keeps different-type toasts stacked separately even when triggered rapidly', () => {
    vi.useFakeTimers()

    function MixedTrigger() {
      const { show } = useToast()
      return (
        <>
          <button onClick={() => show('Info message', 'info')}>Info</button>
          <button onClick={() => show('Error message', 'error')}>Error</button>
        </>
      )
    }

    render(
      <ToastProvider>
        <MixedTrigger />
      </ToastProvider>
    )

    act(() => { screen.getByRole('button', { name: /^info$/i }).click() })
    act(() => { screen.getByRole('button', { name: /^error$/i }).click() })

    expect(screen.getByText('Info message')).toBeInTheDocument()
    expect(screen.getByText('Error message')).toBeInTheDocument()
  })

  it('resets the auto-dismiss timer each time a toast collapses', () => {
    vi.useFakeTimers()

    function RepeatTrigger() {
      const { show } = useToast()
      return <button onClick={() => show('Collapsing', 'info')}>Notify</button>
    }

    render(
      <ToastProvider>
        <RepeatTrigger />
      </ToastProvider>
    )

    const button = screen.getByRole('button', { name: /notify/i })
    act(() => { button.click() })
    act(() => { vi.advanceTimersByTime(3000) })
    act(() => { button.click() }) // collapses, resets timer
    act(() => { vi.advanceTimersByTime(3000) })

    // 6s since first show, but only 3s since last collapse — still visible
    expect(screen.getByText('Collapsing')).toBeInTheDocument()

    act(() => { vi.advanceTimersByTime(1000) })

    expect(screen.queryByText('Collapsing')).not.toBeInTheDocument()
  })

  it('renders children correctly', () => {
    render(
      <ToastProvider>
        <div data-testid="child">Child Content</div>
      </ToastProvider>
    )

    expect(screen.getByTestId('child')).toBeInTheDocument()
    expect(screen.getByText('Child Content')).toBeInTheDocument()
  })
})
