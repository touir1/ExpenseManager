import { useEffect, useRef, useState } from 'react'

export interface ComboOption {
  value: number
  label: string
}

export interface FormComboboxProps {
  id?: string
  value: number | undefined
  onChange: (value: number | undefined) => void
  options: ComboOption[]
  disabled?: boolean
  className?: string
  'aria-describedby'?: string
  'aria-invalid'?: boolean
}

export function FormCombobox({ id, value, onChange, options, disabled, className = 'field-input', ...ariaProps }: FormComboboxProps) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const containerRef = useRef<HTMLDivElement>(null)

  const selectedLabel = options.find(o => o.value === value)?.label ?? ''
  const filtered = query
    ? options.filter(o => o.label.toLowerCase().includes(query.toLowerCase()))
    : options

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false)
        setQuery('')
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  return (
    <div ref={containerRef} className="relative">
      <input
        id={id}
        type="text"
        autoComplete="off"
        className={`${className} ${disabled ? 'opacity-50 cursor-not-allowed' : ''}`}
        disabled={disabled}
        value={open ? query : selectedLabel}
        placeholder="—"
        onFocus={() => { if (!disabled) { setOpen(true); setQuery('') } }}
        onChange={e => setQuery(e.target.value)}
        {...ariaProps}
      />
      {open && !disabled && (
        <ul
          role="listbox"
          className="absolute z-30 w-full mt-1 bg-surface-card border border-surface-border rounded-lg shadow-lg max-h-48 overflow-y-auto"
        >
          <li
            role="option"
            aria-selected={value === undefined}
            className="px-3 py-1.5 text-sm cursor-pointer hover:bg-surface-subtle text-ink-mute"
            onMouseDown={() => { onChange(undefined); setOpen(false); setQuery('') }}
          >
            —
          </li>
          {filtered.map(o => (
            <li
              key={o.value}
              role="option"
              aria-selected={o.value === value}
              className={`px-3 py-1.5 text-sm cursor-pointer hover:bg-surface-subtle ${o.value === value ? 'font-semibold' : ''}`}
              onMouseDown={() => { onChange(o.value); setOpen(false); setQuery('') }}
            >
              {o.label}
            </li>
          ))}
          {filtered.length === 0 && (
            <li className="px-3 py-1.5 text-sm text-ink-mute">—</li>
          )}
        </ul>
      )}
    </div>
  )
}
