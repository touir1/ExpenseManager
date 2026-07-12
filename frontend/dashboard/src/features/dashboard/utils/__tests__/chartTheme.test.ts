import { describe, it, expect, afterEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { getChartColors, useChartColors } from '../chartTheme'

function setVars(vars: Record<string, string>) {
  for (const [name, value] of Object.entries(vars)) {
    document.documentElement.style.setProperty(name, value)
  }
}

afterEach(() => {
  document.documentElement.removeAttribute('style')
  document.documentElement.classList.remove('dark', 'light')
})

describe('getChartColors', () => {
  it('reads --chart-* CSS custom properties from the document root', () => {
    setVars({
      '--chart-grid': '#E8DECB',
      '--chart-tick': '#8E8170',
      '--chart-tooltip-bg': '#FFFCF6',
      '--chart-tooltip-border': '#E8DECB',
    })

    expect(getChartColors()).toEqual({
      grid: '#E8DECB',
      tick: '#8E8170',
      tooltipBg: '#FFFCF6',
      tooltipBorder: '#E8DECB',
    })
  })
})

describe('useChartColors', () => {
  it('recomputes colors when the root class attribute changes', async () => {
    setVars({ '--chart-grid': '#light-grid' })

    const { result } = renderHook(() => useChartColors())
    expect(result.current.grid).toBe('#light-grid')

    setVars({ '--chart-grid': '#dark-grid' })
    await act(async () => {
      document.documentElement.classList.add('dark')
      await Promise.resolve()
    })

    expect(result.current.grid).toBe('#dark-grid')
  })
})
