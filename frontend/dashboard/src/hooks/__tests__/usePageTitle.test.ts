import { describe, it, expect, afterEach } from 'vitest'
import { renderHook } from '@testing-library/react'
import { usePageTitle } from '../usePageTitle'

const SUFFIX = 'Expenses Manager'

afterEach(() => {
  document.title = ''
})

describe('usePageTitle', () => {
  it('sets document.title with provided title and suffix', () => {
    renderHook(() => usePageTitle('Dashboard'))
    expect(document.title).toBe(`Dashboard — ${SUFFIX}`)
  })

  it('sets document.title to suffix only when no title provided', () => {
    renderHook(() => usePageTitle())
    expect(document.title).toBe(SUFFIX)
  })

  it('updates document.title when title changes', () => {
    const { rerender } = renderHook(({ title }) => usePageTitle(title), {
      initialProps: { title: 'Page A' as string | undefined },
    })
    expect(document.title).toBe(`Page A — ${SUFFIX}`)
    rerender({ title: 'Page B' })
    expect(document.title).toBe(`Page B — ${SUFFIX}`)
  })

  it('resets to suffix when title changes to undefined', () => {
    const { rerender } = renderHook(({ title }) => usePageTitle(title), {
      initialProps: { title: 'Expenses' as string | undefined },
    })
    rerender({ title: undefined })
    expect(document.title).toBe(SUFFIX)
  })
})
