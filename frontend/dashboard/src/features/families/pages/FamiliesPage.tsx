import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useAuth } from '@/features/auth/AuthContext'
import { useFamilies } from '@/features/families/FamilyContext'
import {
  createFamily,
  renameFamily,
  archiveFamily,
  unarchiveFamily,
  inviteMember,
  removeMember,
  changeMemberRole,
  leaveFamily,
  getFamilyById,
  getPendingInvitations,
  revokeInvitation,
} from '@/features/families/services/familyApi.service'
import {
  makeCreateFamilySchema,
  makeRenameFamilySchema,
  makeInviteMemberSchema,
  type CreateFamilyData,
  type InviteMemberData,
} from '@/features/families/family.schemas'
import type { Family, FamilyDetail, FamilyMember, FamilyPendingInvitation } from '@/features/families/types/family.type'
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
          ? 'bg-brand-50 text-brand-700 dark:bg-brand-900/40 dark:text-brand-300'
          : 'bg-surface-subtle text-ink-mute'
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
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const handleMouseDown = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) onClose()
    }
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('mousedown', handleMouseDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('mousedown', handleMouseDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [onClose])

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div
        ref={ref}
        className="bg-surface-card rounded-2xl shadow-xl border border-surface-border w-full max-w-md mx-4 p-6"
      >
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-ink">{title}</h2>
          <button
            onClick={onClose}
            className="p-1 rounded-lg text-ink-faint hover:text-ink-body hover:bg-surface-subtle transition-colors cursor-pointer"
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

// ── Confirm Archive Modal ──────────────────────────────────────────────────

function ConfirmArchiveModal({
  family,
  onClose,
  onConfirm,
}: Readonly<{ family: Family; onClose: () => void; onConfirm: () => Promise<void> }>) {
  const { t } = useTranslation()
  const [submitting, setSubmitting] = useState(false)

  const handleConfirm = async () => {
    setSubmitting(true)
    await onConfirm()
    setSubmitting(false)
    onClose()
  }

  return (
    <Modal title={t('families.archiveConfirmTitle', { name: family.name })} onClose={onClose}>
      <p className="text-sm text-ink-mute mb-5">{t('families.archiveConfirmMessage')}</p>
      <div className="flex justify-end gap-2">
        <button
          onClick={onClose}
          className="px-3.5 py-2 rounded-xl border border-surface-border text-sm font-medium text-ink-body hover:bg-surface-subtle transition-colors cursor-pointer"
        >
          {t('families.archiveConfirmCancel')}
        </button>
        <button
          onClick={handleConfirm}
          disabled={submitting}
          className="btn-primary mt-0"
        >
          {submitting ? t('families.archiveConfirmSubmitting') : t('families.archiveConfirmSubmit')}
        </button>
      </div>
    </Modal>
  )
}

// ── Confirm Revoke Modal ───────────────────────────────────────────────────

function ConfirmRevokeModal({
  invitation,
  onClose,
  onConfirm,
}: Readonly<{ invitation: FamilyPendingInvitation; onClose: () => void; onConfirm: () => Promise<void> }>) {
  const { t } = useTranslation()
  const [submitting, setSubmitting] = useState(false)

  const handleConfirm = async () => {
    setSubmitting(true)
    await onConfirm()
    setSubmitting(false)
  }

  return (
    <Modal title={t('families.revokeConfirmTitle')} onClose={onClose}>
      <p className="text-sm text-ink-mute mb-5">
        {t('families.revokeConfirmMessage', { email: invitation.inviteeEmail })}
      </p>
      <div className="flex justify-end gap-2">
        <button
          onClick={onClose}
          className="px-3.5 py-2 rounded-xl border border-surface-border text-sm font-medium text-ink-body hover:bg-surface-subtle transition-colors cursor-pointer"
        >
          {t('families.archiveConfirmCancel')}
        </button>
        <button
          onClick={handleConfirm}
          disabled={submitting}
          className="btn-primary mt-0"
        >
          {submitting ? t('families.revokeConfirmSubmitting') : t('families.revokeConfirmSubmit')}
        </button>
      </div>
    </Modal>
  )
}

// ── Create Family Modal ────────────────────────────────────────────────────

function CreateFamilyModal({ onClose, onCreated }: Readonly<{ onClose: () => void; onCreated: () => void }>) {
  const { t } = useTranslation()
  const { show } = useToast()
  const schema = useMemo(() => makeCreateFamilySchema(t), [t])
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<CreateFamilyData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (data: CreateFamilyData) => {
    const res = await createFamily(data.name)
    if (res.ok) {
      show(t('families.createSuccess'), 'success')
      onCreated()
      onClose()
    } else if (res.status === 409) {
      setError('name', { message: t('validation.familyNameTaken') })
    }
  }

  return (
    <Modal title={t('families.createTitle')} onClose={onClose}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div>
          <label htmlFor="family-name" className="field-label">
            {t('families.nameLabel')}
          </label>
          <input
            id="family-name"
            type="text"
            autoFocus
            className="field-input"
            placeholder={t('families.namePlaceholder')}
            aria-describedby={errors.name ? 'family-name-error' : undefined}
            {...register('name')}
          />
          <FieldError id="family-name-error" message={errors.name?.message} />
        </div>
        <SubmitButton isSubmitting={isSubmitting} label={t('families.createSubmit')} loadingLabel={t('families.createSubmitting')} />
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
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<CreateFamilyData>({
    resolver: zodResolver(schema),
    defaultValues: { name: family.name },
  })

  const onSubmit = async (data: CreateFamilyData) => {
    const res = await renameFamily(family.id, data.name)
    if (res.ok) {
      show(t('families.renameSuccess'), 'success')
      onRenamed()
      onClose()
    } else if (res.status === 409) {
      setError('name', { message: t('validation.familyNameTaken') })
    }
  }

  return (
    <Modal title={t('families.renameTitle')} onClose={onClose}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div>
          <label htmlFor="rename-family" className="field-label">
            {t('families.nameLabel')}
          </label>
          <input
            id="rename-family"
            type="text"
            autoFocus
            className="field-input"
            aria-describedby={errors.name ? 'rename-family-error' : undefined}
            {...register('name')}
          />
          <FieldError id="rename-family-error" message={errors.name?.message} />
        </div>
        <SubmitButton isSubmitting={isSubmitting} label={t('families.renameSubmit')} loadingLabel={t('families.renamingSubmitting')} />
      </form>
    </Modal>
  )
}

// ── Invite Member Modal ────────────────────────────────────────────────────

function InviteMemberModal({
  family,
  onClose,
  onSuccess,
}: Readonly<{
  family: Family
  onClose: () => void
  onSuccess?: () => void
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
      onSuccess?.()
    }
  }

  return (
    <Modal title={t('families.inviteTitle', { name: family.name })} onClose={onClose}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div>
          <label htmlFor="invite-email" className="field-label">
            {t('auth.email.label')}
          </label>
          <input
            id="invite-email"
            type="email"
            autoFocus
            className="field-input"
            placeholder={t('auth.email.placeholder')}
            aria-describedby={errors.email ? 'invite-email-error' : undefined}
            {...register('email')}
          />
          <FieldError id="invite-email-error" message={errors.email?.message} />
        </div>
        <p className="text-xs text-ink-mute">{t('families.inviteHint')}</p>
        <SubmitButton isSubmitting={isSubmitting} label={t('families.inviteSubmit')} loadingLabel={t('families.inviteSubmitting')} />
      </form>
    </Modal>
  )
}

// ── Family Detail Panel ────────────────────────────────────────────────────

function FamilyDetailPanel({
  family,
  detail,
  onRefresh,
  onLeave,
}: Readonly<{
  family: Family
  detail: FamilyDetail
  onRefresh: () => void
  onLeave: () => void
}>) {
  const { t } = useTranslation()
  const { show } = useToast()
  const { user } = useAuth()
  const [showInvite, setShowInvite] = useState(false)
  const [pendingInvitations, setPendingInvitations] = useState<FamilyPendingInvitation[]>([])
  const [loadingInvitations, setLoadingInvitations] = useState(false)
  const [revokeTarget, setRevokeTarget] = useState<FamilyPendingInvitation | null>(null)
  const isHead = family.userRole === 'Head'
  const headCount = detail.members.filter(m => m.role === 'Head').length
  const canLeave = !family.isDefault && !family.isArchived && (!isHead || headCount > 1)

  const loadInvitations = useCallback(async () => {
    setLoadingInvitations(true)
    const res = await getPendingInvitations(family.id)
    if (res.ok && res.data) setPendingInvitations(res.data)
    setLoadingInvitations(false)
  }, [family.id])

  useEffect(() => {
    if (isHead && !family.isDefault) loadInvitations()
  }, [isHead, family.isDefault, loadInvitations])

  const handleLeave = async () => {
    const res = await leaveFamily(family.id)
    if (res.ok) {
      show(t('families.leaveSuccess'), 'success')
      onLeave()
    }
  }

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

  const handleRevoke = async () => {
    if (!revokeTarget) return
    const res = await revokeInvitation(family.id, revokeTarget.token)
    if (res.ok) {
      show(t('families.revokeSuccess'), 'success')
      setRevokeTarget(null)
      loadInvitations()
    }
  }

  return (
    <div className="mt-4 pt-4 border-t border-surface-border">
      <div className="flex items-center justify-between mb-3">
        <span className="text-xs font-semibold text-ink-mute uppercase tracking-wide">
          {t('families.members', { count: detail.members.length })}
        </span>
        {isHead && !family.isDefault && (
          <button
            onClick={() => setShowInvite(true)}
            className="text-xs font-medium text-brand-600 hover:text-brand-700 transition-colors cursor-pointer"
          >
            + {t('families.inviteAction')}
          </button>
        )}
      </div>

      <ul className="space-y-2">
        {detail.members.map(member => {
          const isSelf = member.email === user?.email
          const canToggleRole = isHead && !isSelf && !family.isDefault
          const canRemove = isHead && !family.isDefault && !(isSelf && headCount <= 1)
          return (
            <li key={member.userId} className="flex items-center justify-between gap-2">
              <div className="min-w-0">
                <p className="text-sm font-medium text-ink truncate">
                  {member.firstName} {member.lastName}
                </p>
                <p className="text-xs text-ink-faint truncate">{member.email}</p>
              </div>
              <div className="flex items-center gap-2 shrink-0">
                <RoleBadge role={member.role} />
                {canToggleRole && (
                  <button
                    onClick={() => handleToggleRole(member)}
                    className="text-xs text-ink-mute hover:text-ink-body transition-colors cursor-pointer"
                    title={t('families.changeRole')}
                  >
                    <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
                    </svg>
                  </button>
                )}
                {canRemove && (
                  <button
                    onClick={() => handleRemove(member)}
                    className="text-xs text-red-400 hover:text-red-600 transition-colors cursor-pointer"
                    title={t('families.removeMember')}
                  >
                    <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                )}
              </div>
            </li>
          )
        })}
      </ul>

      {isHead && !family.isDefault && (
        <div className="mt-3 pt-3 border-t border-surface-border">
          <span className="text-xs font-semibold text-ink-mute uppercase tracking-wide">
            {t('families.pendingInvitations', { count: pendingInvitations.length })}
          </span>
          {loadingInvitations ? (
            <div className="mt-2 animate-pulse space-y-1.5">
              {[0, 1].map(i => <div key={i} className="h-3 bg-surface-subtle rounded w-48" />)}
            </div>
          ) : pendingInvitations.length === 0 ? (
            <p className="text-xs text-ink-faint mt-1">{t('families.pendingInvitationsEmpty')}</p>
          ) : (
            <ul className="mt-2 space-y-1.5">
              {pendingInvitations.map(inv => (
                <li key={inv.token} className="flex items-center justify-between gap-2">
                  <div className="min-w-0">
                    <p className="text-sm text-ink truncate">{inv.inviteeEmail}</p>
                    <p className="text-xs text-ink-faint">
                      {t('families.expiresAt', { date: new Date(inv.expiresAt).toLocaleDateString() })}
                    </p>
                  </div>
                  <button
                    onClick={() => setRevokeTarget(inv)}
                    className="text-xs font-medium text-red-500 hover:text-red-700 transition-colors cursor-pointer shrink-0"
                  >
                    {t('families.revokeAction')}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {canLeave && (
        <div className="mt-3 pt-3 border-t border-surface-border flex justify-end">
          <button
            onClick={handleLeave}
            className="text-xs font-medium text-red-500 hover:text-red-700 transition-colors cursor-pointer"
          >
            {t('families.leaveAction')}
          </button>
        </div>
      )}

      {showInvite && (
        <InviteMemberModal
          family={family}
          onClose={() => setShowInvite(false)}
          onSuccess={loadInvitations}
        />
      )}

      {revokeTarget && (
        <ConfirmRevokeModal
          invitation={revokeTarget}
          onClose={() => setRevokeTarget(null)}
          onConfirm={handleRevoke}
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
  const [showArchiveConfirm, setShowArchiveConfirm] = useState(false)
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

  const detailPanel = detail ? (
    <FamilyDetailPanel
      family={family}
      detail={detail}
      onRefresh={handleDetailRefresh}
      onLeave={onRefresh}
    />
  ) : null

  const expandedContent = loadingDetail ? (
    <div className="mt-4 pt-4 border-t border-surface-border animate-pulse space-y-2">
      {[0, 1].map(i => (
        <div key={i} className="flex items-center gap-3">
          <div className="h-3 bg-surface-subtle rounded w-32" />
          <div className="h-3 bg-surface-subtle rounded w-20" />
        </div>
      ))}
    </div>
  ) : detailPanel

  return (
    <div className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-5">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <h3 className="text-sm font-semibold text-ink truncate">{family.name}</h3>
            {family.isDefault && (
              <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">
                {t('families.default')}
              </span>
            )}
            {family.isArchived && (
              <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-surface-subtle text-ink-mute">
                {t('families.archived')}
              </span>
            )}
            <RoleBadge role={family.userRole} />
          </div>
          <p className="text-xs text-ink-faint mt-0.5">
            {t('families.createdAt', { date: new Date(family.createdAt).toLocaleDateString() })}
          </p>
        </div>

        <div className="flex items-center gap-1 shrink-0">
          {isHead && !family.isDefault && !family.isArchived && (
            <button
              onClick={() => setShowRename(true)}
              className="p-1.5 rounded-lg text-ink-faint hover:text-ink-body hover:bg-surface-subtle transition-colors cursor-pointer"
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
                className="p-1.5 rounded-lg text-ink-faint hover:text-emerald-600 hover:bg-emerald-50 dark:hover:text-emerald-400 dark:hover:bg-emerald-900/20 transition-colors cursor-pointer"
                title={t('families.unarchiveAction')}
              >
                <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4" />
                </svg>
              </button>
            ) : (
              <button
                onClick={() => setShowArchiveConfirm(true)}
                className="p-1.5 rounded-lg text-ink-faint hover:text-amber-600 hover:bg-amber-50 dark:hover:text-amber-400 dark:hover:bg-amber-900/20 transition-colors cursor-pointer"
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
            className="p-1.5 rounded-lg text-ink-faint hover:text-ink-body hover:bg-surface-subtle transition-colors cursor-pointer"
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

      {(expanded || detail !== null || loadingDetail) && (
        <div
          style={{
            maxHeight: expanded ? '600px' : '0px',
            overflow: 'hidden',
            transition: 'max-height 0.25s ease-in-out',
          }}
        >
          {expandedContent}
        </div>
      )}

      {showRename && (
        <RenameFamilyModal
          family={family}
          onClose={() => setShowRename(false)}
          onRenamed={onRefresh}
        />
      )}

      {showArchiveConfirm && (
        <ConfirmArchiveModal
          family={family}
          onClose={() => setShowArchiveConfirm(false)}
          onConfirm={handleArchive}
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
    () => families.filter(f => !f.isArchived && !f.isDefault),
    [families]
  )
  const archivedFamilies = useMemo(
    () => families.filter(f => f.isArchived),
    [families]
  )

  const displayed = tab === 'active' ? activeFamilies : archivedFamilies

  const familiesList = displayed.length === 0 ? (
    <div className="text-center py-16">
      <p className="text-sm text-ink-faint">
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

  const listContent = isLoading ? (
    <div className="grid gap-4 sm:grid-cols-2">
      {[0, 1, 2].map(i => (
        <div key={i} className="bg-surface-card rounded-2xl border border-surface-border shadow-card p-5 animate-pulse">
          <div className="h-4 bg-surface-border rounded w-32 mb-2" />
          <div className="h-3 bg-surface-subtle rounded w-20" />
        </div>
      ))}
    </div>
  ) : familiesList

  const tabClass = (t: Tab) =>
    `px-3 py-1.5 text-sm font-medium rounded-lg transition-colors duration-150 cursor-pointer ${
      tab === t
        ? 'bg-brand-50 text-brand-700 dark:bg-brand-900/40 dark:text-brand-300'
        : 'text-ink-mute hover:text-ink hover:bg-surface-subtle'
    }`

  return (
    <div className="max-w-5xl mx-auto w-full px-4 sm:px-6 py-8">
      <div className="mb-6 flex items-start justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold text-ink tracking-tight">{t('families.title')}</h1>
          <p className="text-sm text-ink-mute mt-1">{t('families.subtitle')}</p>
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
          <span className="ml-1.5 text-xs text-ink-faint">({activeFamilies.length})</span>
        </button>
        <button className={tabClass('archived')} onClick={() => setTab('archived')}>
          {t('families.tabArchived')}
          <span className="ml-1.5 text-xs text-ink-faint">({archivedFamilies.length})</span>
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
