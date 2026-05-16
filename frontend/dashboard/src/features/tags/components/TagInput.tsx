import { useEffect, useRef, useState } from 'react'
import { getTags, useTag as adoptTag } from '@/features/tags/services/tagsApi.service'
import type { Tag, TagList } from '@/features/tags/types/tag.type'

interface TagInputProps {
  readonly value: Tag[]
  readonly onChange: (tags: Tag[]) => void
}

export default function TagInput({ value, onChange }: TagInputProps) {
  const [tagList, setTagList] = useState<TagList>({ own: [], family: [] })
  const [query, setQuery] = useState('')
  const [open, setOpen] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    getTags().then(res => {
      if (res.ok && res.data) setTagList(res.data)
    })
  }, [])

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  const selectedIds = new Set(value.map(t => t.id))

  const filterGroup = (tags: Tag[]) =>
    tags.filter(t => !selectedIds.has(t.id) && t.name.toLowerCase().includes(query.toLowerCase()))

  const filteredOwn = filterGroup(tagList.own)
  const filteredFamily = filterGroup(tagList.family)

  const allVisible = [...tagList.own, ...tagList.family]
  const exactMatch = allVisible.some(t => t.name === query)
  const showCreate = query.trim().length > 0 && !exactMatch

  const select = (tag: Tag) => {
    onChange([...value, tag])
    setQuery('')
    setOpen(false)
  }

  const handleCreate = async () => {
    const res = await adoptTag(query.trim())
    if (res.ok && res.data) {
      select(res.data)
    }
  }

  const removeAt = (index: number) => {
    const next = [...value]
    next.splice(index, 1)
    onChange(next)
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Escape') {
      setOpen(false)
    } else if (e.key === 'Backspace' && query === '' && value.length > 0) {
      removeAt(value.length - 1)
    } else if (e.key === 'Enter') {
      e.preventDefault()
      const first = filteredOwn[0] ?? filteredFamily[0]
      if (first) {
        select(first)
      } else if (showCreate) {
        handleCreate()
      }
    }
  }

  const hasDropdown = filteredOwn.length > 0 || filteredFamily.length > 0 || showCreate

  return (
    <div ref={containerRef} className="relative">
      <div className="flex flex-wrap gap-1 p-1.5 min-h-[2.5rem] border border-slate-300 rounded-lg focus-within:ring-2 focus-within:ring-brand-500 focus-within:border-brand-500 bg-white cursor-text">
        {value.map((tag, i) => (
          <span
            key={tag.id}
            className="inline-flex items-center gap-1 px-2 py-0.5 bg-brand-100 text-brand-700 text-sm rounded-md"
          >
            {tag.name}
            <button
              type="button"
              aria-label={`Remove ${tag.name}`}
              onClick={e => { e.stopPropagation(); removeAt(i) }}
              className="text-brand-500 hover:text-brand-700 leading-none cursor-pointer"
            >
              ×
            </button>
          </span>
        ))}
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={e => { setQuery(e.target.value); setOpen(true) }}
          onFocus={() => setOpen(true)}
          onKeyDown={handleKeyDown}
          className="flex-1 min-w-[6rem] outline-none text-sm py-0.5 px-1 bg-transparent"
          aria-label="Tag input"
          aria-expanded={open && hasDropdown}
          aria-haspopup="menu"
          aria-controls="tag-input-menu"
          role="combobox"
          aria-autocomplete="list"
        />
      </div>

      {open && hasDropdown && (
        <div
          id="tag-input-menu"
          role="menu"
          className="absolute z-50 left-0 right-0 top-full mt-1 bg-white border border-slate-200 rounded-xl shadow-lg py-1 max-h-60 overflow-y-auto"
        >
          {filteredOwn.length > 0 && (
            <>
              <div className="px-3 py-1 text-xs font-semibold text-slate-400 uppercase tracking-wide select-none">
                My tags
              </div>
              {filteredOwn.map(tag => (
                <button
                  key={tag.id}
                  type="button"
                  role="menuitem"
                  onClick={() => select(tag)}
                  className="w-full text-left px-3 py-1.5 text-sm text-slate-700 hover:bg-slate-50 cursor-pointer"
                >
                  {tag.name}
                </button>
              ))}
            </>
          )}

          {filteredFamily.length > 0 && (
            <>
              <div className="px-3 py-1 text-xs font-semibold text-slate-400 uppercase tracking-wide select-none">
                Family tags
              </div>
              {filteredFamily.map(tag => (
                <button
                  key={tag.id}
                  type="button"
                  role="menuitem"
                  onClick={() => select(tag)}
                  className="w-full text-left px-3 py-1.5 text-sm text-slate-700 hover:bg-slate-50 cursor-pointer"
                >
                  {tag.name}
                </button>
              ))}
            </>
          )}

          {showCreate && (
            <button
              type="button"
              role="menuitem"
              onClick={handleCreate}
              className="w-full text-left px-3 py-1.5 text-sm text-brand-600 hover:bg-brand-50 cursor-pointer"
            >
              Create &ldquo;{query.trim()}&rdquo;
            </button>
          )}
        </div>
      )}
    </div>
  )
}
