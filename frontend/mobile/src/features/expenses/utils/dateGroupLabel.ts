export function dateGroupLabel(dateStr: string, t: (key: string, fallback: string) => string, now = new Date()): string {
  const date = new Date(dateStr)
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  const target = new Date(date.getFullYear(), date.getMonth(), date.getDate())
  const diffDays = Math.round((today.getTime() - target.getTime()) / 86400000)

  if (diffDays === 0) return t('common.today', 'Today')
  if (diffDays === 1) return t('common.yesterday', 'Yesterday')
  return date.toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric' })
}
