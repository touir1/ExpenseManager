import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import FamiliesPage from '../FamiliesPage'
import type { Family, FamilyDetail } from '../../types/family.type'

// ── Mocks ────────────────────────────────────────────────────────────────────

const mockShow = vi.fn()
vi.mock('@/components/Toast', () => ({
  useToast: () => ({ show: mockShow }),
}))

vi.mock('@/components/FieldError', () => ({
  default: ({ message }: { message?: string }) => message ? <span role="alert">{message}</span> : null,
}))

vi.mock('@/components/SubmitButton', () => ({
  default: ({ isSubmitting, label }: { isSubmitting: boolean; label: string }) => (
    <button type="submit" disabled={isSubmitting}>{label}</button>
  ),
}))

const mockUseFamilies = vi.fn()
vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => mockUseFamilies(),
}))

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))

vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: vi.fn() }))

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: () => ({ user: { email: 'current@example.com' } }),
}))

vi.mock('@/features/families/services/familyApi.service', () => ({
  createFamily: vi.fn(),
  renameFamily: vi.fn(),
  archiveFamily: vi.fn(),
  unarchiveFamily: vi.fn(),
  inviteMember: vi.fn(),
  removeMember: vi.fn(),
  changeMemberRole: vi.fn(),
  getFamilyById: vi.fn(),
}))

import * as familyApi from '@/features/families/services/familyApi.service'

// ── Fixtures ─────────────────────────────────────────────────────────────────

const mockFamily: Family = {
  id: 1,
  name: 'Smith household',
  isDefault: true,
  isArchived: false,
  userRole: 'Head',
  createdAt: '2024-01-01T00:00:00Z',
}

const mockFamily2: Family = {
  id: 2,
  name: 'Holiday',
  isDefault: false,
  isArchived: false,
  userRole: 'Head',
  createdAt: '2024-02-01T00:00:00Z',
}

const mockArchivedFamily: Family = {
  id: 3,
  name: 'Old group',
  isDefault: false,
  isArchived: true,
  userRole: 'Head',
  createdAt: '2023-01-01T00:00:00Z',
}

const mockDetail: FamilyDetail = {
  ...mockFamily2,
  members: [
    {
      userId: 10,
      firstName: 'Alice',
      lastName: 'Smith',
      email: 'alice@example.com',
      role: 'Head',
      joinedAt: '2024-01-01T00:00:00Z',
    },
    {
      userId: 11,
      firstName: 'Bob',
      lastName: 'Jones',
      email: 'bob@example.com',
      role: 'Member',
      joinedAt: '2024-02-01T00:00:00Z',
    },
  ],
}

function makeCtx(overrides: Partial<ReturnType<typeof mockUseFamilies>> = {}) {
  mockUseFamilies.mockReturnValue({
    families: [mockFamily],
    isLoading: false,
    refresh: vi.fn(),
    ...overrides,
  })
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('FamiliesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  // ── Layout ─────────────────────────────────────────────────────────────────

  describe('layout', () => {
    it('renders title and subtitle', () => {
      makeCtx()
      render(<FamiliesPage />)
      expect(screen.getByText('families.title')).toBeInTheDocument()
      expect(screen.getByText('families.subtitle')).toBeInTheDocument()
    })

    it('renders create button', () => {
      makeCtx()
      render(<FamiliesPage />)
      expect(screen.getByRole('button', { name: /families\.createAction/i })).toBeInTheDocument()
    })

    it('renders active and archived tab buttons', () => {
      makeCtx()
      render(<FamiliesPage />)
      expect(screen.getByRole('button', { name: /tabActive/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /tabArchived/i })).toBeInTheDocument()
    })

    it('calls refresh on mount', () => {
      const refresh = vi.fn()
      makeCtx({ refresh })
      render(<FamiliesPage />)
      expect(refresh).toHaveBeenCalledOnce()
    })
  })

  // ── Loading ────────────────────────────────────────────────────────────────

  describe('loading state', () => {
    it('shows skeleton cards when isLoading is true', () => {
      makeCtx({ isLoading: true, families: [] })
      render(<FamiliesPage />)
      const skeletons = document.querySelectorAll('.animate-pulse')
      expect(skeletons.length).toBeGreaterThan(0)
    })

    it('does not show family cards while loading', () => {
      makeCtx({ isLoading: true, families: [mockFamily] })
      render(<FamiliesPage />)
      expect(screen.queryByText('Smith household')).not.toBeInTheDocument()
    })
  })

  // ── Empty states ───────────────────────────────────────────────────────────

  describe('empty states', () => {
    it('shows emptyActive when no active families', () => {
      makeCtx({ families: [] })
      render(<FamiliesPage />)
      expect(screen.getByText('families.emptyActive')).toBeInTheDocument()
    })

    it('shows emptyArchived when no archived families on archived tab', async () => {
      makeCtx({ families: [] })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /tabArchived/i }))
      expect(screen.getByText('families.emptyArchived')).toBeInTheDocument()
    })
  })

  // ── Tabs ───────────────────────────────────────────────────────────────────

  describe('tabs', () => {
    it('shows active families on active tab by default', () => {
      makeCtx({ families: [mockFamily, mockArchivedFamily] })
      render(<FamiliesPage />)
      expect(screen.getByText('Smith household')).toBeInTheDocument()
      expect(screen.queryByText('Old group')).not.toBeInTheDocument()
    })

    it('shows archived families when archived tab is clicked', async () => {
      makeCtx({ families: [mockFamily, mockArchivedFamily] })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /tabArchived/i }))
      expect(screen.getByText('Old group')).toBeInTheDocument()
      expect(screen.queryByText('Smith household')).not.toBeInTheDocument()
    })

    it('shows family counts in tab labels', () => {
      makeCtx({ families: [mockFamily, mockArchivedFamily] })
      render(<FamiliesPage />)
      expect(screen.getByRole('button', { name: /tabActive/ }).textContent).toContain('1')
      expect(screen.getByRole('button', { name: /tabArchived/ }).textContent).toContain('1')
    })

    it('active tab has active class, archived tab inactive by default', () => {
      makeCtx()
      render(<FamiliesPage />)
      const activeTab = screen.getByRole('button', { name: /tabActive/ })
      const archivedTab = screen.getByRole('button', { name: /tabArchived/ })
      expect(activeTab).toHaveClass('bg-brand-50', 'text-brand-700')
      expect(archivedTab).not.toHaveClass('bg-brand-50')
    })

    it('clicking active tab after viewing archived switches back to active families', async () => {
      makeCtx({ families: [mockFamily, mockArchivedFamily] })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /tabArchived/i }))
      expect(screen.queryByText('Smith household')).not.toBeInTheDocument()
      await user.click(screen.getByRole('button', { name: /tabActive/i }))
      expect(screen.getByText('Smith household')).toBeInTheDocument()
      expect(screen.queryByText('Old group')).not.toBeInTheDocument()
    })
  })

  // ── FamilyCard ─────────────────────────────────────────────────────────────

  describe('FamilyCard', () => {
    it('shows family name', () => {
      makeCtx()
      render(<FamiliesPage />)
      expect(screen.getByText('Smith household')).toBeInTheDocument()
    })

    it('shows default badge for default family', () => {
      makeCtx()
      render(<FamiliesPage />)
      expect(screen.getByText('families.default')).toBeInTheDocument()
    })

    it('shows archived badge for archived family', async () => {
      makeCtx({ families: [mockArchivedFamily] })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /tabArchived/i }))
      expect(screen.getByText('families.archived')).toBeInTheDocument()
    })

    it('shows role badge', () => {
      makeCtx()
      render(<FamiliesPage />)
      expect(screen.getByText('Head')).toBeInTheDocument()
    })

    it('shows expand button', () => {
      makeCtx()
      render(<FamiliesPage />)
      const expandBtn = screen.getByRole('button', { name: /families\.expand/i })
      expect(expandBtn).toHaveAttribute('aria-expanded', 'false')
    })

    it('loads and shows detail panel on expand click', async () => {
      makeCtx()
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({
        ok: true, status: 200, data: { ...mockFamily, members: [
          { userId: 10, firstName: 'Alice', lastName: 'Smith', email: 'alice@example.com', role: 'Head', joinedAt: '2024-01-01T00:00:00Z' }
        ]}
      })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.expand/i }))
      await waitFor(() => expect(screen.getByText('Alice Smith')).toBeInTheDocument())
      expect(familyApi.getFamilyById).toHaveBeenCalledWith(1)
    })

    it('collapses detail panel on second expand click', async () => {
      makeCtx()
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({ ok: true, status: 200, data: { ...mockFamily, members: [] } })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      const expandBtn = screen.getByRole('button', { name: /families\.expand/i })
      await user.click(expandBtn)
      await waitFor(() => expect(expandBtn).toHaveAttribute('aria-expanded', 'true'))
      await user.click(screen.getByRole('button', { name: /families\.collapse/i }))
      expect(screen.queryByText('families.members')).not.toBeInTheDocument()
    })

    it('shows no detail panel when getFamilyById fails', async () => {
      makeCtx()
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({ ok: false, status: 500 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.expand/i }))
      await waitFor(() => expect(familyApi.getFamilyById).toHaveBeenCalledWith(1))
      expect(screen.queryByText('families.members')).not.toBeInTheDocument()
    })

    it('does not reload detail on second expand if already loaded', async () => {
      makeCtx()
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({ ok: true, status: 200, data: { ...mockFamily, members: [] } })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.expand/i }))
      await waitFor(() => expect(familyApi.getFamilyById).toHaveBeenCalledTimes(1))
      await user.click(screen.getByRole('button', { name: /families\.collapse/i }))
      await user.click(screen.getByRole('button', { name: /families\.expand/i }))
      expect(familyApi.getFamilyById).toHaveBeenCalledTimes(1)
    })

    it('shows archive button for Head + non-default + non-archived family', () => {
      makeCtx({ families: [mockFamily2] })
      render(<FamiliesPage />)
      expect(screen.getByRole('button', { name: /families\.archiveAction/i })).toBeInTheDocument()
    })

    it('does not show archive button for default family', () => {
      makeCtx({ families: [mockFamily] }) // isDefault=true
      render(<FamiliesPage />)
      expect(screen.queryByRole('button', { name: /families\.archiveAction/i })).not.toBeInTheDocument()
    })

    it('calls archiveFamily when archive button clicked', async () => {
      makeCtx({ families: [mockFamily2] })
      vi.mocked(familyApi.archiveFamily).mockResolvedValue({ ok: true, status: 200 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.archiveAction/i }))
      expect(familyApi.archiveFamily).toHaveBeenCalledWith(2)
    })

    it('shows toast and calls refresh after archive', async () => {
      const refresh = vi.fn()
      makeCtx({ families: [mockFamily2], refresh })
      vi.mocked(familyApi.archiveFamily).mockResolvedValue({ ok: true, status: 200 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.archiveAction/i }))
      await waitFor(() => expect(mockShow).toHaveBeenCalledWith('families.archiveSuccess', 'success'))
      expect(refresh).toHaveBeenCalledTimes(2) // once on mount, once after archive
    })

    it('does not show toast or refresh when archive fails', async () => {
      const refresh = vi.fn()
      makeCtx({ families: [mockFamily2], refresh })
      vi.mocked(familyApi.archiveFamily).mockResolvedValue({ ok: false, status: 422 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.archiveAction/i }))
      await waitFor(() => expect(familyApi.archiveFamily).toHaveBeenCalledWith(2))
      expect(mockShow).not.toHaveBeenCalled()
      expect(refresh).toHaveBeenCalledTimes(1) // mount only
    })

    it('shows rename button for Head + non-default + non-archived family', () => {
      makeCtx({ families: [mockFamily2] })
      render(<FamiliesPage />)
      expect(screen.getByRole('button', { name: /families\.renameAction/i })).toBeInTheDocument()
    })

    it('opens rename modal when rename button clicked', async () => {
      makeCtx({ families: [mockFamily2] })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.renameAction/i }))
      expect(screen.getByText('families.renameTitle')).toBeInTheDocument()
    })

    it('calls renameFamily on rename modal submit', async () => {
      makeCtx({ families: [mockFamily2] })
      vi.mocked(familyApi.renameFamily).mockResolvedValue({ ok: true, status: 200 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.renameAction/i }))
      const input = screen.getByRole('textbox')
      await user.clear(input)
      await user.type(input, 'New Name')
      await user.click(screen.getByRole('button', { name: /families\.renameSubmit/i }))
      await waitFor(() => expect(familyApi.renameFamily).toHaveBeenCalledWith(2, 'New Name'))
    })

    it('shows toast and closes rename modal on success', async () => {
      makeCtx({ families: [mockFamily2] })
      vi.mocked(familyApi.renameFamily).mockResolvedValue({ ok: true, status: 200 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.renameAction/i }))
      const input = screen.getByRole('textbox')
      await user.clear(input)
      await user.type(input, 'New Name')
      await user.click(screen.getByRole('button', { name: /families\.renameSubmit/i }))
      await waitFor(() => expect(screen.queryByText('families.renameTitle')).not.toBeInTheDocument())
      expect(mockShow).toHaveBeenCalledWith('families.renameSuccess', 'success')
    })

    it('shows validation error when rename name is empty', async () => {
      makeCtx({ families: [mockFamily2] })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.renameAction/i }))
      const input = screen.getByRole('textbox')
      await user.clear(input)
      await user.click(screen.getByRole('button', { name: /families\.renameSubmit/i }))
      await waitFor(() => expect(screen.getByRole('alert')).toBeInTheDocument())
      expect(familyApi.renameFamily).not.toHaveBeenCalled()
    })

    it('does not close rename modal when rename API fails', async () => {
      makeCtx({ families: [mockFamily2] })
      vi.mocked(familyApi.renameFamily).mockResolvedValue({ ok: false, status: 400 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.renameAction/i }))
      const input = screen.getByRole('textbox')
      await user.clear(input)
      await user.type(input, 'New Name')
      await user.click(screen.getByRole('button', { name: /families\.renameSubmit/i }))
      await waitFor(() => expect(familyApi.renameFamily).toHaveBeenCalled())
      expect(screen.getByText('families.renameTitle')).toBeInTheDocument()
      expect(mockShow).not.toHaveBeenCalled()
    })
  })

  // ── Archived FamilyCard ────────────────────────────────────────────────────

  describe('archived family card', () => {
    beforeEach(() => {
      makeCtx({ families: [mockArchivedFamily] })
    })

    async function renderOnArchivedTab() {
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /tabArchived/i }))
      return user
    }

    it('shows unarchive button for Head + non-default + archived family', async () => {
      await renderOnArchivedTab()
      expect(screen.getByRole('button', { name: /families\.unarchiveAction/i })).toBeInTheDocument()
    })

    it('calls unarchiveFamily when unarchive button clicked', async () => {
      vi.mocked(familyApi.unarchiveFamily).mockResolvedValue({ ok: true, status: 200 })
      const user = await renderOnArchivedTab()
      await user.click(screen.getByRole('button', { name: /families\.unarchiveAction/i }))
      expect(familyApi.unarchiveFamily).toHaveBeenCalledWith(3)
    })

    it('shows toast and refreshes after unarchive', async () => {
      const refresh = vi.fn()
      mockUseFamilies.mockReturnValue({ families: [mockArchivedFamily], isLoading: false, refresh })
      vi.mocked(familyApi.unarchiveFamily).mockResolvedValue({ ok: true, status: 200 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /tabArchived/i }))
      await user.click(screen.getByRole('button', { name: /families\.unarchiveAction/i }))
      await waitFor(() => expect(mockShow).toHaveBeenCalledWith('families.unarchiveSuccess', 'success'))
      expect(refresh).toHaveBeenCalledTimes(2)
    })

    it('does not show toast or refresh when unarchive fails', async () => {
      const refresh = vi.fn()
      mockUseFamilies.mockReturnValue({ families: [mockArchivedFamily], isLoading: false, refresh })
      vi.mocked(familyApi.unarchiveFamily).mockResolvedValue({ ok: false, status: 422 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /tabArchived/i }))
      await user.click(screen.getByRole('button', { name: /families\.unarchiveAction/i }))
      await waitFor(() => expect(familyApi.unarchiveFamily).toHaveBeenCalledWith(3))
      expect(mockShow).not.toHaveBeenCalled()
      expect(refresh).toHaveBeenCalledTimes(1) // mount only
    })
  })

  // ── Create Family Modal ────────────────────────────────────────────────────

  describe('CreateFamilyModal', () => {
    it('opens when create button is clicked', async () => {
      makeCtx()
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.createAction/i }))
      expect(screen.getByText('families.createTitle')).toBeInTheDocument()
    })

    it('closes when modal close button is clicked', async () => {
      makeCtx()
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.createAction/i }))
      await user.click(screen.getByRole('button', { name: /close/i }))
      expect(screen.queryByText('families.createTitle')).not.toBeInTheDocument()
    })

    it('closes when overlay is clicked', async () => {
      makeCtx()
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.createAction/i }))
      // click the fixed overlay (parent of modal content)
      const overlay = document.querySelector('.fixed.inset-0')!
      await user.click(overlay)
      expect(screen.queryByText('families.createTitle')).not.toBeInTheDocument()
    })

    it('submits and calls createFamily', async () => {
      makeCtx()
      vi.mocked(familyApi.createFamily).mockResolvedValue({ ok: true, status: 201 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.createAction/i }))
      await user.type(screen.getByRole('textbox'), 'My Family')
      await user.click(screen.getByRole('button', { name: /families\.createSubmit/i }))
      await waitFor(() => expect(familyApi.createFamily).toHaveBeenCalledWith('My Family'))
    })

    it('shows toast, refreshes, and closes modal on success', async () => {
      const refresh = vi.fn()
      makeCtx({ refresh })
      vi.mocked(familyApi.createFamily).mockResolvedValue({ ok: true, status: 201 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.createAction/i }))
      await user.type(screen.getByRole('textbox'), 'My Family')
      await user.click(screen.getByRole('button', { name: /families\.createSubmit/i }))
      await waitFor(() => expect(screen.queryByText('families.createTitle')).not.toBeInTheDocument())
      expect(mockShow).toHaveBeenCalledWith('families.createSuccess', 'success')
      expect(refresh).toHaveBeenCalledTimes(2) // mount + create
    })

    it('shows validation error when name is empty', async () => {
      makeCtx()
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.createAction/i }))
      await user.click(screen.getByRole('button', { name: /families\.createSubmit/i }))
      await waitFor(() => expect(screen.getByRole('alert')).toBeInTheDocument())
      expect(familyApi.createFamily).not.toHaveBeenCalled()
    })

    it('does not close modal when API fails', async () => {
      makeCtx()
      vi.mocked(familyApi.createFamily).mockResolvedValue({ ok: false, status: 400 })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.createAction/i }))
      await user.type(screen.getByRole('textbox'), 'My Family')
      await user.click(screen.getByRole('button', { name: /families\.createSubmit/i }))
      await waitFor(() => expect(familyApi.createFamily).toHaveBeenCalled())
      expect(screen.getByText('families.createTitle')).toBeInTheDocument()
    })
  })

  // ── FamilyDetailPanel ──────────────────────────────────────────────────────

  describe('FamilyDetailPanel', () => {
    async function renderExpanded(overrides: Partial<{ detail: FamilyDetail; family: Family }> = {}) {
      const family = overrides.family ?? mockFamily2
      const detail = overrides.detail ?? mockDetail
      makeCtx({ families: [family] })
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({ ok: true, status: 200, data: detail })
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.expand/i }))
      await waitFor(() => expect(screen.getByText('Alice Smith')).toBeInTheDocument())
      return user
    }

    it('shows all members with name and email', async () => {
      await renderExpanded()
      expect(screen.getByText('Alice Smith')).toBeInTheDocument()
      expect(screen.getByText('alice@example.com')).toBeInTheDocument()
      expect(screen.getByText('Bob Jones')).toBeInTheDocument()
      expect(screen.getByText('bob@example.com')).toBeInTheDocument()
    })

    it('shows role badges for members', async () => {
      await renderExpanded()
      expect(screen.getAllByText('Head').length).toBeGreaterThan(0)
      expect(screen.getByText('Member')).toBeInTheDocument()
    })

    it('shows invite button for Head family', async () => {
      await renderExpanded()
      expect(screen.getByRole('button', { name: /inviteAction/i })).toBeInTheDocument()
    })

    it('does not show invite button for Member family', async () => {
      const memberFamily: Family = { ...mockFamily2, userRole: 'Member' }
      await renderExpanded({ family: memberFamily, detail: { ...mockDetail, userRole: 'Member' } })
      expect(screen.queryByRole('button', { name: /inviteAction/i })).not.toBeInTheDocument()
    })

    it('calls removeMember when remove button clicked', async () => {
      vi.mocked(familyApi.removeMember).mockResolvedValue({ ok: true, status: 200 })
      const user = await renderExpanded()
      const removeButtons = screen.getAllByTitle('families.removeMember')
      await user.click(removeButtons[0])
      expect(familyApi.removeMember).toHaveBeenCalledWith(2, 10)
    })

    it('shows toast and refreshes after member removed', async () => {
      vi.mocked(familyApi.removeMember).mockResolvedValue({ ok: true, status: 200 })
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({ ok: true, status: 200, data: mockDetail })
      const user = await renderExpanded()
      await user.click(screen.getAllByTitle('families.removeMember')[0])
      await waitFor(() => expect(mockShow).toHaveBeenCalledWith(expect.stringContaining('families.memberRemoved'), 'success'))
    })

    it('does not show toast when removeMember fails', async () => {
      vi.mocked(familyApi.removeMember).mockResolvedValue({ ok: false, status: 422 })
      const user = await renderExpanded()
      await user.click(screen.getAllByTitle('families.removeMember')[0])
      await waitFor(() => expect(familyApi.removeMember).toHaveBeenCalled())
      expect(mockShow).not.toHaveBeenCalled()
    })

    it('calls changeMemberRole Head→Member when toggle clicked', async () => {
      vi.mocked(familyApi.changeMemberRole).mockResolvedValue({ ok: true, status: 200 })
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({ ok: true, status: 200, data: mockDetail })
      const user = await renderExpanded()
      const roleButtons = screen.getAllByTitle('families.changeRole')
      await user.click(roleButtons[0]) // Alice is Head → becomes Member
      expect(familyApi.changeMemberRole).toHaveBeenCalledWith(2, 10, 'Member')
    })

    it('calls changeMemberRole Member→Head when toggle clicked', async () => {
      vi.mocked(familyApi.changeMemberRole).mockResolvedValue({ ok: true, status: 200 })
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({ ok: true, status: 200, data: mockDetail })
      const user = await renderExpanded()
      const roleButtons = screen.getAllByTitle('families.changeRole')
      await user.click(roleButtons[1]) // Bob is Member → becomes Head
      expect(familyApi.changeMemberRole).toHaveBeenCalledWith(2, 11, 'Head')
    })

    it('shows toast after role changed', async () => {
      vi.mocked(familyApi.changeMemberRole).mockResolvedValue({ ok: true, status: 200 })
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({ ok: true, status: 200, data: mockDetail })
      const user = await renderExpanded()
      await user.click(screen.getAllByTitle('families.changeRole')[0])
      await waitFor(() => expect(mockShow).toHaveBeenCalledWith('families.roleChanged', 'success'))
    })

    it('does not show toast when changeMemberRole fails', async () => {
      vi.mocked(familyApi.changeMemberRole).mockResolvedValue({ ok: false, status: 422 })
      vi.mocked(familyApi.getFamilyById).mockResolvedValue({ ok: true, status: 200, data: mockDetail })
      const user = await renderExpanded()
      await user.click(screen.getAllByTitle('families.changeRole')[0])
      await waitFor(() => expect(familyApi.changeMemberRole).toHaveBeenCalled())
      expect(mockShow).not.toHaveBeenCalled()
    })

    it('opens InviteMemberModal when invite button clicked', async () => {
      const user = await renderExpanded()
      await user.click(screen.getByRole('button', { name: /inviteAction/i }))
      expect(screen.getByText('families.inviteTitle')).toBeInTheDocument()
    })

    it('calls inviteMember when invite form submitted', async () => {
      vi.mocked(familyApi.inviteMember).mockResolvedValue({ ok: true, status: 200 })
      const user = await renderExpanded()
      await user.click(screen.getByRole('button', { name: /inviteAction/i }))
      await user.type(screen.getByRole('textbox', { name: /auth\.email\.label/i }), 'new@example.com')
      await user.click(screen.getByRole('button', { name: /families\.inviteSubmit/i }))
      await waitFor(() => expect(familyApi.inviteMember).toHaveBeenCalledWith(2, 'new@example.com'))
    })

    it('shows toast and closes invite modal on success', async () => {
      vi.mocked(familyApi.inviteMember).mockResolvedValue({ ok: true, status: 200 })
      const user = await renderExpanded()
      await user.click(screen.getByRole('button', { name: /inviteAction/i }))
      await user.type(screen.getByRole('textbox', { name: /auth\.email\.label/i }), 'new@example.com')
      await user.click(screen.getByRole('button', { name: /families\.inviteSubmit/i }))
      await waitFor(() => expect(screen.queryByText('families.inviteTitle')).not.toBeInTheDocument())
      expect(mockShow).toHaveBeenCalledWith('families.inviteSuccess', 'success')
    })

    it('shows validation error when invite email is empty', async () => {
      const user = await renderExpanded()
      await user.click(screen.getByRole('button', { name: /inviteAction/i }))
      await user.click(screen.getByRole('button', { name: /families\.inviteSubmit/i }))
      await waitFor(() => expect(screen.getByRole('alert')).toBeInTheDocument())
      expect(familyApi.inviteMember).not.toHaveBeenCalled()
    })

    it('does not close invite modal when inviteMember fails', async () => {
      vi.mocked(familyApi.inviteMember).mockResolvedValue({ ok: false, status: 400 })
      const user = await renderExpanded()
      await user.click(screen.getByRole('button', { name: /inviteAction/i }))
      await user.type(screen.getByRole('textbox', { name: /auth\.email\.label/i }), 'new@example.com')
      await user.click(screen.getByRole('button', { name: /families\.inviteSubmit/i }))
      await waitFor(() => expect(familyApi.inviteMember).toHaveBeenCalled())
      expect(screen.getByText('families.inviteTitle')).toBeInTheDocument()
      expect(mockShow).not.toHaveBeenCalled()
    })
  })

  // ── Modal interactions ─────────────────────────────────────────────────────

  describe('Modal', () => {
    it('click inside modal does not close it', async () => {
      makeCtx()
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.createAction/i }))
      const inner = document.querySelector('.bg-white.rounded-2xl.shadow-xl')!
      await user.click(inner)
      expect(screen.getByText('families.createTitle')).toBeInTheDocument()
    })

    it('closes modal when Escape key is pressed', async () => {
      makeCtx()
      const user = userEvent.setup()
      render(<FamiliesPage />)
      await user.click(screen.getByRole('button', { name: /families\.createAction/i }))
      expect(screen.getByText('families.createTitle')).toBeInTheDocument()
      await user.keyboard('{Escape}')
      expect(screen.queryByText('families.createTitle')).not.toBeInTheDocument()
    })
  })
})
