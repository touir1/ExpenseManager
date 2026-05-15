import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { usePageTitle } from '@/hooks/usePageTitle'

function HeroReceipt() {
  return (
    <div
      className="relative bg-surface-card rounded-3xl p-7 border border-surface-border"
      style={{
        transform: 'rotate(-3deg)',
        boxShadow: '0 30px 60px -30px rgba(60,30,10,.35), 0 8px 24px -12px rgba(60,30,10,.15)',
      }}
    >
      <div
        className="absolute -top-4 left-14 w-20 h-6 rounded-sm"
        style={{ background: 'rgba(214,162,63,.45)', transform: 'rotate(-6deg)' }}
      />

      <div className="flex justify-between items-start mb-4">
        <div>
          <div className="text-[11px] font-bold tracking-widest text-ink-mute uppercase">Saturday · 12 Sep</div>
          <div className="font-serif text-2xl text-ink mt-1">The weekly shop</div>
        </div>
        <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-brand-100 text-brand-600 text-xs font-semibold">
          🛒 Groceries
        </span>
      </div>

      <div className="border-t border-b border-dashed border-surface-border py-3 flex flex-col gap-2 mb-4">
        {([
          ['Sourdough, eggs, milk', '€11.40'],
          ['School-lunch supplies', '€23.80'],
          ['Sunday roast', '€38.50'],
          ['Apples, pears, carrots', '€7.20'],
        ] as [string, string][]).map(([label, amount]) => (
          <div key={label} className="flex justify-between text-sm text-ink-body">
            <span>{label}</span>
            <span className="font-mono">{amount}</span>
          </div>
        ))}
      </div>

      <div className="flex justify-between items-baseline">
        <div className="text-sm text-ink-mute">
          Paid by <strong className="text-ink font-semibold">Maya</strong>
        </div>
        <div className="font-serif text-3xl text-ink">€80.90</div>
      </div>

      <div className="mt-3 px-3 py-2.5 rounded-xl flex items-center gap-2" style={{ background: '#F9E8DD' }}>
        {([['MR', '#C8623E'], ['JR', '#6B8E5A'], ['I', '#D6A23F']] as [string, string][]).map(
          ([init, color], i) => (
            <span
              key={init}
              className="h-6 w-6 rounded-full flex items-center justify-center text-white text-[10px] font-bold shrink-0"
              style={{ background: color, marginLeft: i ? -6 : 0 }}
            >
              {init}
            </span>
          )
        )}
        <span className="text-xs font-semibold text-brand-600 ml-1">Split 3 ways · €26.97 each</span>
      </div>
    </div>
  )
}

export default function HomePublicPage() {
  const { t } = useTranslation()
  usePageTitle()

  return (
    <div className="bg-surface-page flex-1">
      <div className="max-w-6xl mx-auto px-6 lg:px-14 py-16 lg:py-24">
        <div className="grid lg:grid-cols-2 gap-12 lg:gap-20 items-center">
          {/* Left: hero text */}
          <div>
            <span className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-mustard-soft text-ink-body text-xs font-semibold mb-6">
              <span className="h-1.5 w-1.5 rounded-full bg-mustard shrink-0"></span>
              Smart family finance
            </span>

            <h1 className="font-serif text-5xl lg:text-[68px] leading-[1.02] text-ink tracking-tight mb-5">
              {t('public.home.headline')}{' '}
              <em className="text-brand-500" style={{ fontStyle: 'italic' }}>
                {t('public.home.headlineAccent')}
              </em>
            </h1>

            <p className="text-base lg:text-lg text-ink-body leading-relaxed mb-8 max-w-md">
              {t('public.home.description')}
            </p>

            <div className="flex flex-col sm:flex-row gap-3">
              <Link
                to="/login"
                className="inline-flex items-center justify-center gap-2 px-6 py-3 rounded-full bg-ink hover:opacity-90 text-surface-page text-sm font-semibold transition-opacity duration-150"
                style={{ boxShadow: '0 8px 20px -4px rgba(30,20,10,.25)' }}
              >
                {t('public.home.signIn')}
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                  <path d="M5 12h14M13 5l7 7-7 7" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              </Link>
              <Link
                to="/register"
                className="inline-flex items-center justify-center px-6 py-3 rounded-full bg-surface-card hover:bg-surface-subtle text-ink text-sm font-semibold border border-surface-border transition-colors duration-150"
              >
                {t('public.home.createAccount')}
              </Link>
            </div>

            <div className="flex items-center gap-3 mt-6 text-xs text-ink-mute">
              <div className="flex">
                {([['MR', '#C8623E'], ['JR', '#6B8E5A'], ['I', '#D6A23F'], ['A', '#5C8C9E']] as [string, string][]).map(
                  ([init, color], i) => (
                    <span
                      key={init}
                      className="h-7 w-7 rounded-full border-2 border-surface-page flex items-center justify-center text-white text-[10px] font-bold shrink-0"
                      style={{ background: color, marginLeft: i ? -8 : 0 }}
                    >
                      {init}
                    </span>
                  )
                )}
              </div>
              Free for the whole family · No card required
            </div>
          </div>

          {/* Right: receipt visual */}
          <div className="hidden lg:flex items-center justify-center relative h-[440px]">
            <div
              className="absolute inset-0 rounded-3xl"
              style={{ background: 'radial-gradient(60% 60% at 50% 50%, #F9E8DD 0%, transparent 70%)' }}
            />
            <div className="relative">
              <HeroReceipt />
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
