import { createContext, useContext, useMemo, useRef, useState } from 'react'

export type Toast = { id: string; message: string; type?: 'info' | 'success' | 'error'; count: number; createdAt: number }

const GROUP_WINDOW_MS = 3000
const DISMISS_MS = 4000

const ToastContext = createContext<{ show: (message: string, type?: Toast['type']) => void } | null>(null)

const toastStyles: Record<NonNullable<Toast['type']>, string> = {
  success: 'bg-sage-soft  border-sage/30  text-ink-body',
  info:    'bg-sky-50     border-sky-200  text-sky-800',
  error:   'bg-berry-soft border-berry/30 text-ink-body',
}

const toastIcons: Record<NonNullable<Toast['type']>, JSX.Element> = {
  success: (
    <svg className="h-4 w-4 shrink-0 text-sage" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
    </svg>
  ),
  info: (
    <svg className="h-4 w-4 shrink-0 text-sky-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M13 16h-1v-4h-1m1-4h.01M12 2a10 10 0 110 20A10 10 0 0112 2z" />
    </svg>
  ),
  error: (
    <svg className="h-4 w-4 shrink-0 text-berry" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
    </svg>
  ),
}

export function ToastProvider({ children }: Readonly<{ children: React.ReactNode }>) {
  const [toasts, setToasts] = useState<Toast[]>([])
  const timeoutsRef = useRef<Record<string, ReturnType<typeof setTimeout>>>({})

  const scheduleDismiss = (id: string) => {
    if (timeoutsRef.current[id]) clearTimeout(timeoutsRef.current[id])
    timeoutsRef.current[id] = setTimeout(() => {
      delete timeoutsRef.current[id]
      setToasts(t => t.filter(x => x.id !== id))
    }, DISMISS_MS)
  }

  const show = (message: string, type: Toast['type'] = 'error') => {
    const now = Date.now()
    setToasts(prev => {
      const idx = prev.findIndex(t => t.type === type && now - t.createdAt < GROUP_WINDOW_MS)
      if (idx !== -1) {
        scheduleDismiss(prev[idx].id)
        const next = [...prev]
        next[idx] = { ...prev[idx], message, count: prev[idx].count + 1, createdAt: now }
        return next
      }
      const id = crypto.randomUUID()
      scheduleDismiss(id)
      return [...prev, { id, message, type, count: 1, createdAt: now }]
    })
  }

  const value = useMemo(() => ({ show }), [])

  return (
    <ToastContext.Provider value={value}>
      {children}
      {/* Toast container – fixed top-right, stacks vertically */}
      <section
        className="fixed right-4 top-4 z-50 flex flex-col gap-2 w-80 max-w-[calc(100vw-2rem)]"
        aria-label="Notifications"
      >
        {toasts.map(t => {
          const type = t.type!
          return (
            <div
              key={t.id}
              role={type === 'error' ? 'alert' : 'status'}
              aria-live={type === 'error' ? 'assertive' : 'polite'}
              aria-atomic="true"
              className={`flex items-start gap-3 px-4 py-3 rounded-xl border shadow-card-md text-sm font-medium transition-opacity duration-200 ${toastStyles[type]}`}
            >
              {toastIcons[type]}
              <span className="flex-1">{t.message}</span>
              {t.count > 1 && (
                <span
                  aria-label={`${t.count} notifications`}
                  className="shrink-0 min-w-[18px] h-[18px] px-1 rounded-full bg-white/60 text-[11px] font-bold flex items-center justify-center leading-none"
                >
                  {t.count}
                </span>
              )}
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
