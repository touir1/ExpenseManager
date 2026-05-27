import { NavLink, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `block px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
    isActive
      ? 'bg-brand-100 text-brand-600'
      : 'text-ink-mute hover:text-ink hover:bg-surface-subtle'
  }`

export default function AdminLayout() {
  const { t } = useTranslation()

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 py-6 flex gap-6">
      <aside className="w-48 shrink-0">
        <nav className="flex flex-col gap-1">
          <NavLink to="/admin/users" className={linkClass}>{t('admin.nav.users')}</NavLink>
          <NavLink to="/admin/categories" className={linkClass}>{t('admin.nav.categories')}</NavLink>
          <NavLink to="/admin/currencies" className={linkClass}>{t('admin.nav.currencies')}</NavLink>
          <NavLink to="/admin/rates" className={linkClass}>{t('admin.nav.rates')}</NavLink>
          <NavLink to="/admin/rate-conflicts" className={linkClass}>{t('admin.nav.rateConflicts')}</NavLink>
        </nav>
      </aside>
      <main className="flex-1 min-w-0">
        <Outlet />
      </main>
    </div>
  )
}
