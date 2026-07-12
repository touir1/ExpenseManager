import { useEffect, useId, useRef, useState } from 'react'
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

const TYPEAHEAD_RESET_MS = 500
const MAX_VISIBLE_OPTIONS = 50

export function FormCombobox({ id, value, onChange, options, disabled, className = 'field-input', ...ariaProps }: FormComboboxProps) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const [highlightedIndex, setHighlightedIndex] = useState(-1)
  const [dropdownStyle, setDropdownStyle] = useState<React.CSSProperties>({})
  const containerRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const dropdownRef = useRef<HTMLUListElement>(null)
  const typeaheadRef = useRef({ buffer: '', timer: undefined as ReturnType<typeof setTimeout> | undefined })
  const listboxId = useId()

  const selectedLabel = options.find(o => o.value === value)?.label ?? ''
  const filtered = query
    ? options.filter(o => o.label.toLowerCase().includes(query.toLowerCase()))
    : options
  const items: Array<ComboOption | undefined> = [undefined, ...filtered]
  const optionId = (index: number) => `${listboxId}-option-${index}`

  const overflowCount = Math.max(0, items.length - MAX_VISIBLE_OPTIONS)
  const visibleStart =
    overflowCount > 0 && highlightedIndex >= MAX_VISIBLE_OPTIONS
      ? Math.min(highlightedIndex - MAX_VISIBLE_OPTIONS + 1, items.length - MAX_VISIBLE_OPTIONS)
      : 0
  const visibleItems = overflowCount > 0 ? items.slice(visibleStart, visibleStart + MAX_VISIBLE_OPTIONS) : items

  useEffect(() => {
    if (!open) return
    const onMouseDown = (e: MouseEvent) => {
      if (
        containerRef.current?.contains(e.target as Node) ||
        dropdownRef.current?.contains(e.target as Node)
      ) return
      closeDropdown()
    }
    const onScroll = () => closeDropdown()
    document.addEventListener('mousedown', onMouseDown)
    window.addEventListener('scroll', onScroll, true)
    return () => {
      document.removeEventListener('mousedown', onMouseDown)
      window.removeEventListener('scroll', onScroll, true)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  useEffect(() => {
    if (highlightedIndex < 0) return
    dropdownRef.current?.children[highlightedIndex - visibleStart]?.scrollIntoView?.({ block: 'nearest' })
  }, [highlightedIndex, visibleStart])

  function closeDropdown() {
    setOpen(false)
    setQuery('')
    setHighlightedIndex(-1)
  }

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
    setHighlightedIndex(items.findIndex(o => o?.value === value))
  }

  function selectItem(item: ComboOption | undefined) {
    onChange(item?.value)
    closeDropdown()
  }

  function jumpToTypeahead(char: string) {
    const t = typeaheadRef.current
    clearTimeout(t.timer)
    t.buffer += char.toLowerCase()
    t.timer = setTimeout(() => { t.buffer = '' }, TYPEAHEAD_RESET_MS)
    const match = items.findIndex(o => o?.label.toLowerCase().startsWith(t.buffer))
    if (match !== -1) setHighlightedIndex(match)
  }

  function onKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (!open && (e.key === 'ArrowDown' || e.key === 'ArrowUp')) {
      e.preventDefault()
      openDropdown()
      return
    }
    if (!open) return
    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault()
        setHighlightedIndex(i => Math.min(items.length - 1, i + 1))
        break
      case 'ArrowUp':
        e.preventDefault()
        setHighlightedIndex(i => Math.max(0, i - 1))
        break
      case 'Home':
        e.preventDefault()
        setHighlightedIndex(0)
        break
      case 'End':
        e.preventDefault()
        setHighlightedIndex(items.length - 1)
        break
      case 'Enter':
        e.preventDefault()
        if (highlightedIndex >= 0) selectItem(items[highlightedIndex])
        break
      case 'Escape':
        e.preventDefault()
        closeDropdown()
        break
      default:
        if (e.key.length === 1 && /\S/.test(e.key)) jumpToTypeahead(e.key)
    }
  }

  return (
    <div ref={containerRef} className="relative">
      <input
        ref={inputRef}
        id={id}
        type="text"
        role="combobox"
        aria-expanded={open}
        aria-controls={listboxId}
        aria-autocomplete="list"
        aria-activedescendant={open && highlightedIndex >= 0 ? optionId(highlightedIndex) : undefined}
        autoComplete="off"
        className={`${className} ${disabled ? 'opacity-50 cursor-not-allowed' : ''}`}
        disabled={disabled}
        value={open ? query : selectedLabel}
        placeholder="—"
        onFocus={openDropdown}
        onChange={e => { setQuery(e.target.value); setHighlightedIndex(-1) }}
        onKeyDown={onKeyDown}
        {...ariaProps}
      />
      {open && !disabled && createPortal(
        <ul
          ref={dropdownRef}
          id={listboxId}
          role="listbox"
          style={dropdownStyle}
          className="bg-surface-card border border-surface-border rounded-lg shadow-warm max-h-48 overflow-y-auto text-ink"
        >
          {visibleItems.map((item, i) => {
            const index = visibleStart + i
            const isSelected = item?.value === value
            const isHighlighted = index === highlightedIndex
            return (
              <li
                key={item?.value ?? 'clear'}
                id={optionId(index)}
                role="option"
                aria-selected={isSelected}
                className={`px-3 py-1.5 text-sm cursor-pointer flex items-center justify-between ${
                  isHighlighted ? 'bg-brand-50' : 'hover:bg-surface-subtle'
                } ${isSelected ? 'font-semibold text-brand-600' : item ? 'text-ink' : 'text-ink-mute'}`}
                onMouseEnter={() => setHighlightedIndex(index)}
                onMouseDown={() => selectItem(item)}
              >
                {item?.label ?? '—'}
                {isSelected && (
                  <svg className="h-3.5 w-3.5 shrink-0 text-brand-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5} aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                  </svg>
                )}
              </li>
            )
          })}
          {filtered.length === 0 && (
            <li className="px-3 py-1.5 text-sm text-ink-mute">—</li>
          )}
          {overflowCount > 0 && (
            <li className="px-3 py-1.5 text-xs text-ink-faint select-none">
              {overflowCount} more — keep typing to narrow
            </li>
          )}
        </ul>,
        document.body
      )}
    </div>
  )
}
