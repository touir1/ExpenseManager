import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import FamilySelector from '../FamilySelector'
import type { Family } from '../../types/family.type'

const mockUseFamilies = vi.fn()
vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => mockUseFamilies(),
}))

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))

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
  userRole: 'Member',
  createdAt: '2024-02-01T00:00:00Z',
}

function makeFamilies(
  overrides: Partial<ReturnType<typeof mockUseFamilies>> = {}
) {
  mockUseFamilies.mockReturnValue({
    families: [mockFamily],
    activeFamilyId: null,
    setActiveFamilyId: vi.fn(),
    ...overrides,
  })
}

describe('FamilySelector', () => {
  beforeEach(() => vi.clearAllMocks())

  it('returns null when no active families', () => {
    mockUseFamilies.mockReturnValue({
      families: [],
      activeFamilyId: null,
      setActiveFamilyId: vi.fn(),
    })
    const { container } = render(<FamilySelector />)
    expect(container.firstChild).toBeNull()
  })

  it('returns null when all families are archived', () => {
    mockUseFamilies.mockReturnValue({
      families: [{ ...mockFamily, isArchived: true }],
      activeFamilyId: null,
      setActiveFamilyId: vi.fn(),
    })
    const { container } = render(<FamilySelector />)
    expect(container.firstChild).toBeNull()
  })

  it('renders toggle button when active families exist', () => {
    makeFamilies()
    render(<FamilySelector />)
    expect(screen.getByRole('button')).toBeInTheDocument()
  })

  it('shows selectorPersonal label when activeFamilyId is null', () => {
    makeFamilies()
    render(<FamilySelector />)
    expect(screen.getByRole('button').textContent).toContain('families.selectorPersonal')
  })

  it('shows active family name when a family is selected', () => {
    makeFamilies({ families: [mockFamily, mockFamily2], activeFamilyId: 2 })
    render(<FamilySelector />)
    expect(screen.getByRole('button').textContent).toContain('Holiday')
  })

  it('dropdown is not visible initially', () => {
    makeFamilies()
    render(<FamilySelector />)
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('opens dropdown when toggle button is clicked', async () => {
    makeFamilies()
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    expect(screen.getByRole('listbox')).toBeInTheDocument()
  })

  it('shows selectorPersonal option in dropdown', async () => {
    makeFamilies()
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    const options = screen.getAllByRole('option')
    expect(options[0].textContent).toBe('families.selectorPersonal')
  })

  it('shows non-default families in dropdown', async () => {
    makeFamilies({ families: [mockFamily, mockFamily2] })
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    expect(screen.getByRole('option', { name: 'Holiday' })).toBeInTheDocument()
  })

  it('does not show default family as a selectable option in dropdown', async () => {
    makeFamilies({ families: [mockFamily, mockFamily2] })
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    const options = screen.getAllByRole('option')
    expect(options.map(o => o.textContent)).not.toContain('Smith household')
  })

  it('calls setActiveFamilyId(null) when selectorPersonal option is clicked', async () => {
    const setActiveFamilyId = vi.fn()
    makeFamilies({ setActiveFamilyId })
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    await user.click(screen.getAllByRole('option')[0])
    expect(setActiveFamilyId).toHaveBeenCalledWith(null)
  })

  it('calls setActiveFamilyId with family id when a family option is clicked', async () => {
    const setActiveFamilyId = vi.fn()
    makeFamilies({ families: [mockFamily, mockFamily2], setActiveFamilyId })
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    await user.click(screen.getByRole('option', { name: 'Holiday' }))
    expect(setActiveFamilyId).toHaveBeenCalledWith(2)
  })

  it('closes dropdown after selecting an option', async () => {
    makeFamilies({ families: [mockFamily, mockFamily2] })
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    await user.click(screen.getAllByRole('option')[0])
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('closes dropdown when clicking outside', async () => {
    makeFamilies()
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    expect(screen.getByRole('listbox')).toBeInTheDocument()
    fireEvent.mouseDown(document.body)
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('toggle button has aria-haspopup=listbox', () => {
    makeFamilies()
    render(<FamilySelector />)
    expect(screen.getByRole('button')).toHaveAttribute('aria-haspopup', 'listbox')
  })

  it('toggle button aria-expanded is false when closed', () => {
    makeFamilies()
    render(<FamilySelector />)
    expect(screen.getByRole('button')).toHaveAttribute('aria-expanded', 'false')
  })

  it('toggle button aria-expanded is true when open', async () => {
    makeFamilies()
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    expect(screen.getByRole('button')).toHaveAttribute('aria-expanded', 'true')
  })

  it('selectorPersonal option has aria-selected=true when activeFamilyId is null', async () => {
    makeFamilies()
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    expect(screen.getAllByRole('option')[0]).toHaveAttribute('aria-selected', 'true')
  })

  it('family option has aria-selected=true when it is active', async () => {
    makeFamilies({ families: [mockFamily, mockFamily2], activeFamilyId: 2 })
    const user = userEvent.setup()
    render(<FamilySelector />)
    await user.click(screen.getByRole('button'))
    expect(screen.getByRole('option', { name: 'Holiday' })).toHaveAttribute('aria-selected', 'true')
  })
})
