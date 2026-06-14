import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import {
  getCategories,
  addCategory,
  updateCategory,
  archiveCategory,
  unarchiveCategory,
  addSubcategory,
  updateSubcategory,
  archiveSubcategory,
  unarchiveSubcategory,
  type AdminCategory,
  type AdminSubcategory,
} from '@/features/admin/services/adminCategoriesApi.service'

type CategoryModalState = { mode: 'add' } | { mode: 'edit'; id: number; name: string; description?: string }
type SubcategoryModalState = { mode: 'add'; parentId: number } | { mode: 'edit'; parentId: number; id: number; name: string; description?: string }

export default function AdminCategoriesPage() {
  const { t } = useTranslation()
  usePageTitle(t('admin.categories.pageTitle'))
  const qc = useQueryClient()

  const [showArchived, setShowArchived] = useState(false)
  const [expanded, setExpanded] = useState<Set<number>>(new Set())
  const [catModal, setCatModal] = useState<CategoryModalState | null>(null)
  const [subModal, setSubModal] = useState<SubcategoryModalState | null>(null)
  const [formName, setFormName] = useState('')
  const [formDesc, setFormDesc] = useState('')

  const { data: categories = [], isLoading } = useQuery({
    queryKey: ['admin-categories'],
    queryFn: () => getCategories(),
    select: r => r.data ?? [],
  })

  const invalidate = () => qc.invalidateQueries({ queryKey: ['admin-categories'] })

  const addCatMutation = useMutation({ mutationFn: ({ name, desc }: { name: string; desc?: string }) => addCategory(name, desc), onSuccess: invalidate })
  const updateCatMutation = useMutation({ mutationFn: ({ id, name, desc }: { id: number; name: string; desc?: string }) => updateCategory(id, name, desc), onSuccess: invalidate })
  const archiveCatMutation = useMutation({ mutationFn: (id: number) => archiveCategory(id), onSuccess: invalidate })
  const unarchiveCatMutation = useMutation({ mutationFn: (id: number) => unarchiveCategory(id), onSuccess: invalidate })
  const addSubMutation = useMutation({ mutationFn: ({ pid, name, desc }: { pid: number; name: string; desc?: string }) => addSubcategory(pid, name, desc), onSuccess: invalidate })
  const updateSubMutation = useMutation({ mutationFn: ({ pid, id, name, desc }: { pid: number; id: number; name: string; desc?: string }) => updateSubcategory(pid, id, name, desc), onSuccess: invalidate })
  const archiveSubMutation = useMutation({ mutationFn: ({ pid, id }: { pid: number; id: number }) => archiveSubcategory(pid, id), onSuccess: invalidate })
  const unarchiveSubMutation = useMutation({ mutationFn: ({ pid, id }: { pid: number; id: number }) => unarchiveSubcategory(pid, id), onSuccess: invalidate })

  const openCatModal = (state: CategoryModalState) => {
    setCatModal(state)
    setFormName(state.mode === 'edit' ? state.name : '')
    setFormDesc(state.mode === 'edit' ? (state.description ?? '') : '')
  }

  const openSubModal = (state: SubcategoryModalState) => {
    setSubModal(state)
    setFormName(state.mode === 'edit' ? state.name : '')
    setFormDesc(state.mode === 'edit' ? (state.description ?? '') : '')
  }

  const submitCatModal = () => {
    if (!catModal) return
    if (catModal.mode === 'add') addCatMutation.mutate({ name: formName, desc: formDesc || undefined })
    else updateCatMutation.mutate({ id: catModal.id, name: formName, desc: formDesc || undefined })
    setCatModal(null)
  }

  const submitSubModal = () => {
    if (!subModal) return
    if (subModal.mode === 'add') addSubMutation.mutate({ pid: subModal.parentId, name: formName, desc: formDesc || undefined })
    else updateSubMutation.mutate({ pid: subModal.parentId, id: subModal.id, name: formName, desc: formDesc || undefined })
    setSubModal(null)
  }

  const visible = categories.filter((c: AdminCategory) => showArchived || !c.isArchived)

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-ink">{t('admin.categories.pageTitle')}</h1>
        <div className="flex gap-2">
          <label className="flex items-center gap-2 text-sm text-ink-mute cursor-pointer">
            <input type="checkbox" checked={showArchived} onChange={e => setShowArchived(e.target.checked)} />
            {t('admin.categories.showArchived')}
          </label>
          <button
            onClick={() => openCatModal({ mode: 'add' })}
            className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600"
          >
            {t('admin.categories.add')}
          </button>
        </div>
      </div>

      {isLoading ? (
        <p className="text-ink-mute text-sm">{t('common.loading', 'Loading…')}</p>
      ) : (
        <div className="flex flex-col gap-2">
          {visible.map((cat: AdminCategory) => (
            <div key={cat.id} className={`bg-surface-card border border-surface-border rounded-xl shadow-card ${cat.isArchived ? 'opacity-60' : ''}`}>
              <div className="flex items-center justify-between px-4 py-3">
                <button
                  onClick={() => setExpanded(prev => { const s = new Set(prev); s.has(cat.id) ? s.delete(cat.id) : s.add(cat.id); return s })}
                  className="flex items-center gap-2 text-sm font-medium text-ink"
                >
                  <span>{expanded.has(cat.id) ? '▼' : '▶'}</span>
                  <span>{cat.name}</span>
                  {cat.isArchived && <span className="text-xs text-ink-mute ml-1">(archived)</span>}
                </button>
                <div className="flex gap-2">
                  <button onClick={() => openCatModal({ mode: 'edit', id: cat.id, name: cat.name, description: cat.description })} className="text-xs px-2 py-1 rounded bg-surface-subtle text-ink-mute hover:bg-surface-muted">{t('admin.categories.edit')}</button>
                  {cat.isArchived ? (
                    <button onClick={() => unarchiveCatMutation.mutate(cat.id)} className="text-xs px-2 py-1 rounded bg-green-50 text-green-700 hover:bg-green-100">{t('admin.categories.unarchive')}</button>
                  ) : (
                    <>
                      <button onClick={() => openSubModal({ mode: 'add', parentId: cat.id })} className="text-xs px-2 py-1 rounded bg-brand-50 text-brand-600 hover:bg-brand-100">{t('admin.categories.addSub')}</button>
                      <button onClick={() => archiveCatMutation.mutate(cat.id)} className="text-xs px-2 py-1 rounded bg-red-50 text-red-700 hover:bg-red-100">{t('admin.categories.archive')}</button>
                    </>
                  )}
                </div>
              </div>

              {expanded.has(cat.id) && cat.subcategories.length > 0 && (
                <div className="border-t border-surface-border px-4 py-2 flex flex-col gap-1">
                  {cat.subcategories.filter((s: AdminSubcategory) => showArchived || !s.isArchived).map((sub: AdminSubcategory) => (
                    <div key={sub.id} className={`flex items-center justify-between text-sm py-1 ${sub.isArchived ? 'opacity-60' : ''}`}>
                      <span className="text-ink-mute">{sub.name}{sub.isArchived && ' (archived)'}</span>
                      <div className="flex gap-2">
                        <button onClick={() => openSubModal({ mode: 'edit', parentId: cat.id, id: sub.id, name: sub.name, description: sub.description })} className="text-xs px-2 py-0.5 rounded bg-surface-subtle text-ink-mute hover:bg-surface-muted">{t('admin.categories.edit')}</button>
                        {sub.isArchived ? (
                          <button onClick={() => unarchiveSubMutation.mutate({ pid: cat.id, id: sub.id })} className="text-xs px-2 py-0.5 rounded bg-green-50 text-green-700 hover:bg-green-100">{t('admin.categories.unarchive')}</button>
                        ) : (
                          <button onClick={() => archiveSubMutation.mutate({ pid: cat.id, id: sub.id })} className="text-xs px-2 py-0.5 rounded bg-red-50 text-red-700 hover:bg-red-100">{t('admin.categories.archive')}</button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {(catModal || subModal) && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-surface-card rounded-2xl shadow-xl p-6 w-80">
            <h2 className="text-base font-semibold text-ink mb-3">
              {catModal ? (catModal.mode === 'add' ? t('admin.categories.add') : t('admin.categories.edit'))
                        : (subModal!.mode === 'add' ? t('admin.categories.addSub') : t('admin.categories.edit'))}
            </h2>
            <input
              type="text"
              placeholder="Name"
              value={formName}
              onChange={e => setFormName(e.target.value)}
              className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-2 focus:outline-none focus:ring-2 focus:ring-brand-300"
            />
            <input
              type="text"
              placeholder="Description (optional)"
              value={formDesc}
              onChange={e => setFormDesc(e.target.value)}
              className="w-full border border-surface-border rounded-lg px-3 py-2 text-sm mb-4 focus:outline-none focus:ring-2 focus:ring-brand-300"
            />
            <div className="flex gap-2 justify-end">
              <button onClick={() => { setCatModal(null); setSubModal(null) }} className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle">
                {t('common.cancel', 'Cancel')}
              </button>
              <button onClick={catModal ? submitCatModal : submitSubModal} className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600">
                {t('common.save', 'Save')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
