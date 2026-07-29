export function formatExpenseDate(dateStr: string): string {
  return new Date(dateStr + 'T00:00:00').toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })
}
