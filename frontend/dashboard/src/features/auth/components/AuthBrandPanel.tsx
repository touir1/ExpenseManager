export default function AuthBrandPanel() {
  return (
    <div
      className="hidden lg:flex flex-col justify-between flex-none relative overflow-hidden p-14"
      style={{
        flexBasis: '46%',
        background: 'linear-gradient(155deg, #C8623E 0%, #A24B2A 60%, #6E2F18 100%)',
        color: '#FAF6EE',
      }}
    >
      {/* Decorative radial glows */}
      <div style={{ position: 'absolute', right: -80, top: -100, width: 380, height: 380, borderRadius: '50%', background: 'radial-gradient(circle, rgba(255,220,180,0.18) 0%, rgba(255,220,180,0) 70%)', pointerEvents: 'none' }} />
      <div style={{ position: 'absolute', left: -60, bottom: -80, width: 280, height: 280, borderRadius: '50%', background: 'radial-gradient(circle, rgba(214,162,63,0.25) 0%, rgba(214,162,63,0) 70%)', pointerEvents: 'none' }} />

      {/* Logo wordmark */}
      <div className="relative flex items-center gap-3">
        <span
          className="inline-flex items-center justify-center rounded-[10px] shrink-0"
          style={{ width: 32, height: 32, background: '#FAF6EE' }}
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
            <path d="M5 20V8c0-1 .8-2 2-2h2c1.2 0 2 .8 2 2v4h2V8c0-1 .8-2 2-2h2c1.2 0 2 .8 2 2v12" stroke="#A24B2A" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
          </svg>
        </span>
        <span className="font-bold tracking-tight" style={{ fontSize: 18, color: '#FAF6EE' }}>
          Expense<span style={{ color: '#D6A23F' }}>Manager.</span>
        </span>
      </div>

      {/* Hero tagline */}
      <div className="relative">
        <h2
          className="font-serif leading-[1.05] tracking-[-0.01em] mb-4"
          style={{ fontSize: 44, color: '#FAF6EE', fontStyle: 'normal' }}
        >
          Welcome to the<br />
          <em className="font-serif" style={{ fontStyle: 'italic', color: '#F4E1B5' }}>
            kitchen counter.
          </em>
        </h2>
        <p className="leading-[1.55] max-w-[380px]" style={{ fontSize: 15, color: '#F4E1D3' }}>
          Where the family money lives — soft-edged, shared, and quietly intelligent.
        </p>

        {/* Floating receipt card */}
        <div
          className="mt-9 flex items-center gap-3 rounded-2xl"
          style={{
            padding: '16px 18px',
            background: 'rgba(255,252,246,0.08)',
            backdropFilter: 'blur(10px)',
            border: '1px solid rgba(255,252,246,0.2)',
          }}
        >
          <span style={{ fontSize: 26 }} aria-hidden="true">🛒</span>
          <div className="flex-1">
            <div className="font-semibold" style={{ fontSize: 14, color: '#FAF6EE' }}>Weekly shop</div>
            <div style={{ fontSize: 12, color: '#F4D7C9' }}>Maya · split with the family</div>
          </div>
          <div className="font-serif" style={{ fontSize: 24, color: '#FAF6EE' }}>€80.90</div>
        </div>
      </div>

      {/* Footer */}
      <div className="relative flex justify-between items-center" style={{ fontSize: 12, color: '#F4D7C9' }}>
        <span>© ExpenseManager, 2026</span>
      </div>
    </div>
  )
}
