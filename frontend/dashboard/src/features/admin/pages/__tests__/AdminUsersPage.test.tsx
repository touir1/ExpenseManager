import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import AdminUsersPage from '../AdminUsersPage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))
vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: vi.fn() }))

const mockGetUsers = vi.fn()
const mockGetRoles = vi.fn()
const mockDisableUser = vi.fn()
const mockEnableUser = vi.fn()
const mockSetUserRoles = vi.fn()

vi.mock('@/features/admin/services/adminUsersApi.service', () => ({
  getUsers: (...a: unknown[]) => mockGetUsers(...a),
  getRoles: (...a: unknown[]) => mockGetRoles(...a),
  disableUser: (...a: unknown[]) => mockDisableUser(...a),
  enableUser: (...a: unknown[]) => mockEnableUser(...a),
  setUserRoles: (...a: unknown[]) => mockSetUserRoles(...a),
}))

const user1 = { id: 1, email: 'admin@test.com', firstName: 'Admin', lastName: 'User', isDisabled: false, isDeleted: false, isEmailValidated: true, createdAt: '2024-01-01', roles: [] }
const user2 = { id: 2, email: 'disabled@test.com', firstName: 'Dis', lastName: 'Abled', isDisabled: true, isDeleted: false, isEmailValidated: true, createdAt: '2024-01-01', roles: [] }

function renderPage() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <AdminUsersPage />
    </QueryClientProvider>
  )
}

describe('AdminUsersPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetUsers.mockResolvedValue({ ok: true, data: { users: [user1, user2], total: 2, page: 1, pageSize: 20 } })
    mockGetRoles.mockResolvedValue({ ok: true, data: [] })
    mockDisableUser.mockResolvedValue({ ok: true })
    mockEnableUser.mockResolvedValue({ ok: true })
    mockSetUserRoles.mockResolvedValue({ ok: true })
  })

  it('renders user table after loading', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByText('admin@test.com')).toBeInTheDocument())
    expect(screen.getByText('disabled@test.com')).toBeInTheDocument()
  })

  it('renders search input', () => {
    renderPage()
    expect(screen.getByPlaceholderText('admin.users.search')).toBeInTheDocument()
  })

  it('calls disableUser when disable button clicked', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('admin@test.com'))
    const disableBtns = screen.getAllByText('admin.users.disable')
    await user.click(disableBtns[0])
    expect(mockDisableUser).toHaveBeenCalledWith(1)
  })

  it('calls enableUser when enable button clicked', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('disabled@test.com'))
    const enableBtn = screen.getByText('admin.users.enable')
    await user.click(enableBtn)
    expect(mockEnableUser).toHaveBeenCalledWith(2)
  })

  it('opens roles modal when manage roles clicked', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('admin@test.com'))
    const roleBtns = screen.getAllByText('admin.users.manageRoles')
    await user.click(roleBtns[0])
    expect(screen.getByText('common.save')).toBeInTheDocument()
  })
})
