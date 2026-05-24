export type Subcategory = {
  id: number
  name: string
  description?: string
}

export type Category = {
  id: number
  name: string
  description?: string
  subcategories: Subcategory[]
}

export type Currency = {
  id: number
  code: string
  name: string
  symbol: string
  decimals: number
}

export type TagDto = {
  id: number
  name: string
}

export type FamilyNameDto = {
  id: number
  name: string
}

export type ExpenseDto = {
  id: number
  amount: number
  currency: Currency | null
  date: string
  category: Subcategory | null
  subcategory: Subcategory | null
  description: string | null
  createdAt: string
  modifiedAt: string | null
  modifiedFrom: string | null
  tags: TagDto[]
  families?: FamilyNameDto[]
  convertedAmount: number | null
  displayCurrency: Currency | null
}

export type ExpensePagedResponse = {
  items: ExpenseDto[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export type ExpenseFilter = {
  dateFrom?: string
  dateTo?: string
  categoryId?: number
  subcategoryId?: number
  currencyId?: number
  amountMin?: number
  amountMax?: number
  description?: string
  tagIds?: number[]
  displayCurrencyId?: number
  page?: number
  pageSize?: number
}

export type ExpenseRequest = {
  amount: number
  currencyId: number
  date: string
  categoryId?: number
  subcategoryId?: number
  description?: string
  familyIds?: number[]
  tagIds?: number[]
}
