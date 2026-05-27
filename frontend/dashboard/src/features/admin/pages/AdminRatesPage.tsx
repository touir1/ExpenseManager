import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, useMutation } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import {
  getRateHistory,
  addManualRate,
  refreshRates,
} from '@/features/admin/services/adminRatesApi.service'
import { getCurrencies } from '@/features/expenses/services/currenciesApi.service'

type Currency = { id: number; code: string; name: string; symbol: string; decimals: number }

export default function AdminRatesPage() {
  const { t } = useTranslation()
  usePageTitle(t('admin.rates.pageTitle'))

  const [srcId, setSrcId] = useState<number | ''>('')
  const [dstId, setDstId] = useState<number | ''>('')
  const [addModal, setAddModal] = useState(false)
  const [backfillModal, setBackfillModal] = useState(false)
  const [rateDate, setRateDate] = useState('')
  const [rateValue, setRateValue] = useState('')
  const [backfillFrom, setBackfillFrom] = useState('')

  const { data: currencies = [] } = useQuery({
    queryKey: ['currencies'],
    queryFn: () => getCurrencies(),
    select: r => r.data ?? [],
  })

  const { data: history = [], refetch } = useQuery({
    queryKey: ['admin-rate-history', srcId, dstId],
    queryFn: () => getRateHistory(srcId as number, dstId as number),
    enabled: !!srcId && !!dstId,
    select: r => r.data ?? [],
  })

  const addMutation = useMutation({
    mutationFn: () => addManualRate(srcId as number, dstId as number, rateDate, parseFloat(rateValue)),
    onSuccess: () => { setAddModal(false); setRateDate(''); setRateValue(''); refetch() },
  })

  const refreshMutation = useMutation({
    mutationFn: () => refreshRates(backfillFrom, srcId || undefined, dstId || undefined),
    onSuccess: () => { setBackfillModal(false); setBackfillFrom(''); refetch() },
  })

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-ink">{t('admin.rates.pageTitle')}</h1>
        <div className="flex gap-2">
          <button onClick={() => setBackfillModal(true)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">
            {t('admin.rates.backfill')}
          </button>
          <button onClick={() => setAddModal(true)} disabled={!srcId || !dstId} className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60">
            {t('admin.rates.addManual')}
          </button>
        </div>
      </div>

      <div className="flex gap-3 mb-4">
        <select value={srcId} onChange={e => setSrcId(e.target.value ? Number(e.target.value) : '')}
          className="border border-surface-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-brand-300">
          <option value="">Source</option>
          {(currencies as Currency[]).map(c => <option key={c.id} value={c.id}>{c.code}</option>)}
        </select>
        <span className="text-ink-mute self-center">→</span>
        <select value={dstId} onChange={e => setDstId(e.target.value ? Number(e.target.value) : '')}
          className="border border-surface-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-brand-300">
          <option value="">Destination</option>
          {(currencies as Currency[]).map(c => <option key={c.id} value={c.id}>{c.code}</option>)}
        </select>
      </div>

      {srcId && dstId && (
        <div className="bg-white shadow-card border border-slate-200 rounded-2xl overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-surface-subtle">
              <tr>
                <th className="text-left px-4 py-2 text-ink-mute font-medium">Date</th>
                <th className="text-left px-4 py-2 text-ink-mute font-medium">Rate</th>
                <th className="text-left px-4 py-2 text-ink-mute font-medium">{t('admin.rates.source')}</th>
              </tr>
            </thead>
            <tbody>
              {(history as { id: number; date: string; rate: number; rateSourceId: number }[]).map(r => (
                <tr key={r.id} className="border-t border-surface-border">
                  <td className="px-4 py-2">{r.date}</td>
                  <td className="px-4 py-2">{r.rate}</td>
                  <td className="px-4 py-2">{r.rateSourceId === 1 ? 'Auto' : 'Manual'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {addModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-xl p-6 w-72">
            <h2 className="text-base font-semibold text-ink mb-3">{t('admin.rates.addManual')}</h2>
            <input type="date" value={rateDate} onChange={e => setRateDate(e.target.value)}
              className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-2 focus:outline-none focus:ring-2 focus:ring-brand-300" />
            <input type="number" step="any" placeholder="Rate" value={rateValue} onChange={e => setRateValue(e.target.value)}
              className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-4 focus:outline-none focus:ring-2 focus:ring-brand-300" />
            <div className="flex gap-2 justify-end">
              <button onClick={() => setAddModal(false)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">{t('common.cancel', 'Cancel')}</button>
              <button onClick={() => addMutation.mutate()} disabled={!rateDate || !rateValue || addMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60">{t('common.save', 'Save')}</button>
            </div>
          </div>
        </div>
      )}

      {backfillModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-xl p-6 w-72">
            <h2 className="text-base font-semibold text-ink mb-3">{t('admin.rates.backfill')}</h2>
            <input type="date" value={backfillFrom} onChange={e => setBackfillFrom(e.target.value)}
              className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-4 focus:outline-none focus:ring-2 focus:ring-brand-300" />
            <div className="flex gap-2 justify-end">
              <button onClick={() => setBackfillModal(false)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">{t('common.cancel', 'Cancel')}</button>
              <button onClick={() => refreshMutation.mutate()} disabled={!backfillFrom || refreshMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60">{t('admin.rates.backfill')}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
