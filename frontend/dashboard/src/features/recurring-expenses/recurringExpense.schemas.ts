import { z } from 'zod'
import type { TFunction } from 'i18next'

export function makeRecurringExpenseSchema(t: TFunction) {
  return z
    .object({
      description: z.string().min(1, t('recurringExpenses.errors.descriptionRequired')).max(500),
      amount: z
        .number({ error: t('validation.required') })
        .positive(t('recurringExpenses.errors.amountPositive')),
      currencyId: z.number({ error: t('validation.required') }),
      categoryId: z.number({ error: t('validation.required') }),
      subcategoryId: z.number().optional().catch(undefined),
      familyId: z.number().optional().catch(undefined),
      frequencyId: z.number({ error: t('validation.required') }),
      nextDueDate: z
        .string()
        .min(1, t('validation.required'))
        .regex(/^\d{4}-\d{2}-\d{2}$/, t('expenses.errors.dateFormat')),
      autoCreate: z.boolean().catch(false),
      isActive: z.boolean().catch(true),
    })
    .refine(d => !d.subcategoryId || d.categoryId != null, {
      path: ['subcategoryId'],
      message: t('expenses.errors.subcategoryRequiresCategory'),
    })
}

export type RecurringExpenseFormData = z.infer<ReturnType<typeof makeRecurringExpenseSchema>>
