import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import { getPendingConflicts, resolveConflict, type RateConflictDto } from '@/features/admin/services/adminRatesApi.service'
import { getCurrencies } from '@/features/expenses/services/currenciesApi.service'

type Currency = { id: number; code: string }

function currencyCode(currencies: Currency[], id: number) {
  return currencies.find(c => c.id === id)?.code ?? String(id)
}

export default function AdminRateConflictsPage() {
  const { t } = useTranslation()
  usePageTitle(t('admin.rateConflicts.pageTitle'))
  const qc = useQueryClient()

  const [resolutions, setResolutions] = useState<Record<number, { mode: 'AcceptAuto' | 'KeepManual' | 'Custom'; custom?: string }>>({})

  const { data: conflicts = [] } = useQuery({
    queryKey: ['admin-rate-conflicts'],
    queryFn: () => getPendingConflicts(),
    select: r => r.data ?? [],
  })

  const { data: currencies = [] } = useQuery({
    queryKey: ['currencies'],
    queryFn: () => getCurrencies(),
    select: r => r.data ?? [],
  })

  const resolveMutation = useMutation({
    mutationFn: ({ id, resolution, custom }: { id: number; resolution: string; custom?: number }) =>
      resolveConflict(id, resolution, custom),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-rate-conflicts'] }),
  })

  const getResolution = (id: number) => resolutions[id] ?? { mode: 'AcceptAuto' }

  const resolve = (conflict: RateConflictDto) => {
    const r = getResolution(conflict.id)
    resolveMutation.mutate({
      id: conflict.id,
      resolution: r.mode,
      custom: r.mode === 'Custom' ? parseFloat(r.custom ?? '0') : undefined,
    })
  }

  const resolveAll = () => {
    (conflicts as RateConflictDto[]).forEach(c => resolveMutation.mutate({ id: c.id, resolution: 'AcceptAuto' }))
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-ink">{t('admin.rateConflicts.pageTitle')}</h1>
        {(conflicts as RateConflictDto[]).length > 0 && (
          <button onClick={resolveAll} className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600">
            {t('admin.rateConflicts.resolveAll')}
          </button>
        )}
      </div>

      {(conflicts as RateConflictDto[]).length === 0 ? (
        <p className="text-ink-mute text-sm">{t('admin.rateConflicts.noConflicts', 'No pending conflicts.')}</p>
      ) : (
        <div className="flex flex-col gap-3">
          {(conflicts as RateConflictDto[]).map(c => {
            const r = getResolution(c.id)
            const delta = c.autoRate && c.manualRate
              ? (Math.abs(c.autoRate - c.manualRate) / c.manualRate * 100).toFixed(1)
              : '—'
            return (
              <div key={c.id} className="bg-white shadow-card border border-slate-200 rounded-xl p-4">
                <div className="flex items-start justify-between gap-4">
                  <div className="text-sm">
                    <p className="font-medium text-ink">
                      {currencyCode(currencies as Currency[], c.sourceCurrencyId)} → {currencyCode(currencies as Currency[], c.destinationCurrencyId)}
                      <span className="text-ink-mute ml-2">{c.date}</span>
                    </p>
                    <p className="text-ink-mute mt-1">
                      {t('admin.rateConflicts.autoRate')}: <span className="text-ink">{c.autoRate}</span>
                      {' · '}
                      {t('admin.rateConflicts.manualRate')}: <span className="text-ink">{c.manualRate}</span>
                      {' · '}
                      Δ {delta}%
                    </p>
                  </div>
                  <div className="flex flex-col gap-2 min-w-48">
                    {(['AcceptAuto', 'KeepManual', 'Custom'] as const).map(mode => (
                      <label key={mode} className="flex items-center gap-2 text-sm cursor-pointer">
                        <input type="radio" name={`resolution-${c.id}`} checked={r.mode === mode}
                          onChange={() => setResolutions(prev => ({ ...prev, [c.id]: { mode } }))} />
                        {t(`admin.rateConflicts.${mode === 'AcceptAuto' ? 'acceptAuto' : mode === 'KeepManual' ? 'keepManual' : 'custom'}`)}
                      </label>
                    ))}
                    {r.mode === 'Custom' && (
                      <input type="number" step="any" placeholder="Custom rate" value={r.custom ?? ''}
                        onChange={e => setResolutions(prev => ({ ...prev, [c.id]: { mode: 'Custom', custom: e.target.value } }))}
                        className="border border-surface-border rounded-lg px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-brand-300" />
                    )}
                    <button onClick={() => resolve(c)} disabled={resolveMutation.isPending}
                      className="text-xs px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60 mt-1">
                      {t('admin.rateConflicts.resolve')}
                    </button>
                  </div>
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
