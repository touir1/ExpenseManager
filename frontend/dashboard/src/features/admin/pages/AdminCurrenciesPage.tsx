import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import { addCurrency, setDefaultRate } from '@/features/admin/services/adminCurrenciesApi.service'
import { getCurrencies } from '@/features/expenses/services/currenciesApi.service'

export default function AdminCurrenciesPage() {
  const { t } = useTranslation()
  usePageTitle(t('admin.currencies.pageTitle'))
  const qc = useQueryClient()

  const [addModal, setAddModal] = useState(false)
  const [defaultRateModal, setDefaultRateModal] = useState(false)
  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [symbol, setSymbol] = useState('')
  const [decimals, setDecimals] = useState(2)
  const [srcId, setSrcId] = useState<number | ''>('')
  const [dstId, setDstId] = useState<number | ''>('')
  const [rate, setRate] = useState('')

  const { data: currencies = [] } = useQuery({
    queryKey: ['currencies'],
    queryFn: () => getCurrencies(),
    select: r => r.data ?? [],
  })

  const addMutation = useMutation({
    mutationFn: () => addCurrency(code.toUpperCase(), name, symbol, decimals),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] })
      setAddModal(false)
      setCode(''); setName(''); setSymbol(''); setDecimals(2)
    },
  })

  const defaultRateMutation = useMutation({
    mutationFn: () => setDefaultRate(srcId as number, dstId as number, parseFloat(rate)),
    onSuccess: () => { setDefaultRateModal(false); setSrcId(''); setDstId(''); setRate('') },
  })

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-ink">{t('admin.currencies.pageTitle')}</h1>
        <div className="flex gap-2">
          <button onClick={() => setDefaultRateModal(true)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">
            {t('admin.currencies.setDefaultRate')}
          </button>
          <button onClick={() => setAddModal(true)} className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600">
            {t('admin.currencies.add')}
          </button>
        </div>
      </div>

      <div className="bg-white shadow-card border border-slate-200 rounded-2xl overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-surface-subtle">
            <tr>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">Code</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">Name</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">Symbol</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">Decimals</th>
            </tr>
          </thead>
          <tbody>
            {(currencies as { id: number; code: string; name: string; symbol: string; decimals: number }[]).map(c => (
              <tr key={c.id} className="border-t border-surface-border">
                <td className="px-4 py-2 font-mono font-medium">{c.code}</td>
                <td className="px-4 py-2">{c.name}</td>
                <td className="px-4 py-2">{c.symbol}</td>
                <td className="px-4 py-2">{c.decimals}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {addModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-xl p-6 w-80">
            <h2 className="text-base font-semibold text-ink mb-3">{t('admin.currencies.add')}</h2>
            {[
              { label: 'Code (3 chars)', value: code, set: setCode },
              { label: 'Name', value: name, set: setName },
              { label: 'Symbol', value: symbol, set: setSymbol },
            ].map(f => (
              <input key={f.label} type="text" placeholder={f.label} value={f.value} onChange={e => f.set(e.target.value)}
                className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-2 focus:outline-none focus:ring-2 focus:ring-brand-300" />
            ))}
            <input type="number" placeholder="Decimals" value={decimals} onChange={e => setDecimals(parseInt(e.target.value))}
              className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-4 focus:outline-none focus:ring-2 focus:ring-brand-300" />
            <div className="flex gap-2 justify-end">
              <button onClick={() => setAddModal(false)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">{t('common.cancel', 'Cancel')}</button>
              <button onClick={() => addMutation.mutate()} disabled={!code || !name || !symbol || addMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60">{t('common.save', 'Save')}</button>
            </div>
          </div>
        </div>
      )}

      {defaultRateModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-xl p-6 w-80">
            <h2 className="text-base font-semibold text-ink mb-3">{t('admin.currencies.setDefaultRate')}</h2>
            <select value={srcId} onChange={e => setSrcId(Number(e.target.value))} className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-2 focus:outline-none focus:ring-2 focus:ring-brand-300">
              <option value="">Source currency</option>
              {(currencies as { id: number; code: string }[]).map(c => <option key={c.id} value={c.id}>{c.code}</option>)}
            </select>
            <select value={dstId} onChange={e => setDstId(Number(e.target.value))} className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-2 focus:outline-none focus:ring-2 focus:ring-brand-300">
              <option value="">Destination currency</option>
              {(currencies as { id: number; code: string }[]).map(c => <option key={c.id} value={c.id}>{c.code}</option>)}
            </select>
            <input type="number" step="any" placeholder="Rate" value={rate} onChange={e => setRate(e.target.value)}
              className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-4 focus:outline-none focus:ring-2 focus:ring-brand-300" />
            <div className="flex gap-2 justify-end">
              <button onClick={() => setDefaultRateModal(false)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">{t('common.cancel', 'Cancel')}</button>
              <button onClick={() => defaultRateMutation.mutate()} disabled={!srcId || !dstId || !rate || defaultRateMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60">{t('common.save', 'Save')}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
