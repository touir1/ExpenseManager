export function formatAmountDisplay(value: number, decimals = 2): string {
  return value.toLocaleString(undefined, { minimumFractionDigits: decimals, maximumFractionDigits: decimals })
}

export function parseAmountInput(raw: string): number | undefined {
  const stripped = raw.replace(/[^\d.-]/g, '')
  if (stripped === '' || stripped === '-') return undefined
  const parsed = Number(stripped)
  return Number.isNaN(parsed) ? undefined : parsed
}

/** Keeps only digits and a single decimal point, for live-typing input (before blur formatting). */
export function sanitizeAmountInputChars(raw: string): string {
  const digitsAndDot = raw.replace(/[^\d.]/g, '')
  const firstDot = digitsAndDot.indexOf('.')
  if (firstDot === -1) return digitsAndDot
  return digitsAndDot.slice(0, firstDot + 1) + digitsAndDot.slice(firstDot + 1).replace(/\./g, '')
}
