import { useEffect, useState } from 'react'

export function getCSSVar(name: string): string {
  return getComputedStyle(document.documentElement).getPropertyValue(name).trim()
}

export type ChartColors = {
  grid: string
  tick: string
  tooltipBg: string
  tooltipBorder: string
}

export function getChartColors(): ChartColors {
  return {
    grid: getCSSVar('--chart-grid'),
    tick: getCSSVar('--chart-tick'),
    tooltipBg: getCSSVar('--chart-tooltip-bg'),
    tooltipBorder: getCSSVar('--chart-tooltip-border'),
  }
}

/**
 * Re-reads --chart-* CSS vars whenever the `dark`/`light` class on <html> changes.
 * Watches the DOM directly (MutationObserver) rather than ThemeContext so charts stay
 * decoupled from the theme provider tree and keep working in tests that render them standalone.
 */
export function useChartColors(): ChartColors {
  const [colors, setColors] = useState<ChartColors>(getChartColors)

  useEffect(() => {
    const root = document.documentElement
    const observer = new MutationObserver(() => setColors(getChartColors()))
    observer.observe(root, { attributes: true, attributeFilter: ['class'] })
    return () => observer.disconnect()
  }, [])

  return colors
}
