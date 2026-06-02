import { get, post, put, del, postFormData } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type {
  ExpenseDto,
  ExpenseFilter,
  ExpensePagedResponse,
  ExpenseRequest,
  CsvImportPreviewDto,
  CsvImportConfirmRowDto,
  CsvImportResultDto,
  RawCsvRowDto,
} from '@/features/expenses/types/expenses.type'

const BASE = '/api/expenses'

export function getExpenses(filter: ExpenseFilter = {}): Promise<ApiResponse<ExpensePagedResponse>> {
  const params = new URLSearchParams()
  if (filter.dateFrom) params.set('dateFrom', filter.dateFrom)
  if (filter.dateTo) params.set('dateTo', filter.dateTo)
  if (filter.categoryId != null) params.set('categoryId', String(filter.categoryId))
  if (filter.subcategoryId != null) params.set('subcategoryId', String(filter.subcategoryId))
  if (filter.currencyId != null) params.set('currencyId', String(filter.currencyId))
  if (filter.amountMin != null) params.set('amountMin', String(filter.amountMin))
  if (filter.amountMax != null) params.set('amountMax', String(filter.amountMax))
  if (filter.description) params.set('description', filter.description)
  if (filter.tagIds?.length) filter.tagIds.forEach(id => params.append('tagIds', String(id)))
  if (filter.displayCurrencyId != null) params.set('displayCurrencyId', String(filter.displayCurrencyId))
  if (filter.page != null) params.set('page', String(filter.page))
  if (filter.pageSize != null) params.set('pageSize', String(filter.pageSize))
  const qs = params.toString()
  return get<ExpensePagedResponse>(qs ? `${BASE}?${qs}` : BASE)
}

export function getExpenseById(
  id: number,
  displayCurrencyId?: number,
): Promise<ApiResponse<ExpenseDto>> {
  const url = displayCurrencyId != null
    ? `${BASE}/${id}?displayCurrencyId=${displayCurrencyId}`
    : `${BASE}/${id}`
  return get<ExpenseDto>(url)
}

export function addExpense(req: ExpenseRequest): Promise<ApiResponse<ExpenseDto>> {
  return post<ExpenseDto>(BASE, req)
}

export function updateExpense(id: number, req: ExpenseRequest): Promise<ApiResponse<ExpenseDto>> {
  return put<ExpenseDto>(`${BASE}/${id}`, req)
}

export function deleteExpense(id: number): Promise<ApiResponse<void>> {
  return del<void>(`${BASE}/${id}`)
}

export function previewCsvImport(file: File): Promise<ApiResponse<CsvImportPreviewDto>> {
  const fd = new FormData()
  fd.append('file', file)
  return postFormData<CsvImportPreviewDto>(`${BASE}/import/preview`, fd)
}

export function confirmCsvImport(rows: CsvImportConfirmRowDto[]): Promise<ApiResponse<CsvImportResultDto>> {
  return post<CsvImportResultDto>(`${BASE}/import/confirm`, { rows })
}

export function getImportTemplateUrl(): string {
  const base = (import.meta.env.VITE_API_BASE ?? '').replace(/\/$/, '')
  return `${base}${BASE}/import/template`
}

export function validateCsvRows(rows: RawCsvRowDto[]): Promise<ApiResponse<CsvImportPreviewDto>> {
  return post<CsvImportPreviewDto>(`${BASE}/import/validate-rows`, { rows })
}
