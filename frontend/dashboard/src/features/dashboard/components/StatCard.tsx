import type { ReactNode } from 'react'

type Props = {
  label: string
  value: ReactNode
  isLoading?: boolean
  trend?: { text: string; positive: boolean; title?: string }
  subtext?: string
}

function Skeleton() {
  return (
    <div
      className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-5 animate-pulse"
      role="status"
      aria-label="Loading stat"
    >
      <div className="h-3.5 bg-surface-muted rounded w-20 mb-3" />
      <div className="h-7 bg-surface-subtle rounded w-24" />
    </div>
  )
}

export function StatCard({ label, value, isLoading, trend, subtext }: Readonly<Props>) {
  if (isLoading) return <Skeleton />

  const trendClass = trend?.positive
    ? 'bg-sage-soft text-sage'
    : 'bg-berry-soft text-berry'

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-5">
      <p className="text-xs font-semibold uppercase tracking-widest text-ink-mute mb-2">{label}</p>
      <div className="flex items-baseline gap-2 flex-wrap">
        <span className="text-2xl font-bold text-ink tracking-tight">{value}</span>
        {trend && (
          <span
            className={`inline-flex items-center text-xs font-semibold px-2 py-0.5 rounded-full ${trendClass}`}
            title={trend.title}
          >
            {trend.text}
          </span>
        )}
      </div>
      {subtext && <p className="text-xs text-ink-faint mt-1.5">{subtext}</p>}
    </div>
  )
}
