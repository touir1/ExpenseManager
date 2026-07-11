import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
import { useReturnFocusOnUnmount } from '../useReturnFocusOnUnmount'

function Dialog({ onClose }: Readonly<{ onClose: () => void }>) {
  useReturnFocusOnUnmount()
  return <button onClick={onClose}>Close dialog</button>
}

function Harness() {
  const [open, setOpen] = useState(false)
  return (
    <div>
      <button onClick={() => setOpen(true)}>Open dialog</button>
      {open && <Dialog onClose={() => setOpen(false)} />}
    </div>
  )
}

describe('useReturnFocusOnUnmount', () => {
  it('returns focus to the trigger element after the component unmounts', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    const opener = screen.getByRole('button', { name: 'Open dialog' })
    await user.click(opener)
    expect(opener).toHaveFocus()

    const closeButton = screen.getByRole('button', { name: 'Close dialog' })
    await user.click(closeButton)

    expect(screen.queryByRole('button', { name: 'Close dialog' })).not.toBeInTheDocument()
    expect(opener).toHaveFocus()
  })
})
