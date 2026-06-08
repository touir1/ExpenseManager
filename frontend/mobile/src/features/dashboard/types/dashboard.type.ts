import type { Currency, Subcategory } from '@/features/expenses/types/expenses.type'

export type { Currency, Subcategory }

export type DashboardSummaryDto = {
  totalAmount: number
  convertedTotal: number | null
  displayCurrency: Currency | null
  expenseCount: number
  previousPeriodTotal: number | null
  changePercent: number | null
  topCategory: Subcategory | null
  topCategoryAmount: number | null
}

export type CategoryAmountDto = {
  category: Subcategory | null
  amount: number
  convertedAmount: number | null
}

export type MonthlyBreakdownDto = {
  year: number
  month: number
  totalAmount: number
  convertedTotal: number | null
  byCategory: CategoryAmountDto[]
}

export type CategoryBreakdownDto = {
  category: Subcategory | null
  totalAmount: number
  convertedTotal: number | null
  percentage: number
  subcategories: CategoryAmountDto[]
}

export type SameMonthYearlyDto = {
  year: number
  totalAmount: number
  convertedTotal: number | null
}

export type CurrencyBreakdownDto = {
  currency: Currency
  totalAmount: number
  convertedAmount: number | null
  expenseCount: number
}

export type DashboardFilter = {
  familyId?: number
  dateFrom?: string
  dateTo?: string
  displayCurrencyId?: number
}
