import type { Currency } from '@/features/expenses/types/expenses.type'

export type UserConfigDto = {
  defaultCurrencyId: number | null
  defaultCurrency: Currency | null
}

export type UpdateUserConfigRequest = {
  defaultCurrencyId: number | null
}
