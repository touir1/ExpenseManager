import type { FC } from 'react'
import { useTranslation } from 'react-i18next'

const LEVELS = [
  null,
  { key: 'weak',   barColor: 'bg-rose-500',    textColor: 'text-rose-600' },
  { key: 'weak',   barColor: 'bg-rose-500',    textColor: 'text-rose-600' },
  { key: 'fair',   barColor: 'bg-amber-500',   textColor: 'text-amber-600' },
  { key: 'good',   barColor: 'bg-lime-500',    textColor: 'text-lime-600' },
  { key: 'strong', barColor: 'bg-emerald-500', textColor: 'text-emerald-600' },
]

const PasswordStrength: FC<{ password: string }> = ({ password }) => {
  const { t } = useTranslation()

  const CRITERIA = [
    { key: 'length',    label: t('passwordStrength.criteria.length'),    test: (p: string) => p.length >= 8 },
    { key: 'uppercase', label: t('passwordStrength.criteria.uppercase'),  test: (p: string) => /[A-Z]/.test(p) },
    { key: 'lowercase', label: t('passwordStrength.criteria.lowercase'),  test: (p: string) => /[a-z]/.test(p) },
    { key: 'number',    label: t('passwordStrength.criteria.number'),     test: (p: string) => /\d/.test(p) },
    { key: 'special',   label: t('passwordStrength.criteria.special'),    test: (p: string) => /[^A-Za-z0-9]/.test(p) },
  ]

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
              /* c8 ignore next */
              i < score ? (level?.barColor ?? 'bg-slate-200') : 'bg-slate-200'
            }`}
          />
        ))}
      </div>

      {level && (
        <p className="text-xs text-slate-500">
          {t('passwordStrength.label')}:{' '}
          <span className={`font-semibold ${level.textColor}`} data-testid="strength-label">
            {t(`passwordStrength.levels.${level.key}`)}
          </span>
        </p>
      )}

      <ul className="space-y-0.5" aria-label={t('passwordStrength.requirements')}>
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
