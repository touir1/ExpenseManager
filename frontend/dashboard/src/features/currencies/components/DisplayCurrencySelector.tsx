import { useRef, useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useDisplayCurrency } from '@/features/currencies/DisplayCurrencyContext'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'

export default function DisplayCurrencySelector() {
  const { t } = useTranslation()
  const { currencies } = useExpensesData()
  const { displayCurrencyId, setDisplayCurrencyId } = useDisplayCurrency()
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  const selected = displayCurrencyId ? currencies.find(c => c.id === displayCurrencyId) : null
  const label = selected ? `${selected.code} ${selected.symbol}` : t('currencies.noConversion', 'No conversion')

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  if (currencies.length === 0) return null

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen(v => !v)}
        className="flex items-center gap-1.5 text-sm font-medium px-2.5 py-1.5 rounded-lg text-slate-600 hover:text-slate-900 hover:bg-slate-100 transition-colors duration-150 cursor-pointer"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-label={t('currencies.displayCurrencySelector', 'Display currency')}
      >
        <svg className="h-3.5 w-3.5 text-slate-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <span>{label}</span>
        <svg className={`h-3 w-3 text-slate-400 shrink-0 transition-transform duration-150 ${open ? 'rotate-180' : ''}`} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {open && (
        <div
          role="menu"
          className="absolute right-0 top-full mt-1 w-48 bg-white border border-slate-200 rounded-xl shadow-lg py-1 z-50 max-h-64 overflow-y-auto"
        >
          <button
            role="menuitemradio"
            aria-checked={displayCurrencyId === null}
            onClick={() => { setDisplayCurrencyId(null); setOpen(false) }}
            className={`w-full text-left px-3 py-2 text-sm transition-colors duration-100 cursor-pointer ${
              displayCurrencyId === null
                ? 'text-brand-700 bg-brand-50 font-medium'
                : 'text-slate-700 hover:bg-slate-50'
            }`}
          >
            {t('currencies.noConversion', 'No conversion')}
          </button>

          {currencies.map(currency => (
            <button
              key={currency.id}
              role="menuitemradio"
              aria-checked={displayCurrencyId === currency.id}
              onClick={() => { setDisplayCurrencyId(currency.id); setOpen(false) }}
              className={`w-full text-left px-3 py-2 text-sm truncate transition-colors duration-100 cursor-pointer ${
                displayCurrencyId === currency.id
                  ? 'text-brand-700 bg-brand-50 font-medium'
                  : 'text-slate-700 hover:bg-slate-50'
              }`}
            >
              {currency.code} — {currency.name} {currency.symbol}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
