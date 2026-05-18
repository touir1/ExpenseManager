import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import AcceptInvitePage from '../AcceptInvitePage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))

vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: vi.fn() }))

const mockSearchParams = vi.fn()
vi.mock('react-router-dom', () => ({
  useSearchParams: () => [{ get: mockSearchParams }],
  Link: ({ children, to }: { children: React.ReactNode; to: string }) => <a href={to}>{children}</a>,
}))

const mockAcceptInvite = vi.fn()
vi.mock('@/features/families/services/familyApi.service', () => ({
  acceptInvite: (...args: unknown[]) => mockAcceptInvite(...args),
}))

describe('AcceptInvitePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockSearchParams.mockReturnValue('test-token')
    mockAcceptInvite.mockResolvedValue({ ok: true })
  })

  it('shows loading state initially before invite resolves', () => {
    mockAcceptInvite.mockReturnValue(new Promise(() => {}))
    render(<AcceptInvitePage />)
    expect(screen.getByText('families.acceptInvite.loading')).toBeInTheDocument()
  })

  it('shows success state after successful invite', async () => {
    mockAcceptInvite.mockResolvedValue({ ok: true })
    render(<AcceptInvitePage />)
    await waitFor(() =>
      expect(screen.getByText('families.acceptInvite.success')).toBeInTheDocument()
    )
  })

  it('shows res.error as error message when invite fails with error string', async () => {
    mockAcceptInvite.mockResolvedValue({ ok: false, error: 'apiErrors.familyInvitationExpired' })
    render(<AcceptInvitePage />)
    await waitFor(() =>
      expect(screen.getByText('apiErrors.familyInvitationExpired')).toBeInTheDocument()
    )
  })

  it('shows fallback error message when invite fails without error string', async () => {
    mockAcceptInvite.mockResolvedValue({ ok: false })
    render(<AcceptInvitePage />)
    await waitFor(() =>
      expect(screen.getByText('families.acceptInvite.error')).toBeInTheDocument()
    )
  })

  it('shows error state immediately when token is missing', async () => {
    mockSearchParams.mockReturnValue(null)
    render(<AcceptInvitePage />)
    await waitFor(() =>
      expect(screen.getByText('families.acceptInvite.error')).toBeInTheDocument()
    )
  })

  it('does not call acceptInvite when token is missing', async () => {
    mockSearchParams.mockReturnValue(null)
    render(<AcceptInvitePage />)
    await waitFor(() => expect(screen.getByText('families.acceptInvite.error')).toBeInTheDocument())
    expect(mockAcceptInvite).not.toHaveBeenCalled()
  })

  it('calls acceptInvite with token and silent:true', async () => {
    render(<AcceptInvitePage />)
    await waitFor(() => expect(mockAcceptInvite).toHaveBeenCalledWith('test-token', { silent: true }))
  })

  it('shows go-to-families link on success', async () => {
    render(<AcceptInvitePage />)
    await waitFor(() =>
      expect(screen.getByText('families.acceptInvite.goToFamilies')).toBeInTheDocument()
    )
  })

  it('shows go-to-families link on error', async () => {
    mockAcceptInvite.mockResolvedValue({ ok: false, error: 'some error' })
    render(<AcceptInvitePage />)
    await waitFor(() =>
      expect(screen.getByText('families.acceptInvite.goToFamilies')).toBeInTheDocument()
    )
  })
})
