import { createContext, useContext, useMemo, useState } from 'react'

export type Toast = { id: number; message: string; type?: 'info' | 'success' | 'error' }

const ToastContext = createContext<{ show: (message: string, type?: Toast['type']) => void } | null>(null)

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const show = (message: string, type: Toast['type'] = 'error') => {
    const id = Date.now() + Math.random()
    setToasts(t => [...t, { id, message, type }])
    // auto-dismiss
    setTimeout(() => setToasts(t => t.filter(x => x.id !== id)), 4000)
  }

  const value = useMemo(() => ({ show }), [])

  return (
    <ToastContext.Provider value={value}>
      {children}
      {/* Container */}
      <div style={{ position: 'fixed', right: 16, top: 16, display: 'grid', gap: 8, zIndex: 1000 }}>
        {toasts.map(t => (
          <div key={t.id} style={{
            padding: '10px 12px',
            borderRadius: 8,
            background: t.type === 'success' ? '#16a34a' : t.type === 'info' ? '#2563eb' : '#dc2626',
            color: 'white',
            boxShadow: '0 4px 12px rgba(0,0,0,0.2)'
          }}>
            {t.message}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}

export function useToast() {
  const ctx = useContext(ToastContext)
  if (!ctx) throw new Error('useToast must be used within ToastProvider')
  return ctx
}
