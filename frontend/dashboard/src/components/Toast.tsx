import { createContext, useContext, useMemo, useState } from 'react'

export type Toast = { id: number; message: string; type?: 'info' | 'success' | 'error' }

const ToastContext = createContext<{ show: (message: string, type?: Toast['type']) => void } | null>(null)

const toastStyles: Record<NonNullable<Toast['type']>, string> = {
  success: 'bg-emerald-50 border-emerald-200 text-emerald-800',
  info:    'bg-sky-50    border-sky-200    text-sky-800',
  error:   'bg-rose-50   border-rose-200   text-rose-800',
}

const toastIcons: Record<NonNullable<Toast['type']>, JSX.Element> = {
  success: (
    <svg className="h-4 w-4 shrink-0 text-emerald-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
    </svg>
  ),
  info: (
    <svg className="h-4 w-4 shrink-0 text-sky-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M13 16h-1v-4h-1m1-4h.01M12 2a10 10 0 110 20A10 10 0 0112 2z" />
    </svg>
  ),
  error: (
    <svg className="h-4 w-4 shrink-0 text-rose-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
    </svg>
  ),
}

export function ToastProvider({ children }: Readonly<{ children: React.ReactNode }>) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const show = (message: string, type: Toast['type'] = 'error') => {
    const id = Date.now() + Math.random()
    setToasts(t => [...t, { id, message, type }])
    const removeById = (t: Toast[]) => t.filter(x => x.id !== id)
    setTimeout(() => setToasts(removeById), 4000)
  }

  const value = useMemo(() => ({ show }), [])

  return (
    <ToastContext.Provider value={value}>
      {children}
      {/* Toast container – fixed top-right, stacks vertically */}
      <section
        className="fixed right-4 top-4 z-50 flex flex-col gap-2 w-80 max-w-[calc(100vw-2rem)]"
        aria-live="polite"
        aria-label="Notifications"
      >
        {toasts.map(t => {
          const type = t.type!
          return (
            <div
              key={t.id}
              className={`flex items-start gap-3 px-4 py-3 rounded-xl border shadow-card-md text-sm font-medium transition-opacity duration-200 ${toastStyles[type]}`}
            >
              {toastIcons[type]}
              <span>{t.message}</span>
            </div>
          )
        })}
      </section>
    </ToastContext.Provider>
  )
}

export function useToast() {
  const ctx = useContext(ToastContext)
  if (!ctx) throw new Error('useToast must be used within ToastProvider')
  return ctx
}
