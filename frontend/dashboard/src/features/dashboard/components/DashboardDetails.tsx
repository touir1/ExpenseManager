import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ReactNode } from 'react'

type TabKey = 'recent' | 'largest' | 'recurring'

type Props = {
  recent: ReactNode
  largest: ReactNode
  recurring: ReactNode
}

export function DashboardDetails({ recent, largest, recurring }: Readonly<Props>) {
  const { t } = useTranslation()
  const [active, setActive] = useState<TabKey>('recent')

  const tabs: { key: TabKey; label: string }[] = [
    { key: 'recent', label: t('dashboard.recent.title') },
    { key: 'largest', label: t('dashboard.largest.title') },
    { key: 'recurring', label: t('dashboard.recurring.title') },
  ]

  const panels: Record<TabKey, ReactNode> = { recent, largest, recurring }

  return (
    <div>
      <div role="tablist" aria-label={t('dashboard.title')} className="flex items-center gap-1 mb-3">
        {tabs.map(tab => (
          <button
            key={tab.key}
            role="tab"
            type="button"
            id={`dashboard-tab-${tab.key}`}
            aria-controls={`dashboard-tabpanel-${tab.key}`}
            aria-selected={active === tab.key}
            onClick={() => setActive(tab.key)}
            className={`text-xs font-semibold px-3 py-1.5 rounded-lg transition-colors duration-150 cursor-pointer ${
              active === tab.key
                ? 'bg-brand-50 text-brand-700'
                : 'text-ink-mute hover:text-ink hover:bg-surface-subtle'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {tabs.map(tab => (
        <div
          key={tab.key}
          role="tabpanel"
          id={`dashboard-tabpanel-${tab.key}`}
          aria-labelledby={`dashboard-tab-${tab.key}`}
          hidden={active !== tab.key}
        >
          {panels[tab.key]}
        </div>
      ))}
    </div>
  )
}
