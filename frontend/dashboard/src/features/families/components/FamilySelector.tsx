import { useRef, useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useFamilies } from '@/features/families/FamilyContext'

export default function FamilySelector() {
  const { t } = useTranslation()
  const { families, activeFamilyId, setActiveFamilyId } = useFamilies()
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  const activeFamilies = families.filter(f => !f.isArchived)
  const activeFamily = activeFamilyId
    ? activeFamilies.find(f => f.id === activeFamilyId)
    : null

  const label = activeFamily?.name ?? t('families.selectorPersonal')

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

  if (activeFamilies.length === 0) return null

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen(v => !v)}
        className="flex items-center gap-1.5 text-sm font-medium px-2.5 py-1.5 rounded-lg text-slate-600 hover:text-slate-900 hover:bg-slate-100 transition-colors duration-150 cursor-pointer max-w-[140px]"
        aria-haspopup="listbox"
        aria-expanded={open}
      >
        <svg className="h-3.5 w-3.5 text-slate-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
          <path strokeLinecap="round" strokeLinejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
        </svg>
        <span className="truncate">{label}</span>
        <svg className={`h-3 w-3 text-slate-400 shrink-0 transition-transform duration-150 ${open ? 'rotate-180' : ''}`} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
          <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {open && (
        <div
          role="listbox"
          className="absolute right-0 top-full mt-1 w-48 bg-white border border-slate-200 rounded-xl shadow-lg py-1 z-50"
        >
          <button
            role="option"
            aria-selected={activeFamilyId === null}
            onClick={() => { setActiveFamilyId(null); setOpen(false) }}
            className={`w-full text-left px-3 py-2 text-sm transition-colors duration-100 cursor-pointer ${
              activeFamilyId === null
                ? 'text-brand-700 bg-brand-50 font-medium'
                : 'text-slate-700 hover:bg-slate-50'
            }`}
          >
            {t('families.selectorPersonal')}
          </button>

          {activeFamilies.filter(f => !f.isDefault).map(family => (
            <button
              key={family.id}
              role="option"
              aria-selected={activeFamilyId === family.id}
              onClick={() => { setActiveFamilyId(family.id); setOpen(false) }}
              className={`w-full text-left px-3 py-2 text-sm truncate transition-colors duration-100 cursor-pointer ${
                activeFamilyId === family.id
                  ? 'text-brand-700 bg-brand-50 font-medium'
                  : 'text-slate-700 hover:bg-slate-50'
              }`}
            >
              {family.name}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
