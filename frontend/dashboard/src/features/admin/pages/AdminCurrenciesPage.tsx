import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import {
  addCurrency,
  updateCurrency,
  deleteCurrency,
  getCurrencyDefaults,
  setDefaultRate,
  type CurrencyDefaultRateDto,
} from '@/features/admin/services/adminCurrenciesApi.service'
import { getCurrencies } from '@/features/expenses/services/currenciesApi.service'
import { FormCombobox } from '@/components/FormCombobox'

const INPUT = 'w-full border border-surface-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-brand-300'

type Currency = { id: number; code: string; name: string; symbol: string; decimals: number }

export default function AdminCurrenciesPage() {
  const { t } = useTranslation()
  usePageTitle(t('admin.currencies.pageTitle'))
  const qc = useQueryClient()

  const [addModal, setAddModal] = useState(false)
  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [symbol, setSymbol] = useState('')
  const [decimals, setDecimals] = useState(2)

  const [editModal, setEditModal] = useState<Currency | null>(null)
  const [editName, setEditName] = useState('')
  const [editSymbol, setEditSymbol] = useState('')
  const [editDecimals, setEditDecimals] = useState(2)

  const [deleteModal, setDeleteModal] = useState<Currency | null>(null)
  const [deleteError, setDeleteError] = useState('')

  const [defaultsModal, setDefaultsModal] = useState<Currency | null>(null)
  const [editingRates, setEditingRates] = useState<Record<number, string>>({})
  const [addPairDstId, setAddPairDstId] = useState<number | undefined>(undefined)
  const [addPairRate, setAddPairRate] = useState('')

  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const PAGE_SIZE = 10

  const { data: currencies = [] } = useQuery({
    queryKey: ['currencies'],
    queryFn: () => getCurrencies(),
    select: r => r.data ?? [],
  })

  const { data: defaultRates = [] } = useQuery<CurrencyDefaultRateDto[]>({
    queryKey: ['admin-currency-defaults', defaultsModal?.id],
    queryFn: () => getCurrencyDefaults(defaultsModal!.id).then(r => r.data ?? []),
    enabled: !!defaultsModal,
  })

  const addMutation = useMutation({
    mutationFn: () => addCurrency(code.toUpperCase(), name, symbol, decimals),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] })
      setAddModal(false)
      setCode(''); setName(''); setSymbol(''); setDecimals(2)
    },
  })

  const editMutation = useMutation({
    mutationFn: () => updateCurrency(editModal!.id, editName, editSymbol, editDecimals),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] })
      setEditModal(null)
    },
  })

  const deleteMutation = useMutation({
    mutationFn: () => deleteCurrency(deleteModal!.id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] })
      setDeleteModal(null)
      setDeleteError('')
    },
    onError: (err: { status?: number }) => {
      if (err?.status === 409) {
        setDeleteError(t('admin.currencies.inUse'))
      }
    },
  })

  const defaultRateMutation = useMutation({
    mutationFn: ({ srcId, dstId, r }: { srcId: number; dstId: number; r: number }) =>
      setDefaultRate(srcId, dstId, r),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-currency-defaults', defaultsModal?.id] })
      setEditingRates({})
      setAddPairDstId(undefined)
      setAddPairRate('')
    },
  })

  const openEdit = (c: Currency) => {
    setEditModal(c)
    setEditName(c.name)
    setEditSymbol(c.symbol)
    setEditDecimals(c.decimals)
  }

  const openDelete = (c: Currency) => {
    setDeleteModal(c)
    setDeleteError('')
  }

  const openDefaults = (c: Currency) => {
    setDefaultsModal(c)
    setEditingRates({})
    setAddPairDstId(undefined)
    setAddPairRate('')
  }

  const pairedDestIds = new Set(defaultRates.map(d => d.destinationCurrencyId))
  const addPairOptions = (currencies as Currency[])
    .filter(c => c.id !== defaultsModal?.id && !pairedDestIds.has(c.id))
    .map(c => ({ value: c.id, label: c.code }))

  const filtered = (currencies as Currency[]).filter(c =>
    !search ||
    c.code.toLowerCase().includes(search.toLowerCase()) ||
    c.name.toLowerCase().includes(search.toLowerCase())
  )
  const totalPages = Math.ceil(filtered.length / PAGE_SIZE) || 1
  const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-ink">{t('admin.currencies.pageTitle')}</h1>
        <button onClick={() => setAddModal(true)} className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600">
          {t('admin.currencies.add')}
        </button>
      </div>

      <div className="mb-4">
        <input
          type="text"
          placeholder={t('admin.currencies.search')}
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1) }}
          className="border border-surface-border rounded-lg px-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-brand-300"
        />
      </div>

      <div className="bg-surface-card shadow-card border border-surface-border rounded-2xl overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-surface-subtle">
            <tr>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">Code</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">Name</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">Symbol</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">Decimals</th>
              <th className="text-left px-4 py-2 text-ink-mute font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {paged.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-4 py-4 text-center text-ink-mute text-sm">{t('admin.currencies.noResults')}</td>
              </tr>
            ) : paged.map(c => (
              <tr key={c.id} className="border-t border-surface-border">
                <td className="px-4 py-2 font-mono font-medium">{c.code}</td>
                <td className="px-4 py-2">{c.name}</td>
                <td className="px-4 py-2">{c.symbol}</td>
                <td className="px-4 py-2">{c.decimals}</td>
                <td className="px-4 py-2 flex gap-1">
                  <button onClick={() => openEdit(c)} className="text-xs px-2 py-1 rounded bg-brand-50 text-brand-600 hover:bg-brand-100">
                    {t('admin.currencies.edit')}
                  </button>
                  <button onClick={() => openDelete(c)} className="text-xs px-2 py-1 rounded bg-red-50 text-red-600 hover:bg-red-100">
                    {t('admin.currencies.delete')}
                  </button>
                  <button onClick={() => openDefaults(c)} className="text-xs px-2 py-1 rounded bg-surface-subtle text-ink-mute hover:bg-surface-muted">
                    {t('admin.currencies.defaults')}
                  </button>
                </td>
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

      {/* Add modal */}
      {addModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-surface-card rounded-2xl shadow-xl p-6 w-80">
            <h2 className="text-base font-semibold text-ink mb-3">{t('admin.currencies.add')}</h2>
            {[
              { label: t('admin.currencies.codeLabel'), value: code, set: setCode },
              { label: t('admin.currencies.nameLabel'), value: name, set: setName },
              { label: t('admin.currencies.symbolLabel'), value: symbol, set: setSymbol },
            ].map(f => (
              <input key={f.label} type="text" placeholder={f.label} value={f.value} onChange={e => f.set(e.target.value)} className={`${INPUT} mb-2`} />
            ))}
            <input type="number" placeholder="Decimals" value={decimals} onChange={e => setDecimals(parseInt(e.target.value))} className={`${INPUT} mb-4`} />
            <div className="flex gap-2 justify-end">
              <button onClick={() => setAddModal(false)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">{t('admin.currencies.cancel')}</button>
              <button onClick={() => addMutation.mutate()} disabled={!code || !name || !symbol || addMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60">{t('admin.currencies.save')}</button>
            </div>
          </div>
        </div>
      )}

      {/* Edit modal */}
      {editModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-surface-card rounded-2xl shadow-xl p-6 w-80">
            <h2 className="text-base font-semibold text-ink mb-3">{t('admin.currencies.edit')} — {editModal.code}</h2>
            <input type="text" placeholder={t('admin.currencies.nameLabel')} value={editName} onChange={e => setEditName(e.target.value)} className={`${INPUT} mb-2`} />
            <input type="text" placeholder={t('admin.currencies.symbolLabel')} value={editSymbol} onChange={e => setEditSymbol(e.target.value)} className={`${INPUT} mb-2`} />
            <input type="number" placeholder="Decimals" value={editDecimals} onChange={e => setEditDecimals(parseInt(e.target.value))} className={`${INPUT} mb-4`} />
            <div className="flex gap-2 justify-end">
              <button onClick={() => setEditModal(null)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">{t('admin.currencies.cancel')}</button>
              <button onClick={() => editMutation.mutate()} disabled={!editName || !editSymbol || editMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60">{t('admin.currencies.save')}</button>
            </div>
          </div>
        </div>
      )}

      {/* Delete modal */}
      {deleteModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-surface-card rounded-2xl shadow-xl p-6 w-80">
            <h2 className="text-base font-semibold text-ink mb-3">{t('admin.currencies.delete')} — {deleteModal.code}</h2>
            <p className="text-sm text-ink-body mb-3">{t('admin.currencies.deleteConfirm')}</p>
            {deleteError && <p className="text-xs text-red-600 mb-3">{deleteError}</p>}
            <div className="flex gap-2 justify-end">
              <button onClick={() => setDeleteModal(null)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">{t('admin.currencies.cancel')}</button>
              <button onClick={() => deleteMutation.mutate()} disabled={deleteMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-red-500 text-white hover:bg-red-600 disabled:opacity-60">{t('admin.currencies.delete')}</button>
            </div>
          </div>
        </div>
      )}

      {/* Defaults modal */}
      {defaultsModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-surface-card rounded-2xl shadow-xl p-6 w-[560px]">
            <h2 className="text-base font-semibold text-ink mb-4">
              {t('admin.currencies.defaultRatesTitle', { code: defaultsModal.code })}
            </h2>
            <table className="w-full text-sm mb-4">
              <thead className="bg-surface-subtle">
                <tr>
                  <th className="text-left px-3 py-1.5 text-ink-mute font-medium">Destination</th>
                  <th className="text-left px-3 py-1.5 text-ink-mute font-medium">Default Rate</th>
                  <th className="text-left px-3 py-1.5 text-ink-mute font-medium">{t('admin.currencies.lastAutoRate')}</th>
                  <th className="text-left px-3 py-1.5 text-ink-mute font-medium">{t('admin.currencies.lastRateDate')}</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {defaultRates.map(d => {
                  const editing = editingRates[d.destinationCurrencyId] !== undefined
                  const inputVal = editing ? editingRates[d.destinationCurrencyId] : String(d.defaultRate ?? '')
                  return (
                    <tr key={d.destinationCurrencyId} className="border-t border-surface-border">
                      <td className="px-3 py-1.5 font-mono">{d.destinationCode}</td>
                      <td className="px-3 py-1.5">
                        <input
                          type="number"
                          step="any"
                          value={inputVal}
                          onChange={e => setEditingRates(prev => ({ ...prev, [d.destinationCurrencyId]: e.target.value }))}
                          className="border border-surface-border rounded px-2 py-1 text-sm w-28 focus:outline-none focus:ring-1 focus:ring-brand-300"
                        />
                      </td>
                      <td className="px-3 py-1.5 text-ink-mute">{d.lastAutoRate ?? '—'}</td>
                      <td className="px-3 py-1.5 text-ink-mute">{d.lastAutoRateDate ?? '—'}</td>
                      <td className="px-3 py-1.5">
                        {editing && (
                          <div className="flex gap-1">
                            <button
                              onClick={() => defaultRateMutation.mutate({ srcId: defaultsModal.id, dstId: d.destinationCurrencyId, r: parseFloat(editingRates[d.destinationCurrencyId]) })}
                              disabled={defaultRateMutation.isPending}
                              className="text-xs px-2 py-0.5 rounded bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60"
                            >{t('admin.currencies.save')}</button>
                            <button
                              onClick={() => setEditingRates(prev => { const n = { ...prev }; delete n[d.destinationCurrencyId]; return n })}
                              className="text-xs px-2 py-0.5 rounded border border-surface-border text-ink-mute hover:bg-surface-subtle"
                            >{t('admin.currencies.cancel')}</button>
                          </div>
                        )}
                      </td>
                    </tr>
                  )
                })}
                {/* Add pair row */}
                <tr className="border-t border-surface-border">
                  <td className="px-3 py-1.5">
                    <FormCombobox
                      value={addPairDstId}
                      onChange={v => setAddPairDstId(v)}
                      options={addPairOptions}
                      className="border border-surface-border rounded-lg px-3 py-1.5 text-sm w-28 focus:outline-none focus:ring-1 focus:ring-brand-300"
                    />
                  </td>
                  <td className="px-3 py-1.5">
                    <input
                      type="number"
                      step="any"
                      placeholder={t('admin.currencies.rateLabel')}
                      value={addPairRate}
                      onChange={e => setAddPairRate(e.target.value)}
                      className="border border-surface-border rounded px-2 py-1 text-sm w-28 focus:outline-none focus:ring-1 focus:ring-brand-300"
                    />
                  </td>
                  <td colSpan={2} />
                  <td className="px-3 py-1.5">
                    <button
                      onClick={() => {
                        if (addPairDstId && addPairRate) {
                          defaultRateMutation.mutate({ srcId: defaultsModal.id, dstId: addPairDstId, r: parseFloat(addPairRate) })
                        }
                      }}
                      disabled={!addPairDstId || !addPairRate || defaultRateMutation.isPending}
                      className="text-xs px-2 py-0.5 rounded bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60"
                    >{t('admin.currencies.addPair')}</button>
                  </td>
                </tr>
              </tbody>
            </table>
            <div className="flex justify-end">
              <button onClick={() => setDefaultsModal(null)} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">
                {t('admin.currencies.cancel')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
