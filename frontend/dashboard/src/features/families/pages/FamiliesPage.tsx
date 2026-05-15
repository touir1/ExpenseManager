import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useFamilies } from '@/features/families/FamilyContext'
import {
  createFamily,
  renameFamily,
  archiveFamily,
  unarchiveFamily,
  inviteMember,
  removeMember,
  changeMemberRole,
  getFamilyById,
} from '@/features/families/services/familyApi.service'
import {
  makeCreateFamilySchema,
  makeRenameFamilySchema,
  makeInviteMemberSchema,
  type CreateFamilyData,
  type InviteMemberData,
} from '@/features/families/family.schemas'
import type { Family, FamilyDetail, FamilyMember } from '@/features/families/types/family.type'
import { useToast } from '@/components/Toast'
import FieldError from '@/components/FieldError'
import SubmitButton from '@/components/SubmitButton'
import { usePageTitle } from '@/hooks/usePageTitle'

// ── Helpers ────────────────────────────────────────────────────────────────

function RoleBadge({ role }: Readonly<{ role: string }>) {
  const isHead = role === 'Head'
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
        isHead
          ? 'bg-brand-50 text-brand-700'
          : 'bg-slate-100 text-slate-600'
      }`}
    >
      {role}
    </span>
  )
}

function Modal({
  title,
  onClose,
  children,
}: Readonly<{
  title: string
  onClose: () => void
  children: React.ReactNode
}>) {
  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      onClick={onClose}
      onKeyDown={(e) => { if (e.key === 'Escape') onClose() }}
      role="presentation"
    >
      <div
        className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-md mx-4 p-6"
        onClick={e => e.stopPropagation()}
        onKeyDown={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-slate-900">{title}</h2>
          <button
            onClick={onClose}
            className="p-1 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors cursor-pointer"
            aria-label="Close"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        {children}
      </div>
    </div>
  )
}

// ── Create Family Modal ────────────────────────────────────────────────────

function CreateFamilyModal({ onClose, onCreated }: Readonly<{ onClose: () => void; onCreated: () => void }>) {
  const { t } = useTranslation()
  const { show } = useToast()
  const schema = useMemo(() => makeCreateFamilySchema(t), [t])
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<CreateFamilyData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (data: CreateFamilyData) => {
    const res = await createFamily(data.name)
    if (res.ok) {
      show(t('families.createSuccess'), 'success')
      onCreated()
      onClose()
    }
  }

  return (
    <Modal title={t('families.createTitle')} onClose={onClose}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div>
          <label htmlFor="family-name" className="block text-sm font-medium text-slate-700 mb-1">
            {t('families.nameLabel')}
          </label>
          <input
            id="family-name"
            type="text"
            autoFocus
            className="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-brand-500 transition"
            placeholder={t('families.namePlaceholder')}
            aria-describedby={errors.name ? 'family-name-error' : undefined}
            {...register('name')}
          />
          <FieldError id="family-name-error" message={errors.name?.message} />
        </div>
        <SubmitButton isSubmitting={isSubmitting} label={t('families.createSubmit')} submittingLabel={t('families.createSubmitting')} />
      </form>
    </Modal>
  )
}

// ── Rename Family Modal ────────────────────────────────────────────────────

function RenameFamilyModal({
  family,
  onClose,
  onRenamed,
}: Readonly<{
  family: Family
  onClose: () => void
  onRenamed: () => void
}>) {
  const { t } = useTranslation()
  const { show } = useToast()
  const schema = useMemo(() => makeRenameFamilySchema(t), [t])
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<CreateFamilyData>({
    resolver: zodResolver(schema),
    defaultValues: { name: family.name },
  })

  const onSubmit = async (data: CreateFamilyData) => {
    const res = await renameFamily(family.id, data.name)
    if (res.ok) {
      show(t('families.renameSuccess'), 'success')
      onRenamed()
      onClose()
    }
  }

  return (
    <Modal title={t('families.renameTitle')} onClose={onClose}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div>
          <label htmlFor="rename-family" className="block text-sm font-medium text-slate-700 mb-1">
            {t('families.nameLabel')}
          </label>
          <input
            id="rename-family"
            type="text"
            autoFocus
            className="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-brand-500 transition"
            aria-describedby={errors.name ? 'rename-family-error' : undefined}
            {...register('name')}
          />
          <FieldError id="rename-family-error" message={errors.name?.message} />
        </div>
        <SubmitButton isSubmitting={isSubmitting} label={t('families.renameSubmit')} submittingLabel={t('families.renamingSubmitting')} />
      </form>
    </Modal>
  )
}

// ── Invite Member Modal ────────────────────────────────────────────────────

function InviteMemberModal({
  family,
  onClose,
}: Readonly<{
  family: Family
  onClose: () => void
}>) {
  const { t } = useTranslation()
  const { show } = useToast()
  const schema = useMemo(() => makeInviteMemberSchema(t), [t])
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<InviteMemberData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (data: InviteMemberData) => {
    const res = await inviteMember(family.id, data.email)
    if (res.ok) {
      show(t('families.inviteSuccess'), 'success')
      reset()
      onClose()
    }
  }

  return (
    <Modal title={t('families.inviteTitle', { name: family.name })} onClose={onClose}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div>
          <label htmlFor="invite-email" className="block text-sm font-medium text-slate-700 mb-1">
            {t('auth.email.label')}
          </label>
          <input
            id="invite-email"
            type="email"
            autoFocus
            className="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-brand-500 transition"
            placeholder={t('auth.email.placeholder')}
            aria-describedby={errors.email ? 'invite-email-error' : undefined}
            {...register('email')}
          />
          <FieldError id="invite-email-error" message={errors.email?.message} />
        </div>
        <p className="text-xs text-slate-500">{t('families.inviteHint')}</p>
        <SubmitButton isSubmitting={isSubmitting} label={t('families.inviteSubmit')} submittingLabel={t('families.inviteSubmitting')} />
      </form>
    </Modal>
  )
}

// ── Family Detail Panel ────────────────────────────────────────────────────

function FamilyDetailPanel({
  family,
  detail,
  onRefresh,
}: Readonly<{
  family: Family
  detail: FamilyDetail
  onRefresh: () => void
}>) {
  const { t } = useTranslation()
  const { show } = useToast()
  const [showInvite, setShowInvite] = useState(false)
  const isHead = family.userRole === 'Head'

  const handleRemove = async (member: FamilyMember) => {
    const res = await removeMember(family.id, member.userId)
    if (res.ok) {
      show(t('families.memberRemoved', { name: `${member.firstName} ${member.lastName}` }), 'success')
      onRefresh()
    }
  }

  const handleToggleRole = async (member: FamilyMember) => {
    const newRole = member.role === 'Head' ? 'Member' : 'Head'
    const res = await changeMemberRole(family.id, member.userId, newRole)
    if (res.ok) {
      show(t('families.roleChanged'), 'success')
      onRefresh()
    }
  }

  return (
    <div className="mt-4 pt-4 border-t border-slate-100">
      <div className="flex items-center justify-between mb-3">
        <span className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
          {t('families.members', { count: detail.members.length })}
        </span>
        {isHead && (
          <button
            onClick={() => setShowInvite(true)}
            className="text-xs font-medium text-brand-600 hover:text-brand-700 transition-colors cursor-pointer"
          >
            + {t('families.inviteAction')}
          </button>
        )}
      </div>

      <ul className="space-y-2">
        {detail.members.map(member => (
          <li key={member.userId} className="flex items-center justify-between gap-2">
            <div className="min-w-0">
              <p className="text-sm font-medium text-slate-800 truncate">
                {member.firstName} {member.lastName}
              </p>
              <p className="text-xs text-slate-400 truncate">{member.email}</p>
            </div>
            <div className="flex items-center gap-2 shrink-0">
              <RoleBadge role={member.role} />
              {isHead && (
                <>
                  <button
                    onClick={() => handleToggleRole(member)}
                    className="text-xs text-slate-500 hover:text-slate-700 transition-colors cursor-pointer"
                    title={t('families.changeRole')}
                  >
                    <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
                    </svg>
                  </button>
                  <button
                    onClick={() => handleRemove(member)}
                    className="text-xs text-red-400 hover:text-red-600 transition-colors cursor-pointer"
                    title={t('families.removeMember')}
                  >
                    <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </>
              )}
            </div>
          </li>
        ))}
      </ul>

      {showInvite && (
        <InviteMemberModal
          family={family}
          onClose={() => setShowInvite(false)}
        />
      )}
    </div>
  )
}

// ── Family Card ────────────────────────────────────────────────────────────

function FamilyCard({
  family,
  onRefresh,
}: Readonly<{
  family: Family
  onRefresh: () => void
}>) {
  const { t } = useTranslation()
  const { show } = useToast()
  const [expanded, setExpanded] = useState(false)
  const [detail, setDetail] = useState<FamilyDetail | null>(null)
  const [loadingDetail, setLoadingDetail] = useState(false)
  const [showRename, setShowRename] = useState(false)
  const isHead = family.userRole === 'Head'

  const loadDetail = useCallback(async () => {
    setLoadingDetail(true)
    const res = await getFamilyById(family.id)
    if (res.ok && res.data) setDetail(res.data)
    setLoadingDetail(false)
  }, [family.id])

  const handleExpand = () => {
    if (!expanded && !detail) loadDetail()
    setExpanded(v => !v)
  }

  const handleDetailRefresh = () => loadDetail()

  const handleArchive = async () => {
    const res = await archiveFamily(family.id)
    if (res.ok) {
      show(t('families.archiveSuccess'), 'success')
      onRefresh()
    }
  }

  const handleUnarchive = async () => {
    const res = await unarchiveFamily(family.id)
    if (res.ok) {
      show(t('families.unarchiveSuccess'), 'success')
      onRefresh()
    }
  }

  const expandedContent = loadingDetail ? (
    <div className="mt-4 pt-4 border-t border-slate-100 animate-pulse space-y-2">
      {[0, 1].map(i => (
        <div key={i} className="flex items-center gap-3">
          <div className="h-3 bg-slate-100 rounded w-32" />
          <div className="h-3 bg-slate-100 rounded w-20" />
        </div>
      ))}
    </div>
  ) : detail ? (
    <FamilyDetailPanel
      family={family}
      detail={detail}
      onRefresh={handleDetailRefresh}
    />
  ) : null

  return (
    <div className="bg-white rounded-2xl border border-slate-200 shadow-card p-5">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <h3 className="text-sm font-semibold text-slate-900 truncate">{family.name}</h3>
            {family.isDefault && (
              <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-emerald-50 text-emerald-700">
                {t('families.default')}
              </span>
            )}
            {family.isArchived && (
              <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-slate-100 text-slate-500">
                {t('families.archived')}
              </span>
            )}
            <RoleBadge role={family.userRole} />
          </div>
          <p className="text-xs text-slate-400 mt-0.5">
            {t('families.createdAt', { date: new Date(family.createdAt).toLocaleDateString() })}
          </p>
        </div>

        <div className="flex items-center gap-1 shrink-0">
          {isHead && !family.isDefault && !family.isArchived && (
            <button
              onClick={() => setShowRename(true)}
              className="p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors cursor-pointer"
              title={t('families.renameAction')}
            >
              <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
              </svg>
            </button>
          )}
          {isHead && !family.isDefault && (
            family.isArchived ? (
              <button
                onClick={handleUnarchive}
                className="p-1.5 rounded-lg text-slate-400 hover:text-emerald-600 hover:bg-emerald-50 transition-colors cursor-pointer"
                title={t('families.unarchiveAction')}
              >
                <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4" />
                </svg>
              </button>
            ) : (
              <button
                onClick={handleArchive}
                className="p-1.5 rounded-lg text-slate-400 hover:text-amber-600 hover:bg-amber-50 transition-colors cursor-pointer"
                title={t('families.archiveAction')}
              >
                <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4" />
                </svg>
              </button>
            )
          )}
          <button
            onClick={handleExpand}
            className="p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors cursor-pointer"
            aria-expanded={expanded}
            title={expanded ? t('families.collapse') : t('families.expand')}
          >
            <svg
              className={`h-3.5 w-3.5 transition-transform duration-200 ${expanded ? 'rotate-180' : ''}`}
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
            </svg>
          </button>
        </div>
      </div>

      {expanded && expandedContent}

      {showRename && (
        <RenameFamilyModal
          family={family}
          onClose={() => setShowRename(false)}
          onRenamed={onRefresh}
        />
      )}
    </div>
  )
}

// ── Families Page ──────────────────────────────────────────────────────────

type Tab = 'active' | 'archived'

export default function FamiliesPage() {
  const { t } = useTranslation()
  usePageTitle(t('families.pageTitle'))
  const { families, isLoading, refresh } = useFamilies()
  const [tab, setTab] = useState<Tab>('active')
  const [showCreate, setShowCreate] = useState(false)

  useEffect(() => {
    refresh()
  }, [refresh])

  const activeFamilies = useMemo(
    () => families.filter(f => !f.isArchived),
    [families]
  )
  const archivedFamilies = useMemo(
    () => families.filter(f => f.isArchived),
    [families]
  )

  const displayed = tab === 'active' ? activeFamilies : archivedFamilies

  const listContent = isLoading ? (
    <div className="grid gap-4 sm:grid-cols-2">
      {[0, 1, 2].map(i => (
        <div key={i} className="bg-white rounded-2xl border border-slate-200 shadow-card p-5 animate-pulse">
          <div className="h-4 bg-slate-200 rounded w-32 mb-2" />
          <div className="h-3 bg-slate-100 rounded w-20" />
        </div>
      ))}
    </div>
  ) : displayed.length === 0 ? (
    <div className="text-center py-16">
      <p className="text-sm text-slate-400">
        {tab === 'active' ? t('families.emptyActive') : t('families.emptyArchived')}
      </p>
    </div>
  ) : (
    <div className="grid gap-4 sm:grid-cols-2">
      {displayed.map(family => (
        <FamilyCard key={family.id} family={family} onRefresh={refresh} />
      ))}
    </div>
  )

  const tabClass = (t: Tab) =>
    `px-3 py-1.5 text-sm font-medium rounded-lg transition-colors duration-150 cursor-pointer ${
      tab === t
        ? 'bg-brand-50 text-brand-700'
        : 'text-slate-500 hover:text-slate-700 hover:bg-slate-100'
    }`

  return (
    <div className="max-w-5xl mx-auto w-full px-4 sm:px-6 py-8">
      <div className="mb-6 flex items-start justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">{t('families.title')}</h1>
          <p className="text-sm text-slate-500 mt-1">{t('families.subtitle')}</p>
        </div>
        <button
          onClick={() => setShowCreate(true)}
          className="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-brand-600 hover:bg-brand-700 text-white text-sm font-medium transition-colors duration-150 cursor-pointer"
        >
          <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
          </svg>
          {t('families.createAction')}
        </button>
      </div>

      {/* Tabs */}
      <div className="flex items-center gap-1 mb-4">
        <button className={tabClass('active')} onClick={() => setTab('active')}>
          {t('families.tabActive')}
          <span className="ml-1.5 text-xs text-slate-400">({activeFamilies.length})</span>
        </button>
        <button className={tabClass('archived')} onClick={() => setTab('archived')}>
          {t('families.tabArchived')}
          <span className="ml-1.5 text-xs text-slate-400">({archivedFamilies.length})</span>
        </button>
      </div>

      {/* List */}
      {listContent}

      {showCreate && (
        <CreateFamilyModal
          onClose={() => setShowCreate(false)}
          onCreated={refresh}
        />
      )}
    </div>
  )
}
