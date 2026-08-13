import { get, post, put, del } from '@/services/api.service'
import type { ApiResponse } from '@/types/api.type'
import type { RecurringExpenseDto, RecurringExpenseRequest } from '@/features/dashboard/types/dashboard.type'
import type { ExpenseDto } from '@/features/expenses/types/expenses.type'

const BASE = '/api/expenses/recurring-expenses'

export function getAll(includeInactive = false): Promise<ApiResponse<RecurringExpenseDto[]>> {
  return get<RecurringExpenseDto[]>(`${BASE}?includeInactive=${includeInactive}`)
}

export function getById(id: number): Promise<ApiResponse<RecurringExpenseDto>> {
  return get<RecurringExpenseDto>(`${BASE}/${id}`)
}

export function create(req: RecurringExpenseRequest): Promise<ApiResponse<RecurringExpenseDto>> {
  return post<RecurringExpenseDto>(BASE, req)
}

export function update(id: number, req: RecurringExpenseRequest): Promise<ApiResponse<RecurringExpenseDto>> {
  return put<RecurringExpenseDto>(`${BASE}/${id}`, req)
}

export function remove(id: number): Promise<ApiResponse<void>> {
  return del<void>(`${BASE}/${id}`)
}

export function confirm(id: number): Promise<ApiResponse<ExpenseDto>> {
  return post<ExpenseDto>(`${BASE}/${id}/confirm`, {})
}
