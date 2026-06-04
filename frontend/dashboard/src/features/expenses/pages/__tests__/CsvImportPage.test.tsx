import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent, act } from '@testing-library/react'
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
const mockRefresh = vi.fn()

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
    refresh: mockRefresh,
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

const validatedRow2 = {
  ...errorRow,
  rowNumber: 2,
  isValid: true,
  errors: [],
  amountDisplay: '25.00',
  amount: 25,
  currencyId: 1,
  date: '2025-01-20',
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

// single-row validate response shape expected by saveAndValidateRow
function makeValidateResponse(row: typeof validRow | typeof errorRow): { ok: true; data: CsvImportPreviewDto } {
  return { ok: true, data: { totalRows: 1, validCount: row.isValid ? 1 : 0, errorCount: row.isValid ? 0 : 1, rows: [row] } }
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

async function uploadFile(file = makeFile()) {
  fireEvent.change(document.querySelector('input[type="file"]') as HTMLInputElement, { target: { files: [file] } })
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
    uploadFile()
    await waitFor(() => { expect(screen.getByText('2025-01-15')).toBeInTheDocument() })
  })

  it('shows all 8 CSV columns in table header', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()
    uploadFile()
    await waitFor(() => {
      expect(screen.getByText(/subcategory/i)).toBeInTheDocument()
      expect(screen.getByText(/tags/i)).toBeInTheDocument()
      expect(screen.getByText(/families/i)).toBeInTheDocument()
    })
  })

  it('shows tags as chips in display mode', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()
    uploadFile()
    await waitFor(() => { expect(screen.getByText('work')).toBeInTheDocument() })
  })

  it('shows "—" for empty families in display mode', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()
    uploadFile()
    await waitFor(() => { expect(screen.getByText('—')).toBeInTheDocument() })
  })

  it('does not show default family in FamilyMultiSelect dropdown', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
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
    uploadFile()
    await waitFor(() => {
      expect(screen.getByText(/1 valid row/i)).toBeInTheDocument()
      expect(screen.getByText(/1 row.*error/i)).toBeInTheDocument()
    })
  })

  it('shows error codes in status column', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => { expect(screen.getByText(/Invalid amount/i)).toBeInTheDocument() })
  })

  it('shows edit and remove buttons for each row', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /edit row 1/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /edit row 2/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /remove row 1/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /remove row 2/i })).toBeInTheDocument()
    })
  })

  it('clicking remove removes the row from the table', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /remove row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /remove row 2/i }))

    await waitFor(() => {
      expect(screen.queryByRole('button', { name: /remove row 2/i })).not.toBeInTheDocument()
      expect(screen.getByRole('button', { name: /remove row 1/i })).toBeInTheDocument()
    })
  })

  it('clicking remove updates valid and error count badges', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByText(/1 row.*error/i))

    fireEvent.click(screen.getByRole('button', { name: /remove row 2/i }))

    await waitFor(() => {
      expect(screen.queryByText(/1 row.*error/i)).not.toBeInTheDocument()
      expect(screen.getByText(/1 valid row/i)).toBeInTheDocument()
    })
  })

  it('clicking remove on valid row decrements valid count', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByText(/1 valid row/i))

    fireEvent.click(screen.getByRole('button', { name: /remove row 1/i }))

    await waitFor(() => {
      expect(screen.queryByText(/1 valid row/i)).not.toBeInTheDocument()
    })
  })

  it('shows editable inputs after clicking edit', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
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
    uploadFile()
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
    uploadFile()
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

  it('save triggers immediate single-row validateCsvRows call', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockValidateCsvRows.mockResolvedValue(makeValidateResponse(validatedRow2))
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))
    fireEvent.change(screen.getByLabelText(/row 2 amount/i), { target: { value: '25.00' } })
    fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))

    await waitFor(() => {
      expect(mockValidateCsvRows).toHaveBeenCalledOnce()
      const [rows] = mockValidateCsvRows.mock.calls[0]
      expect(rows).toHaveLength(1)
      expect(rows[0].rowNumber).toBe(2)
      expect(rows[0].amount).toBe('25.00')
    })
  })

  it('tags serialized as semicolons in per-row validate call', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    mockValidateCsvRows.mockResolvedValue(makeValidateResponse(validRow))
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /edit row 1/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 1/i }))
    await waitFor(() => screen.getByLabelText(/row 1 tags/i))
    fireEvent.click(screen.getByRole('button', { name: /save row 1/i }))

    await waitFor(() => {
      const [rows] = mockValidateCsvRows.mock.calls[0]
      expect(rows[0].tags).toBe('work')
    })
  })

  it('updates row in preview after per-row validation succeeds', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockValidateCsvRows.mockResolvedValue(makeValidateResponse(validatedRow2))
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))
    fireEvent.change(screen.getByLabelText(/row 2 amount/i), { target: { value: '25.00' } })
    fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))

    await waitFor(() => {
      // row 2 now valid → error badge gone, 2 valid rows
      expect(screen.queryByText(/1 row.*error/i)).not.toBeInTheDocument()
      expect(screen.getByText(/2 valid rows/i)).toBeInTheDocument()
    })
  })

  it('import button enabled after per-row validation clears all errors', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockValidateCsvRows.mockResolvedValue(makeValidateResponse(validatedRow2))
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))
    fireEvent.change(screen.getByLabelText(/row 2 amount/i), { target: { value: '25.00' } })
    fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /import 2/i })).not.toBeDisabled()
    })
  })

  it('import button absent only when no valid rows (not import button)', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => {
      expect(screen.queryByRole('button', { name: /re-validate/i })).not.toBeInTheDocument()
      expect(screen.getByRole('button', { name: /import/i })).toBeInTheDocument()
    })
  })

  it('no re-validate button shown when all rows valid and no edits', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()
    uploadFile()
    await waitFor(() => { expect(screen.queryByRole('button', { name: /re-validate/i })).not.toBeInTheDocument() })
  })

  it('disables confirm when no valid rows', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllInvalid })
    renderPage()
    uploadFile()
    await waitFor(() => { expect(screen.getByRole('button', { name: /import 0/i })).toBeDisabled() })
  })

  it('calls confirmCsvImport with only valid rows', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockConfirmCsvImport.mockResolvedValue({ ok: true, data: { imported: 1, skipped: 0 } })
    renderPage()
    uploadFile()
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
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /import 1/i }))
    fireEvent.click(screen.getByRole('button', { name: /import 1/i }))

    await waitFor(() => { expect(mockNavigate).toHaveBeenCalledWith('/expenses') })
  })

  it('calls refresh after successful import', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    mockConfirmCsvImport.mockResolvedValue({ ok: true, data: { imported: 1, skipped: 0 } })
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /import 1/i }))
    fireEvent.click(screen.getByRole('button', { name: /import 1/i }))

    await waitFor(() => { expect(mockRefresh).toHaveBeenCalledOnce() })
  })

  it('does not call refresh when import fails', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    mockConfirmCsvImport.mockResolvedValue({ ok: false, error: 'SERVER_ERROR' })
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /import 1/i }))
    fireEvent.click(screen.getByRole('button', { name: /import 1/i }))

    await waitFor(() => { expect(screen.getByText('SERVER_ERROR')).toBeInTheDocument() })
    expect(mockRefresh).not.toHaveBeenCalled()
  })

  it('shows error on preview API failure', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: false, error: 'IMPORT_NO_FILE' })
    renderPage()
    uploadFile()
    await waitFor(() => { expect(screen.getByText('IMPORT_NO_FILE')).toBeInTheDocument() })
  })

  it('shows error and stays on preview when confirm fails', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    mockConfirmCsvImport.mockResolvedValue({ ok: false, error: 'SERVER_ERROR' })
    renderPage()
    uploadFile()
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
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /cancel/i }))

    // click the page-level cancel (not row-level cancel editing)
    const cancelBtn = screen.getAllByRole('button', { name: /cancel/i })
      .find(b => !b.getAttribute('aria-label')?.includes('editing'))
    fireEvent.click(cancelBtn!)

    await waitFor(() => { expect(screen.getByRole('button', { name: /drag.*drop|csv/i })).toBeInTheDocument() })
  })

  it('shows error and does not call API when file exceeds 1 MB', async () => {
    renderPage()
    const bigFile = new File([new ArrayBuffer(2 * 1024 * 1024)], 'big.csv', { type: 'text/csv' })
    uploadFile(bigFile)

    await waitFor(() => { expect(screen.getByText(/1 MB/i)).toBeInTheDocument() })
    expect(mockPreviewCsvImport).not.toHaveBeenCalled()
  })

  it('shows error and does not call API when file has wrong extension', async () => {
    renderPage()
    const wrongFile = new File(['data'], 'upload.exe', { type: 'text/csv' })
    uploadFile(wrongFile)

    await waitFor(() => { expect(screen.getByText(/only csv files are accepted/i)).toBeInTheDocument() })
    expect(mockPreviewCsvImport).not.toHaveBeenCalled()
  })
})
