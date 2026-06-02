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
const mockGetImportTemplateUrl = vi.fn().mockReturnValue('/api/expenses/import/template')

vi.mock('@/features/expenses/services/expensesApi.service', () => ({
  previewCsvImport: (...args: unknown[]) => mockPreviewCsvImport(...args),
  confirmCsvImport: (...args: unknown[]) => mockConfirmCsvImport(...args),
  getImportTemplateUrl: () => mockGetImportTemplateUrl(),
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
  date: null,
  amount: null,
  currencyId: null,
  categoryId: null,
  subcategoryId: null,
  familyIds: null,
}

const previewWithErrors: CsvImportPreviewDto = {
  totalRows: 2,
  validCount: 1,
  errorCount: 1,
  rows: [validRow, errorRow],
}

const previewAllValid: CsvImportPreviewDto = {
  totalRows: 1,
  validCount: 1,
  errorCount: 0,
  rows: [validRow],
}

const previewAllInvalid: CsvImportPreviewDto = {
  totalRows: 1,
  validCount: 0,
  errorCount: 1,
  rows: [errorRow],
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
    const link = screen.getByRole('link', { name: /template/i })
    expect(link).toHaveAttribute('href', '/api/expenses/import/template')
  })

  it('shows preview table after file upload', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByText('2025-01-15')).toBeInTheDocument()
    })
  })

  it('shows valid count and error count badges', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByText(/1 valid row/i)).toBeInTheDocument()
      expect(screen.getByText(/1 row.*error/i)).toBeInTheDocument()
    })
  })

  it('highlights error rows differently from valid rows', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    renderPage()

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [makeFile()] } })

    await waitFor(() => {
      const errorCell = screen.getByText(/Invalid amount/i)
      expect(errorCell).toBeInTheDocument()
    })
  })

  it('disables confirm button when no valid rows', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllInvalid })
    renderPage()

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [makeFile()] } })

    await waitFor(() => {
      const confirmBtn = screen.getByRole('button', { name: /import 0/i })
      expect(confirmBtn).toBeDisabled()
    })
  })

  it('calls confirmCsvImport with only valid rows on confirm', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
    mockConfirmCsvImport.mockResolvedValue({ ok: true, data: { imported: 1, skipped: 0 } })
    renderPage()

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByText(/1 valid row/i)).toBeInTheDocument()
    })

    fireEvent.click(screen.getByRole('button', { name: /import 1/i }))

    await waitFor(() => {
      expect(mockConfirmCsvImport).toHaveBeenCalledOnce()
      const [rows] = mockConfirmCsvImport.mock.calls[0]
      expect(rows).toHaveLength(1)
      expect(rows[0].amount).toBe(45.5)
    })
  })

  it('navigates to /expenses after successful import', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    mockConfirmCsvImport.mockResolvedValue({ ok: true, data: { imported: 1, skipped: 0 } })
    renderPage()

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /import 1/i })).toBeInTheDocument()
    })

    fireEvent.click(screen.getByRole('button', { name: /import 1/i }))

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/expenses')
    })
  })

  it('shows error message on preview API failure', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: false, error: 'IMPORT_NO_FILE' })
    renderPage()

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByText('IMPORT_NO_FILE')).toBeInTheDocument()
    })
  })

  it('shows error and stays on preview when confirm fails', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    mockConfirmCsvImport.mockResolvedValue({ ok: false, error: 'SERVER_ERROR' })
    renderPage()

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /import 1/i })).toBeInTheDocument()
    })

    fireEvent.click(screen.getByRole('button', { name: /import 1/i }))

    await waitFor(() => {
      expect(screen.getByText('SERVER_ERROR')).toBeInTheDocument()
      expect(mockNavigate).not.toHaveBeenCalledWith('/expenses')
    })
  })

  it('returns to upload view when cancel is clicked in preview', async () => {
    mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
    renderPage()

    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement
    fireEvent.change(fileInput, { target: { files: [makeFile()] } })

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument()
    })

    fireEvent.click(screen.getByRole('button', { name: /cancel/i }))

    await waitFor(() => {
      expect(screen.getByRole('button', { name: /drag.*drop|csv/i })).toBeInTheDocument()
    })
  })
})
