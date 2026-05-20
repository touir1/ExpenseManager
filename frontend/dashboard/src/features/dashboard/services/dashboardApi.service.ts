import { get } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { ExpensePagedResponse } from '@/features/expenses/types/expenses.type'
import type {
  DashboardSummaryDto,
  MonthlyBreakdownDto,
  CategoryBreakdownDto,
  SameMonthYearlyDto,
  CurrencyBreakdownDto,
  DashboardFilter,
} from '@/features/dashboard/types/dashboard.type'

const BASE = '/api/expenses/dashboard'

function buildParams(filter: DashboardFilter): URLSearchParams {
  const p = new URLSearchParams()
  if (filter.familyId != null) p.set('familyId', String(filter.familyId))
  if (filter.dateFrom) p.set('dateFrom', filter.dateFrom)
  if (filter.dateTo) p.set('dateTo', filter.dateTo)
  if (filter.displayCurrencyId != null) p.set('displayCurrencyId', String(filter.displayCurrencyId))
  return p
}

function url(path: string, params: URLSearchParams): string {
  const qs = params.toString()
  return qs ? `${BASE}${path}?${qs}` : `${BASE}${path}`
}

export function getSummary(filter: DashboardFilter = {}): Promise<ApiResponse<DashboardSummaryDto>> {
  return get<DashboardSummaryDto>(url('/summary', buildParams(filter)))
}

export function getMonthly(filter: DashboardFilter = {}): Promise<ApiResponse<MonthlyBreakdownDto[]>> {
  return get<MonthlyBreakdownDto[]>(url('/monthly', buildParams(filter)))
}

export function getCategories(filter: DashboardFilter = {}): Promise<ApiResponse<CategoryBreakdownDto[]>> {
  return get<CategoryBreakdownDto[]>(url('/categories', buildParams(filter)))
}

export function getSameMonthYearly(
  month: number,
  familyId?: number,
  displayCurrencyId?: number,
): Promise<ApiResponse<SameMonthYearlyDto[]>> {
  const p = new URLSearchParams()
  p.set('month', String(month))
  if (familyId != null) p.set('familyId', String(familyId))
  if (displayCurrencyId != null) p.set('displayCurrencyId', String(displayCurrencyId))
  return get<SameMonthYearlyDto[]>(`${BASE}/same-month-across-years?${p.toString()}`)
}

export function getByCurrency(filter: DashboardFilter = {}): Promise<ApiResponse<CurrencyBreakdownDto[]>> {
  return get<CurrencyBreakdownDto[]>(url('/by-currency', buildParams(filter)))
}

export function getRecent(filter: DashboardFilter = {}): Promise<ApiResponse<ExpensePagedResponse>> {
  return get<ExpensePagedResponse>(url('/recent', buildParams(filter)))
}
