import { Link } from 'react-router-dom'

interface EmptyStateAction {
  readonly label: string
  readonly onClick?: () => void
  readonly to?: string
}

interface EmptyStateProps {
  readonly icon?: string
  readonly title: string
  readonly subtitle?: string
  readonly action?: EmptyStateAction
  readonly compact?: boolean
}

export default function EmptyState({ icon = '📭', title, subtitle, action, compact = false }: EmptyStateProps) {
  return (
    <div className={`flex flex-col items-center justify-center text-center ${compact ? 'py-8' : 'py-16'}`}>
      <div className={`select-none ${compact ? 'text-3xl mb-2' : 'text-5xl mb-4'}`} aria-hidden="true">{icon}</div>
      <p className={`font-semibold text-ink ${compact ? 'text-sm' : 'text-base mb-1'}`}>{title}</p>
      {subtitle && <p className="text-sm text-ink-mute max-w-sm mt-1 mb-4">{subtitle}</p>}
      {action && (
        action.to ? (
          <Link to={action.to} className="btn-primary px-6 py-2.5 text-sm font-semibold rounded-xl">
            {action.label}
          </Link>
        ) : (
          <button onClick={action.onClick} className="btn-primary px-6 py-2.5 text-sm font-semibold rounded-xl">
            {action.label}
          </button>
        )
      )}
    </div>
  )
}
