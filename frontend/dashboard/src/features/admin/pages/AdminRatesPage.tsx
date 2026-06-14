import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import {
  getRateHistory,
  addManualRate,
  refreshRates,
} from '@/features/admin/services/adminRatesApi.service'
import { getCurrencies } from '@/features/expenses/services/currenciesApi.service'
import { FormCombobox } from '@/components/FormCombobox'

const COMBO_CLASS = 'border border-surface-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-brand-300'

type Currency = { id: number; code: string; name: string; symbol: string; decimals: number }

export default function AdminRatesPage() {
  const { t } = useTranslation()
  usePageTitle(t('admin.rates.pageTitle'))
  const qc = useQueryClient()

  const [srcId, setSrcId] = useState<number | undefined>(undefined)
  const [dstId, setDstId] = useState<number | undefined>(undefined)
  const [page, setPage] = useState(1)

  const [addModal, setAddModal] = useState(false)
  const [modalSrcId, setModalSrcId] = useState<number | undefined>(undefined)
  const [modalDstId, setModalDstId] = useState<number | undefined>(undefined)
  const [rateDate, setRateDate] = useState('')
  const [rateValue, setRateValue] = useState('')

  const [backfillModal, setBackfillModal] = useState(false)
  const [backfillFrom, setBackfillFrom] = useState('')
  const [backfillTo, setBackfillTo] = useState('')

  const { data: currencies = [] } = useQuery({
    queryKey: ['currencies'],
    queryFn: () => getCurrencies(),
    select: r => r.data ?? [],
  })

  const { data: historyData } = useQuery({
    queryKey: ['admin-rate-history', srcId, dstId, page],
    queryFn: () => getRateHistory(srcId, dstId, page),
    select: r => r.data,
  })

  const rates = historyData?.rates ?? []
  const total = historyData?.total ?? 0
  const pageSize = historyData?.pageSize ?? 50
  const totalPages = Math.ceil(total / pageSize)

  const addMutation = useMutation({
    mutationFn: () => addManualRate(modalSrcId!, modalDstId!, rateDate, parseFloat(rateValue)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-rate-history'] })
      setAddModal(false)
      setModalSrcId(undefined)
      setModalDstId(undefined)
      setRateDate('')
      setRateValue('')
    },
  })

  const refreshMutation = useMutation({
    mutationFn: () => refreshRates(backfillFrom, backfillTo || undefined, srcId, dstId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-rate-history'] })
      setBackfillModal(false)
      setBackfillFrom('')
      setBackfillTo('')
    },
  })

  const currencyOptions = (currencies as Currency[]).map(c => ({ value: c.id, label: c.code }))
  const currencyCode = (id?: number) => (currencies as Currency[]).find(c => c.id === id)?.code ?? '—'

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-ink">{t('admin.rates.pageTitle')}</h1>
        <div className="flex gap-2">
          <button onClick={() => setBackfillModal(true)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">
            {t('admin.rates.backfill')}
          </button>
          <button onClick={() => setAddModal(true)} className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600">
            {t('admin.rates.addManual')}
          </button>
        </div>
      </div>

      <div className="flex gap-3 mb-4">
        <FormCombobox
          value={srcId}
          onChange={v => { setSrcId(v); setPage(1) }}
          options={currencyOptions}
          className={COMBO_CLASS}
        />
        <span className="text-ink-mute self-center">→</span>
        <FormCombobox
          value={dstId}
          onChange={v => { setDstId(v); setPage(1) }}
          options={currencyOptions}
          className={COMBO_CLASS}
        />
      </div>

      <div className="bg-surface-card shadow-card border border-surface-border rounded-2xl overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-surface-subtle">
            <tr>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">{t('admin.rates.fromCurrency')}</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">{t('admin.rates.toCurrency')}</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">{t('admin.rates.rateDate')}</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">{t('admin.rates.rateValue')}</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">{t('admin.rates.source')}</th>
            </tr>
          </thead>
          <tbody>
            {rates.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-4 py-4 text-center text-ink-mute text-sm">{t('admin.rates.noRates')}</td>
              </tr>
            ) : rates.map(r => (
              <tr key={r.id} className="border-t border-surface-border">
                <td className="px-4 py-2 font-mono">{currencyCode(r.sourceCurrencyId)}</td>
                <td className="px-4 py-2 font-mono">{currencyCode(r.destinationCurrencyId)}</td>
                <td className="px-4 py-2">{r.date}</td>
                <td className="px-4 py-2">{r.rate}</td>
                <td className="px-4 py-2">{r.rateSource}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex gap-2 mt-4 justify-end">
          <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}
            className="text-sm px-3 py-1 rounded border border-surface-border disabled:opacity-40">←</button>
          <span className="text-sm text-ink-mute py-1">{page} / {totalPages}</span>
          <button disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}
            className="text-sm px-3 py-1 rounded border border-surface-border disabled:opacity-40">→</button>
        </div>
      )}

      {addModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-surface-card rounded-2xl shadow-xl p-6 w-80">
            <h2 className="text-base font-semibold text-ink mb-3">{t('admin.rates.addManual')}</h2>
            <div className="mb-2">
              <label className="block text-xs text-ink-mute mb-1">{t('admin.rates.sourceCurrency')}</label>
              <FormCombobox value={modalSrcId} onChange={setModalSrcId} options={currencyOptions} className={COMBO_CLASS} />
            </div>
            <div className="mb-2">
              <label className="block text-xs text-ink-mute mb-1">{t('admin.rates.destinationCurrency')}</label>
              <FormCombobox value={modalDstId} onChange={setModalDstId} options={currencyOptions} className={COMBO_CLASS} />
            </div>
            <input type="date" value={rateDate} onChange={e => setRateDate(e.target.value)}
              className={`w-full ${COMBO_CLASS} mb-2`} />
            <input type="number" step="any" placeholder={t('admin.rates.rateValue')} value={rateValue} onChange={e => setRateValue(e.target.value)}
              className={`w-full ${COMBO_CLASS} mb-4`} />
            <div className="flex gap-2 justify-end">
              <button onClick={() => { setAddModal(false); setModalSrcId(undefined); setModalDstId(undefined); setRateDate(''); setRateValue('') }}
                className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">{t('common.cancel', 'Cancel')}</button>
              <button onClick={() => addMutation.mutate()} disabled={!modalSrcId || !modalDstId || !rateDate || !rateValue || addMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60">{t('common.save', 'Save')}</button>
            </div>
          </div>
        </div>
      )}

      {backfillModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-surface-card rounded-2xl shadow-xl p-6 w-72">
            <h2 className="text-base font-semibold text-ink mb-3">{t('admin.rates.backfill')}</h2>
            <label className="block text-xs text-ink-mute mb-1">{t('admin.rates.from')}</label>
            <input type="date" value={backfillFrom} onChange={e => setBackfillFrom(e.target.value)}
              className={`w-full ${COMBO_CLASS} mb-2`} />
            <label className="block text-xs text-ink-mute mb-1">{t('admin.rates.to')}</label>
            <input type="date" value={backfillTo} onChange={e => setBackfillTo(e.target.value)}
              className={`w-full ${COMBO_CLASS} mb-4`} />
            <div className="flex gap-2 justify-end">
              <button onClick={() => { setBackfillModal(false); setBackfillFrom(''); setBackfillTo('') }}
                className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">{t('common.cancel', 'Cancel')}</button>
              <button onClick={() => refreshMutation.mutate()} disabled={!backfillFrom || refreshMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60">{t('admin.rates.backfill')}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
