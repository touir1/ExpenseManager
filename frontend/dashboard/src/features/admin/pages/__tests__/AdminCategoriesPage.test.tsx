import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import AdminCategoriesPage from '../AdminCategoriesPage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))
vi.mock('@/hooks/usePageTitle', () => ({ usePageTitle: vi.fn() }))

const mockGetCategories = vi.fn()
const mockAddCategory = vi.fn()
const mockAddSubcategory = vi.fn()
const mockArchiveCategory = vi.fn()
const mockUnarchiveCategory = vi.fn()

vi.mock('@/features/admin/services/adminCategoriesApi.service', () => ({
  getCategories: (...a: unknown[]) => mockGetCategories(...a),
  addCategory: (...a: unknown[]) => mockAddCategory(...a),
  updateCategory: vi.fn(),
  archiveCategory: (...a: unknown[]) => mockArchiveCategory(...a),
  unarchiveCategory: (...a: unknown[]) => mockUnarchiveCategory(...a),
  addSubcategory: (...a: unknown[]) => mockAddSubcategory(...a),
  updateSubcategory: vi.fn(),
  archiveSubcategory: vi.fn(),
  unarchiveSubcategory: vi.fn(),
}))

const categories = [
  { id: 1, name: 'Food', description: null, icon: null, isArchived: false, subcategories: [
    { id: 2, name: 'Groceries', description: null, icon: null, isArchived: false }
  ] },
  { id: 3, name: 'OldCat', description: null, icon: null, isArchived: true, subcategories: [] }
]

function renderPage() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <AdminCategoriesPage />
    </QueryClientProvider>
  )
}

describe('AdminCategoriesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetCategories.mockResolvedValue({ ok: true, data: categories })
    mockAddCategory.mockResolvedValue({ ok: true, data: { id: 10, name: 'New', isArchived: false, subcategories: [] } })
    mockAddSubcategory.mockResolvedValue({ ok: true, data: { id: 11, name: 'NewSub', isArchived: false } })
    mockArchiveCategory.mockResolvedValue({ ok: true })
    mockUnarchiveCategory.mockResolvedValue({ ok: true })
  })

  it('renders active categories', async () => {
    renderPage()
    await waitFor(() => expect(screen.getByText('Food')).toBeInTheDocument())
  })

  it('hides archived categories by default', async () => {
    renderPage()
    await waitFor(() => screen.getByText('Food'))
    expect(screen.queryByText('OldCat')).not.toBeInTheDocument()
  })

  it('shows archived categories when toggle enabled', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('Food'))
    const toggle = screen.getByRole('checkbox')
    await user.click(toggle)
    expect(screen.getByText('OldCat')).toBeInTheDocument()
  })

  it('shows Add Category button', async () => {
    renderPage()
    await waitFor(() => screen.getByText('Food'))
    expect(screen.getByText('admin.categories.add')).toBeInTheDocument()
  })

  it('opens add category modal and submits', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('Food'))
    await user.click(screen.getByText('admin.categories.add'))
    const nameInput = screen.getByPlaceholderText('Name')
    await user.type(nameInput, 'NewCategory')
    await user.click(screen.getByText('common.save'))
    await waitFor(() => expect(mockAddCategory).toHaveBeenCalled())
  })

  it('calls archiveCategory when archive clicked', async () => {
    const user = userEvent.setup()
    renderPage()
    await waitFor(() => screen.getByText('Food'))
    const archiveBtns = screen.getAllByText('admin.categories.archive')
    await user.click(archiveBtns[0])
    expect(mockArchiveCategory).toHaveBeenCalledWith(1)
  })
})
