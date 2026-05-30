import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { usePageTitle } from '@/hooks/usePageTitle'
import { useAuth } from '@/features/auth/AuthContext'
import {
  getUsers,
  getRoles,
  disableUser,
  enableUser,
  setUserRoles,
  type AdminUser,
  type AdminRole,
} from '@/features/admin/services/adminUsersApi.service'

export default function AdminUsersPage() {
  const { t } = useTranslation()
  usePageTitle(t('admin.users.pageTitle'))
  const qc = useQueryClient()
  const { user: currentUser } = useAuth()

  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [rolesModal, setRolesModal] = useState<AdminUser | null>(null)
  const [selectedRoleIds, setSelectedRoleIds] = useState<number[]>([])

  const { data, isLoading } = useQuery({
    queryKey: ['admin-users', search, page],
    queryFn: () => getUsers(search || undefined, page),
    select: r => r.data,
  })

  const { data: allRoles = [] } = useQuery({
    queryKey: ['admin-roles'],
    queryFn: () => getRoles(),
    select: r => r.data ?? [],
  })

  const disableMutation = useMutation({
    mutationFn: (id: number) => disableUser(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-users'] }),
  })

  const enableMutation = useMutation({
    mutationFn: (id: number) => enableUser(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-users'] }),
  })

  const rolesMutation = useMutation({
    mutationFn: ({ id, roleIds }: { id: number; roleIds: number[] }) => setUserRoles(id, roleIds),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-users'] })
      setRolesModal(null)
    },
  })

  const openRolesModal = (user: AdminUser) => {
    setRolesModal(user)
    setSelectedRoleIds(user.roles.map(r => r.id))
  }

  const total = data?.total ?? 0
  const pageSize = data?.pageSize ?? 20
  const totalPages = Math.ceil(total / pageSize)

  return (
    <div>
      <h1 className="text-xl font-bold text-ink mb-4">{t('admin.users.pageTitle')}</h1>

      <div className="mb-4">
        <input
          type="text"
          placeholder={t('admin.users.search')}
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1) }}
          className="border border-surface-border rounded-lg px-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-brand-300"
        />
      </div>

      {isLoading ? (
        <p className="text-ink-mute text-sm">{t('common.loading', 'Loading…')}</p>
      ) : (
        <div className="bg-white shadow-card border border-slate-200 rounded-2xl overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-surface-subtle">
              <tr>
                <th className="text-left px-4 py-2 text-ink-mute font-medium">Email</th>
                <th className="text-left px-4 py-2 text-ink-mute font-medium">Name</th>
                <th className="text-left px-4 py-2 text-ink-mute font-medium">Status</th>
                <th className="text-left px-4 py-2 text-ink-mute font-medium">Roles</th>
                <th className="text-left px-4 py-2 text-ink-mute font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {(data?.users ?? []).map(user => {
                const isSelf = user.email === currentUser?.email
                return (
                  <tr key={user.id} className="border-t border-surface-border">
                    <td className="px-4 py-2">{user.email}</td>
                    <td className="px-4 py-2">{user.firstName} {user.lastName}</td>
                    <td className="px-4 py-2">
                      {user.isDisabled ? (
                        <span className="text-xs px-2 py-0.5 rounded-full bg-red-100 text-red-600">{t('admin.users.disabled', 'Disabled')}</span>
                      ) : (
                        <span className="text-xs px-2 py-0.5 rounded-full bg-green-100 text-green-600">{t('admin.users.active', 'Active')}</span>
                      )}
                    </td>
                    <td className="px-4 py-2">
                      {user.roles.map(r => r.code).join(', ') || '—'}
                    </td>
                    <td className="px-4 py-2 flex gap-2">
                      {user.isDisabled ? (
                        <button
                          onClick={() => enableMutation.mutate(user.id)}
                          disabled={isSelf}
                          className={`text-xs px-2 py-1 rounded bg-green-50 text-green-700 transition-colors ${isSelf ? 'opacity-40 cursor-not-allowed' : 'hover:bg-green-100'}`}
                        >
                          {t('admin.users.enable')}
                        </button>
                      ) : (
                        <button
                          onClick={() => disableMutation.mutate(user.id)}
                          disabled={isSelf}
                          className={`text-xs px-2 py-1 rounded bg-red-50 text-red-700 transition-colors ${isSelf ? 'opacity-40 cursor-not-allowed' : 'hover:bg-red-100'}`}
                        >
                          {t('admin.users.disable')}
                        </button>
                      )}
                      <button
                        onClick={() => openRolesModal(user)}
                        className="text-xs px-2 py-1 rounded bg-brand-50 text-brand-600 hover:bg-brand-100 transition-colors"
                      >
                        {t('admin.users.manageRoles')}
                      </button>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex gap-2 mt-4 justify-end">
          <button
            disabled={page <= 1}
            onClick={() => setPage(p => p - 1)}
            className="text-sm px-3 py-1 rounded border border-surface-border disabled:opacity-40"
          >
            ←
          </button>
          <span className="text-sm text-ink-mute py-1">{page} / {totalPages}</span>
          <button
            disabled={page >= totalPages}
            onClick={() => setPage(p => p + 1)}
            className="text-sm px-3 py-1 rounded border border-surface-border disabled:opacity-40"
          >
            →
          </button>
        </div>
      )}

      {rolesModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-xl p-6 w-80">
            <h2 className="text-base font-semibold text-ink mb-3">
              {t('admin.users.manageRoles')} — {rolesModal.email}
            </h2>
            <div className="flex flex-col gap-2 mb-4">
              {(allRoles as AdminRole[]).map(role => {
                const isSelfAdmin = role.code === 'APP_ADMIN' && rolesModal.email === currentUser?.email
                return (
                  <label
                    key={role.id}
                    className={`flex items-center gap-2 text-sm ${isSelfAdmin ? 'cursor-not-allowed opacity-60' : 'cursor-pointer'}`}
                    title={isSelfAdmin ? t('admin.users.cannotRemoveOwnAdmin') : undefined}
                  >
                    <input
                      type="checkbox"
                      checked={selectedRoleIds.includes(role.id)}
                      disabled={isSelfAdmin}
                      onChange={e => {
                        if (isSelfAdmin && !e.target.checked) return
                        setSelectedRoleIds(prev =>
                          e.target.checked ? [...prev, role.id] : prev.filter(id => id !== role.id)
                        )
                      }}
                    />
                    {role.name} <span className="text-ink-mute text-xs">({role.code})</span>
                  </label>
                )
              })}
            </div>
            <div className="flex gap-2 justify-end">
              <button
                onClick={() => setRolesModal(null)}
                className="text-sm px-3 py-1.5 rounded-lg border border-surface-border text-ink-mute hover:bg-surface-subtle"
              >
                {t('common.cancel', 'Cancel')}
              </button>
              <button
                onClick={() => rolesMutation.mutate({ id: rolesModal.id, roleIds: selectedRoleIds })}
                disabled={rolesMutation.isPending}
                className="text-sm px-3 py-1.5 rounded-lg bg-brand-500 text-white hover:bg-brand-600 disabled:opacity-60"
              >
                {t('common.save', 'Save')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
