import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'

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
  const [dropdownStyle, setDropdownStyle] = useState<React.CSSProperties>({})
  const containerRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const dropdownRef = useRef<HTMLUListElement>(null)

  const selectedLabel = options.find(o => o.value === value)?.label ?? ''
  const filtered = query
    ? options.filter(o => o.label.toLowerCase().includes(query.toLowerCase()))
    : options

  useEffect(() => {
    if (!open) return
    const onMouseDown = (e: MouseEvent) => {
      if (
        containerRef.current?.contains(e.target as Node) ||
        dropdownRef.current?.contains(e.target as Node)
      ) return
      setOpen(false)
      setQuery('')
    }
    const onScroll = () => { setOpen(false); setQuery('') }
    document.addEventListener('mousedown', onMouseDown)
    window.addEventListener('scroll', onScroll, true)
    return () => {
      document.removeEventListener('mousedown', onMouseDown)
      window.removeEventListener('scroll', onScroll, true)
    }
  }, [open])

  function openDropdown() {
    if (disabled) return
    const rect = inputRef.current?.getBoundingClientRect()
    if (rect) {
      setDropdownStyle({
        position: 'fixed',
        top: rect.bottom + 4,
        left: rect.left,
        width: rect.width,
        zIndex: 9999,
      })
    }
    setOpen(true)
    setQuery('')
  }

  return (
    <div ref={containerRef} className="relative">
      <input
        ref={inputRef}
        id={id}
        type="text"
        autoComplete="off"
        className={`${className} ${disabled ? 'opacity-50 cursor-not-allowed' : ''}`}
        disabled={disabled}
        value={open ? query : selectedLabel}
        placeholder="—"
        onFocus={openDropdown}
        onChange={e => setQuery(e.target.value)}
        {...ariaProps}
      />
      {open && !disabled && createPortal(
        <ul
          ref={dropdownRef}
          role="listbox"
          style={dropdownStyle}
          className="bg-surface-card border border-surface-border rounded-lg shadow-lg max-h-48 overflow-y-auto text-ink"
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
              className={`px-3 py-1.5 text-sm cursor-pointer hover:bg-surface-subtle text-ink ${o.value === value ? 'font-semibold' : ''}`}
              onMouseDown={() => { onChange(o.value); setOpen(false); setQuery('') }}
            >
              {o.label}
            </li>
          ))}
          {filtered.length === 0 && (
            <li className="px-3 py-1.5 text-sm text-ink-mute">—</li>
          )}
        </ul>,
        document.body
      )}
    </div>
  )
}
