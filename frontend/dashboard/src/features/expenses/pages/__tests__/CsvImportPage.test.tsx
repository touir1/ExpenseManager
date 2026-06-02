import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import CsvImportPage from '../CsvImportPage'
import type { CsvImportPreviewDto } from '@/features/expenses/types/expenses.type'

// ── Mocks ─────────────────────────────────────────────────────────────────────

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate }
})

const mockPreviewCsvImport = vi.fn()
const mockConfirmCsvImport = vi.fn()
const mockValidateCsvRows = vi.fn()
const mockGetImportTemplateUrl = vi.fn().mockReturnValue('/api/expenses/import/template')

vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  previewCsvImport: (...args: unknown[]) => mockPreviewCsvImport(...args),
  confirmCsvImport: (...args: unknown[]) => mockConfirmCsvImport(...args),
  validateCsvRows: (...args: unknown[]) => mockValidateCsvRows(...args),
  getImportTemplateUrl: () => mockGetImportTemplateUrl(),
}))

vi.mock('@/features/expenses/ExpensesDataContext', () => ({
  useExpensesData: () => ({
    currencies: [{ id: 1, code: 'EUR', name: 'Euro', symbol: '€', decimals: 2 }],
    categories: [
      { id: 10, name: 'Food', subcategories: [{ id: 11, name: 'Restaurant', description: null }] },
    ],
    tags: [{ id: 1, name: 'work' }, { id: 2, name: 'client' }],
    isLoading: false,
    refresh: vi.fn(),
  }),
}))

vi.mock('@/features/families/FamilyContext', () => ({
  useFamilies: () => ({
    families: [
      { id: 1, name: 'My Family', isDefault: true, isArchived: false, userRole: 'Head', createdAt: '' },
      { id: 2, name: 'Shared', isDefault: false, isArchived: false, userRole: 'Member', createdAt: '' },
    ],
    activeFamilyId: null,
    setActiveFamilyId: vi.fn(),
    isLoading: false,
    refresh: vi.fn(),
  }),
}))


// ── Fixtures ──────────────────────────────────────────────────────────────────

const validRow = {
  rowNumber: 1,
  isValid: true,
  errors: [],
  dateDisplay: '2025-01-15',
  amountDisplay: '45.50',
  currencyDisplay: 'EUR',
  categoryDisplay: 'Food',
  subcategoryDisplay: 'Restaurant',
  descriptionDisplay: 'Lunch',
  tagNames: ['work'],
  familiesDisplay: null,
  date: '2025-01-15',
  amount: 45.5,
  currencyId: 1,
  categoryId: 10,
  subcategoryId: 11,
  familyIds: null,
}

const errorRow = {
  rowNumber: 2,
  isValid: false,
  errors: ['AMOUNT_INVALID'],
  dateDisplay: '2025-01-20',
  amountDisplay: 'bad',
  currencyDisplay: 'EUR',
  categoryDisplay: '',
  subcategoryDisplay: '',
  descriptionDisplay: '',
  tagNames: null,
  familiesDisplay: null,
  date: null,
  amount: null,
  currencyId: null,
  categoryId: null,
  subcategoryId: null,
  familyIds: null,
}

const previewWithErrors: CsvImportPreviewDto = {
  totalRows: 2, validCount: 1, errorCount: 1, rows: [validRow, errorRow],
}

const previewAllValid: CsvImportPreviewDto = {
  totalRows: 1, validCount: 1, errorCount: 0, rows: [validRow],
}

const previewAllInvalid: CsvImportPreviewDto = {
  totalRows: 1, validCount: 0, errorCount: 1, rows: [errorRow],
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function renderPage() {
  render(
    <MemoryRouter initialEntries={['/expenses/import']}>
      <Routes>
        <Route path="/expenses/import" element={<CsvImportPage />} />
        <Route path="/expenses" element={<div>Expenses List</div>} />
      </Routes>
    </MemoryRouter>,
  )
}

function makeFile(content = 'date,amount\n') {
  return new File([content], 'test.csv', { type: 'text/csv' })
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('CsvImportPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetImportTemplateUrl.mockReturnValue('/api/expenses/import/template')
  })

  it('renders upload dropzone and template link', () => {
    renderPage()
    expect(screen.getByRole('button', { name: /drag.*drop|csv/i })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /template/i })).toHaveAttribute('href', '/api/expenses/import/template')
  })

  it('shows preview table after file upload', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => { expect(screen.getByText('2025-01-15')).toBeInTheDocument() })
  })

  it('shows all 8 CSV columns in table header', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByText(/subcategory/i)).toBeInTheDocument()
      expect(screen.getByText(/tags/i)).toBeInTheDocument()
      expect(screen.getByText(/families/i)).toBeInTheDocument()
    })
  })

  it('shows tags as chips in display mode', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => { expect(screen.getByText('work')).toBeInTheDocument() })
  })

  it('shows "—" for empty families in display mode', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => { expect(screen.getByText('—')).toBeInTheDocument() })
  })

  it('does not show default family in FamilyMultiSelect dropdown', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 families/i))

    fireEvent.mouseDown(screen.getByLabelText(/row 2 families/i))

    await waitFor(() => {
      expect(screen.queryByText('My Family')).not.toBeInTheDocument()
      expect(screen.getByText('Shared')).toBeInTheDocument()
    })
  })

  it('shows valid count and error count badges', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByText(/1 valid row/i)).toBeInTheDocument()
      expect(screen.getByText(/1 row.*error/i)).toBeInTheDocument()
    })
  })

  it('shows error codes in status column', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => { expect(screen.getByText(/Invalid amount/i)).toBeInTheDocument() })
  })

  it('shows edit button for each row', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /edit row 1/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /edit row 2/i })).toBeInTheDocument()
    })
  })

  it('shows editable inputs after clicking edit', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))

    await waitFor(() => {
      expect(screen.getByLabelText(/row 2 amount/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/row 2 date/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/row 2 tags/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/row 2 families/i)).toBeInTheDocument()
    })
  })

  it('shows save and cancel buttons when editing', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))
    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /save row 2/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /cancel editing row 2/i })).toBeInTheDocument()
    })
  })

  it('cancel discards pending edits', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))
    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))

    fireEvent.change(screen.getByLabelText(/row 2 amount/i), { target: { value: '999' } })
    fireEvent.click(screen.getByRole('button', { name: /cancel editing row 2/i }))

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /edit row 2/i })).toBeInTheDocument()
      expect(screen.queryByLabelText(/row 2 amount/i)).not.toBeInTheDocument()
    })
  })

  it('shows import button (not re-validate) when there are error rows but no edits', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.queryByRole('button', { name: /re-validate/i })).not.toBeInTheDocument()
      expect(screen.getByRole('button', { name: /import/i })).toBeInTheDocument()
    })
  })

  it('shows re-validate (not import) after making an edit', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))
    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))

    fireEvent.change(screen.getByLabelText(/row 2 amount/i), { target: { value: '25.00' } })
    fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /re-validate/i })).toBeInTheDocument()
      expect(screen.queryByRole('button', { name: /import/i })).not.toBeInTheDocument()
    })
  })

  it('does not show re-validate button when all rows valid and no edits', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => { expect(screen.queryByRole('button', { name: /re-validate/i })).not.toBeInTheDocument() })
  })

  it('calls validateCsvRows with pending edits auto-saved on re-validate', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockValidateCsvRows.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))

    fireEvent.change(screen.getByLabelText(/row 2 amount/i), { target: { value: '25.00' } })
    fireEvent.click(screen.getByRole('button', { name: /re-validate/i }))

    await waitFor(() => {
      expect(mockValidateCsvRows).toHaveBeenCalledOnce()
      const [rows] = mockValidateCsvRows.mock.calls[0]
      expect(rows.find((r: { rowNumber: number }) => r.rowNumber === 2).amount).toBe('25.00')
    })
  })

  it('tags sent as semicolon-separated string to validate-rows', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    mockValidateCsvRows.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /edit row 1/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 1/i }))
    await waitFor(() => screen.getByLabelText(/row 1 tags/i))

    // Row 1 has tagNames: ['work'], so tags input should exist
    fireEvent.click(screen.getByRole('button', { name: /re-validate/i }))

    await waitFor(() => {
      const [rows] = mockValidateCsvRows.mock.calls[0]
      // tags were ['work'] from the row, serialised to 'work'
      expect(rows.find((r: { rowNumber: number }) => r.rowNumber === 1).tags).toBe('work')
    })
  })

  it('updates preview after successful re-validate', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockValidateCsvRows.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))
    fireEvent.change(screen.getByLabelText(/row 2 amount/i), { target: { value: '25.00' } })
    fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))

    await waitFor(() => screen.getByRole('button', { name: /re-validate/i }))
    fireEvent.click(screen.getByRole('button', { name: /re-validate/i }))

    await waitFor(() => {
      expect(screen.queryByRole('button', { name: /re-validate/i })).not.toBeInTheDocument()
      expect(screen.getByRole('button', { name: /import/i })).toBeInTheDocument()
    })
  })

  it('disables confirm when no valid rows', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllInvalid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => { expect(screen.getByRole('button', { name: /import 0/i })).toBeDisabled() })
  })

  it('calls confirmCsvImport with only valid rows', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockConfirmCsvImport.mockResolvedValue({ ok: true, data: { imported: 1, skipped: 0 } })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByText(/1 valid row/i))
    fireEvent.click(screen.getByRole('button', { name: /import 1/i }))

    await waitFor(() => {
      const [rows] = mockConfirmCsvImport.mock.calls[0]
      expect(rows).toHaveLength(1)
      expect(rows[0].amount).toBe(45.5)
    })
  })

  it('navigates to /expenses after import', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    mockConfirmCsvImport.mockResolvedValue({ ok: true, data: { imported: 1, skipped: 0 } })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /import 1/i }))
    fireEvent.click(screen.getByRole('button', { name: /import 1/i }))

    await waitFor(() => { expect(mockNavigate).toHaveBeenCalledWith('/expenses') })
  })

  it('shows error on preview API failure', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: false, error: 'IMPORT_NO_FILE' })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })

    await waitFor(() => { expect(screen.getByText('IMPORT_NO_FILE')).toBeInTheDocument() })
  })

  it('shows error and stays on preview when confirm fails', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    mockConfirmCsvImport.mockResolvedValue({ ok: false, error: 'SERVER_ERROR' })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /import 1/i }))
    fireEvent.click(screen.getByRole('button', { name: /import 1/i }))

    await waitFor(() => {
      expect(screen.getByText('SERVER_ERROR')).toBeInTheDocument()
      expect(mockNavigate).not.toHaveBeenCalledWith('/expenses')
    })
  })

  it('cancel returns to upload view', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [makeFile()] } })
    await waitFor(() => screen.getByRole('button', { name: /cancel/i }))
    fireEvent.click(screen.getByRole('button', { name: /cancel/i }))

    await waitFor(() => { expect(screen.getByRole('button', { name: /drag.*drop|csv/i })).toBeInTheDocument() })
  })
})
