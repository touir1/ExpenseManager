import type { Currency } from '@/features/expenses/types/expenses.type'

export type UserConfigDto = {
  defaultCurrencyId: number | null
  defaultCurrency: Currency | null
  defaultCategoryId: number | null
}

export type UpdateUserConfigRequest = {
  defaultCurrencyId?: number | null
  defaultCategoryId?: number | null
}

export type NotificationPreferenceDto = {
  eventType: string
  emailEnabled: boolean
}
