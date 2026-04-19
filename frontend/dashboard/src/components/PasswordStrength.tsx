import type { FC } from 'react'

const CRITERIA = [
  { key: 'length',    label: 'At least 8 characters',     test: (p: string) => p.length >= 8 },
  { key: 'uppercase', label: 'Uppercase letter (A–Z)',     test: (p: string) => /[A-Z]/.test(p) },
  { key: 'lowercase', label: 'Lowercase letter (a–z)',     test: (p: string) => /[a-z]/.test(p) },
  { key: 'number',    label: 'Number (0–9)',               test: (p: string) => /\d/.test(p) },
  { key: 'special',   label: 'Special character (!@#…)',   test: (p: string) => /[^A-Za-z0-9]/.test(p) },
]

const LEVELS = [
  null,
  { label: 'Weak',   barColor: 'bg-rose-500',    textColor: 'text-rose-600' },
  { label: 'Weak',   barColor: 'bg-rose-500',    textColor: 'text-rose-600' },
  { label: 'Fair',   barColor: 'bg-amber-500',   textColor: 'text-amber-600' },
  { label: 'Good',   barColor: 'bg-lime-500',    textColor: 'text-lime-600' },
  { label: 'Strong', barColor: 'bg-emerald-500', textColor: 'text-emerald-600' },
]

const PasswordStrength: FC<{ password: string }> = ({ password }) => {
  if (!password) return null

  const results = CRITERIA.map(c => ({ ...c, met: c.test(password) }))
  const score   = results.filter(r => r.met).length
  const level   = LEVELS[score]

  return (
    <div className="mt-2.5 space-y-2">
      <div className="flex gap-1" aria-hidden="true">
        {results.map((c, i) => (
          <div
            key={c.key}
            className={`h-1 flex-1 rounded-full transition-colors duration-200 ${
              i < score ? (level?.barColor ?? 'bg-slate-200') : 'bg-slate-200'
            }`}
          />
        ))}
      </div>

      {level && (
        <p className="text-xs text-slate-500">
          Strength:{' '}
          <span className={`font-semibold ${level.textColor}`} data-testid="strength-label">
            {level.label}
          </span>
        </p>
      )}

      <ul className="space-y-0.5" aria-label="Password requirements">
        {results.map(c => (
          <li
            key={c.key}
            className={`text-xs flex items-center gap-1.5 ${c.met ? 'text-emerald-600' : 'text-slate-400'}`}
          >
            <span aria-hidden="true">{c.met ? '✓' : '○'}</span>
            {c.label}
          </li>
        ))}
      </ul>
    </div>
  )
}

export default PasswordStrength
