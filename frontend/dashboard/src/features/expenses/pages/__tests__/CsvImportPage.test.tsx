import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent, act } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import CsvImportPage from '../CsvImportPage'
import type { CsvImportPreviewDto } from '@/features/expenses/types/expenses.type'

// ── Mocks ─────────────────────────────────────────────────────────────────────

const mockNavigate = vi.fn()
let mockBlocker: { state: string; proceed?: () => void; reset?: () => void } = { state: 'idle' }
const mockProceed = vi.fn()
const mockReset = vi.fn()

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { ...actual, useNavigate: () => mockNavigate, useBlocker: () => mockBlocker }
})

const mockPreviewCsvImport = vi.fn()
const mockConfirmCsvImport = vi.fn()
const mockValidateCsvRows = vi.fn()
const mockGetImportTemplateUrl = vi.fn().mockReturnValue('/api/expenses/import/template')
const mockDetectCsvHeaders = vi.fn()
const mockRefresh = vi.fn()

vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  previewCsvImport: (...args: unknown[]) => mockPreviewCsvImport(...args),
  confirmCsvImport: (...args: unknown[]) => mockConfirmCsvImport(...args),
  validateCsvRows: (...args: unknown[]) => mockValidateCsvRows(...args),
  getImportTemplateUrl: () => mockGetImportTemplateUrl(),
  detectCsvHeaders: (...args: unknown[]) => mockDetectCsvHeaders(...args),
}))

const mockUpdateDefaultCsvColumnMapping = vi.fn()

vi.mock('@/features/settings/services/userConfigApi.service', () => ({
  updateDefaultCsvColumnMapping: (...args: unknown[]) => mockUpdateDefaultCsvColumnMapping(...args),
  clearDefaultCsvColumnMapping: vi.fn(),
  getConfig: vi.fn(),
  updateConfig: vi.fn(),
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
    mockBlocker = { state: 'idle' }
    mockGetImportTemplateUrl.mockReturnValue('/api/expenses/import/template')
    mockUpdateDefaultCsvColumnMapping.mockResolvedValue({ ok: true, status: 200, data: {} })
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

  // ── Item 1: Upload Spinner ────────────────────────────────────────────────────

  it('shows spinner and "Uploading file…" text while file is processing', async () => {
    let resolveUpload!: (v: unknown) => void
    mockPreviewCsvImport.mockReturnValue(new Promise(r => { resolveUpload = r }))
    renderPage()
    uploadFile()

    await waitFor(() => {
      expect(screen.getByText(/uploading file/i)).toBeInTheDocument()
    })

    resolveUpload({ ok: true, data: previewAllValid })
  })

  it('dropzone has pointer-events-none class while uploading', async () => {
    let resolveUpload!: (v: unknown) => void
    mockPreviewCsvImport.mockReturnValue(new Promise(r => { resolveUpload = r }))
    renderPage()
    uploadFile()

    await waitFor(() => screen.getByText(/uploading file/i))
    const dropzone = screen.getByRole('button', { name: /drag.*drop|csv/i })
    expect(dropzone).toHaveClass('pointer-events-none')

    resolveUpload({ ok: true, data: previewAllValid })
  })

  it('hides spinner and shows preview table after upload resolves', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()
    uploadFile()

    await waitFor(() => {
      expect(screen.queryByText(/uploading file/i)).not.toBeInTheDocument()
      expect(screen.getByText('2025-01-15')).toBeInTheDocument()
    })
  })

  // ── Item 2: Navigation Warning ────────────────────────────────────────────────

  it('shows leave confirmation modal when blocker is triggered', async () => {
    mockBlocker = { state: 'blocked', proceed: mockProceed, reset: mockReset }
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    await waitFor(() => {
      expect(screen.getByText(/leave import\?/i)).toBeInTheDocument()
      expect(screen.getByText(/progress will be lost/i)).toBeInTheDocument()
    })
  })

  it('leave modal has "Leave anyway" and "Stay on page" buttons', async () => {
    mockBlocker = { state: 'blocked', proceed: mockProceed, reset: mockReset }
    renderPage()

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /leave anyway/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /stay on page/i })).toBeInTheDocument()
    })
  })

  it('"Leave anyway" calls blocker.proceed()', async () => {
    mockBlocker = { state: 'blocked', proceed: mockProceed, reset: mockReset }
    renderPage()

    await waitFor(() => screen.getByRole('button', { name: /leave anyway/i }))
    fireEvent.click(screen.getByRole('button', { name: /leave anyway/i }))

    expect(mockProceed).toHaveBeenCalledOnce()
  })

  it('"Stay on page" calls blocker.reset()', async () => {
    mockBlocker = { state: 'blocked', proceed: mockProceed, reset: mockReset }
    renderPage()

    await waitFor(() => screen.getByRole('button', { name: /stay on page/i }))
    fireEvent.click(screen.getByRole('button', { name: /stay on page/i }))

    expect(mockReset).toHaveBeenCalledOnce()
  })

  it('leave modal does not show when blocker is idle', () => {
    renderPage()

    expect(screen.queryByText(/leave import\?/i)).not.toBeInTheDocument()
  })

  // ── Item 3: Edited Badge ──────────────────────────────────────────────────────

  it('shows "Edited" badge on row after save+validate succeeds', async () => {
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
      expect(screen.getByText(/edited/i)).toBeInTheDocument()
    })
  })

  it('does not show "Edited" badge on row that was never edited', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockValidateCsvRows.mockResolvedValue(makeValidateResponse(validatedRow2))
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))
    fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))

    await waitFor(() => screen.getByText(/edited/i))

    expect(screen.getAllByText(/edited/i)).toHaveLength(1)
  })

  it('"Edited" badge clears when the row is removed', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockValidateCsvRows.mockResolvedValue(makeValidateResponse(validatedRow2))
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))
    fireEvent.change(screen.getByLabelText(/row 2 amount/i), { target: { value: '25.00' } })
    fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))
    await waitFor(() => screen.getByText(/edited/i))

    fireEvent.click(screen.getByRole('button', { name: /remove row 2/i }))

    await waitFor(() => {
      expect(screen.queryByText(/edited/i)).not.toBeInTheDocument()
    })
  })

  it('"Edited" badge does not appear while row is still in editing state', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

    fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
    await waitFor(() => screen.getByLabelText(/row 2 amount/i))

    expect(screen.queryByText(/edited/i)).not.toBeInTheDocument()
  })

  // ── Item 4: Sort Toggle ───────────────────────────────────────────────────────

  it('shows sort toggle button in preview header area', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /errors first/i })).toBeInTheDocument()
    })
  })

  it('clicking "Errors first" puts error rows before valid rows in DOM', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /errors first/i }))

    fireEvent.click(screen.getByRole('button', { name: /errors first/i }))

    await waitFor(() => {
      const rows = screen.getAllByRole('row')
      expect(rows[1]).toHaveTextContent('2')
      expect(rows[2]).toHaveTextContent('1')
    })
  })

  it('clicking "Row order" after sort restores original order', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /errors first/i }))

    fireEvent.click(screen.getByRole('button', { name: /errors first/i }))
    await waitFor(() => screen.getByRole('button', { name: /row order/i }))

    fireEvent.click(screen.getByRole('button', { name: /row order/i }))

    await waitFor(() => {
      const rows = screen.getAllByRole('row')
      expect(rows[1]).toHaveTextContent('1')
      expect(rows[2]).toHaveTextContent('2')
    })
  })

  it('sort resets to row order when a new file is uploaded', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()
    uploadFile()
    await waitFor(() => screen.getByRole('button', { name: /errors first/i }))

    fireEvent.click(screen.getByRole('button', { name: /errors first/i }))
    await waitFor(() => screen.getByRole('button', { name: /row order/i }))

    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))
    await waitFor(() => expect(document.querySelector('input[type="file"]')).toBeInTheDocument())

    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    uploadFile()

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /errors first/i })).toBeInTheDocument()
    })
  })

  // ── Item 5: Template Description ─────────────────────────────────────────────

  it('shows template column description text alongside download link', () => {
    renderPage()
    expect(screen.getByText(/expected columns/i)).toBeInTheDocument()
  })

  // ── Column mapping step ──────────────────────────────────────────────────────

  describe('column mapping step', () => {
    it('shows column mapping step when preview fails with MISSING_HEADERS and detect-headers succeeds', async () => {
      mockPreviewCsvImport.mockResolvedValueOnce({ ok: false, status: 400, error: 'CSV is missing required columns', rawCode: 'MISSING_HEADERS:date,amount,currency_code' })
      mockDetectCsvHeaders.mockResolvedValue({
        ok: true, status: 200,
        data: { rawHeaders: ['sum', 'cur', 'tx_date'], suggestedMapping: { sum: 'amount', cur: 'currency_code', tx_date: 'date' }, headersMatchExactly: false },
      })
      renderPage()
      uploadFile()

      await waitFor(() => {
        expect(screen.getByText(/match your csv columns/i)).toBeInTheDocument()
      })
      expect(screen.getByText('sum')).toBeInTheDocument()
      expect(screen.getByText('cur')).toBeInTheDocument()
      expect(screen.getByText('tx_date')).toBeInTheDocument()
    })

    it('does not show mapping step when headers match (normal upload flow unaffected)', async () => {
      mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
      renderPage()
      uploadFile()

      await waitFor(() => { expect(screen.getByText('2025-01-15')).toBeInTheDocument() })
      expect(mockDetectCsvHeaders).not.toHaveBeenCalled()
    })

    it('disables Continue button when a required canonical field is unmapped', async () => {
      mockPreviewCsvImport.mockResolvedValueOnce({ ok: false, status: 400, error: 'CSV is missing required columns', rawCode: 'MISSING_HEADERS:date,amount,currency_code' })
      mockDetectCsvHeaders.mockResolvedValue({
        ok: true, status: 200,
        data: { rawHeaders: ['sum', 'cur'], suggestedMapping: { sum: 'amount', cur: 'currency_code' }, headersMatchExactly: false },
      })
      renderPage()
      uploadFile()

      await waitFor(() => screen.getByRole('button', { name: /continue/i }))
      expect(screen.getByRole('button', { name: /continue/i })).toBeDisabled()
      expect(screen.getByRole('alert')).toBeInTheDocument()
    })

    it('clicking Continue resubmits file with confirmed columnMapping and shows preview table on success', async () => {
      mockPreviewCsvImport.mockResolvedValueOnce({ ok: false, status: 400, error: 'CSV is missing required columns', rawCode: 'MISSING_HEADERS:date,amount,currency_code' })
      mockDetectCsvHeaders.mockResolvedValue({
        ok: true, status: 200,
        data: { rawHeaders: ['sum', 'cur', 'tx_date'], suggestedMapping: { sum: 'amount', cur: 'currency_code', tx_date: 'date' }, headersMatchExactly: false },
      })
      mockPreviewCsvImport.mockResolvedValueOnce({ ok: true, data: previewAllValid })
      renderPage()
      uploadFile()

      await waitFor(() => screen.getByRole('button', { name: /continue/i }))
      fireEvent.click(screen.getByRole('button', { name: /continue/i }))

      await waitFor(() => {
        expect(mockPreviewCsvImport).toHaveBeenCalledTimes(2)
        expect(mockPreviewCsvImport.mock.calls[1][1]).toEqual({ sum: 'amount', cur: 'currency_code', tx_date: 'date' })
      })
      await waitFor(() => { expect(screen.getByText('2025-01-15')).toBeInTheDocument() })
      expect(mockUpdateDefaultCsvColumnMapping).toHaveBeenCalledWith({ sum: 'amount', cur: 'currency_code', tx_date: 'date' })
    })

    it('unchecking "Remember this mapping" does not save a default mapping on Continue', async () => {
      mockPreviewCsvImport.mockResolvedValueOnce({ ok: false, status: 400, error: 'CSV is missing required columns', rawCode: 'MISSING_HEADERS:date,amount,currency_code' })
      mockDetectCsvHeaders.mockResolvedValue({
        ok: true, status: 200,
        data: { rawHeaders: ['sum', 'cur', 'tx_date'], suggestedMapping: { sum: 'amount', cur: 'currency_code', tx_date: 'date' }, headersMatchExactly: false },
      })
      mockPreviewCsvImport.mockResolvedValueOnce({ ok: true, data: previewAllValid })
      renderPage()
      uploadFile()

      await waitFor(() => screen.getByRole('checkbox', { name: /remember this mapping/i }))
      fireEvent.click(screen.getByRole('checkbox', { name: /remember this mapping/i }))
      fireEvent.click(screen.getByRole('button', { name: /continue/i }))

      await waitFor(() => { expect(screen.getByText('2025-01-15')).toBeInTheDocument() })
      expect(mockUpdateDefaultCsvColumnMapping).not.toHaveBeenCalled()
    })

    it('Cancel on mapping step returns to the dropzone and clears mapping state', async () => {
      mockPreviewCsvImport.mockResolvedValueOnce({ ok: false, status: 400, error: 'CSV is missing required columns', rawCode: 'MISSING_HEADERS:date,amount,currency_code' })
      mockDetectCsvHeaders.mockResolvedValue({
        ok: true, status: 200,
        data: { rawHeaders: ['sum', 'cur'], suggestedMapping: { sum: 'amount', cur: 'currency_code' }, headersMatchExactly: false },
      })
      renderPage()
      uploadFile()

      await waitFor(() => screen.getByText(/match your csv columns/i))
      fireEvent.click(screen.getByRole('button', { name: 'Cancel' }))

      await waitFor(() => {
        expect(document.querySelector('input[type="file"]')).toBeInTheDocument()
        expect(screen.queryByText(/match your csv columns/i)).not.toBeInTheDocument()
      })
    })
  })
})
