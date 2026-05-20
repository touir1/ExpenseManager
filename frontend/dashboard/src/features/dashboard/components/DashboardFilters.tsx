import { useTranslation } from 'react-i18next'
import type { DashboardFilter } from '@/features/dashboard/types/dashboard.type'

type Props = {
  filter: DashboardFilter
  onChange: (filter: DashboardFilter) => void
}

function todayStr(): string {
  return new Date().toISOString().slice(0, 10)
}

function startOfMonthStr(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`
}

function startOfYearStr(): string {
  return `${new Date().getFullYear()}-01-01`
}

export function DashboardFilters({ filter, onChange }: Props) {
  const { t } = useTranslation()

  const setThisMonth = () =>
    onChange({ ...filter, dateFrom: startOfMonthStr(), dateTo: todayStr() })

  const setThisYear = () =>
    onChange({ ...filter, dateFrom: startOfYearStr(), dateTo: todayStr() })

  const presetClass = (active: boolean) =>
    `text-xs font-medium px-2.5 py-1.5 rounded-lg border transition-colors duration-150 cursor-pointer ${
      active
        ? 'bg-brand-50 border-brand-200 text-brand-700'
        : 'bg-surface-subtle border-surface-border text-ink-mute hover:text-ink hover:bg-surface-card'
    }`

  const isThisMonth =
    filter.dateFrom === startOfMonthStr() && filter.dateTo === todayStr()
  const isThisYear =
    filter.dateFrom === startOfYearStr() && filter.dateTo === todayStr()

  return (
    <div className="flex flex-wrap items-center gap-2 mb-6">
      <button
        type="button"
        onClick={setThisMonth}
        className={presetClass(isThisMonth)}
        aria-pressed={isThisMonth}
      >
        {t('dashboard.filters.thisMonth')}
      </button>

      <button
        type="button"
        onClick={setThisYear}
        className={presetClass(isThisYear)}
        aria-pressed={isThisYear}
      >
        {t('dashboard.filters.thisYear')}
      </button>

      <div className="flex items-center gap-1 ml-auto">
        <label className="text-xs text-ink-mute sr-only" htmlFor="dash-date-from">
          {t('expenses.filters.dateFrom')}
        </label>
        <input
          id="dash-date-from"
          type="date"
          value={filter.dateFrom ?? ''}
          max={filter.dateTo ?? todayStr()}
          onChange={e => onChange({ ...filter, dateFrom: e.target.value || undefined })}
          className="text-xs border border-surface-border rounded-lg px-2 py-1.5 text-ink-body bg-surface-card focus:outline-none focus:ring-2 focus:ring-brand-300 cursor-pointer"
        />
        <span className="text-xs text-ink-faint">–</span>
        <label className="text-xs text-ink-mute sr-only" htmlFor="dash-date-to">
          {t('expenses.filters.dateTo')}
        </label>
        <input
          id="dash-date-to"
          type="date"
          value={filter.dateTo ?? ''}
          min={filter.dateFrom}
          max={todayStr()}
          onChange={e => onChange({ ...filter, dateTo: e.target.value || undefined })}
          className="text-xs border border-surface-border rounded-lg px-2 py-1.5 text-ink-body bg-surface-card focus:outline-none focus:ring-2 focus:ring-brand-300 cursor-pointer"
        />
      </div>
    </div>
  )
}
