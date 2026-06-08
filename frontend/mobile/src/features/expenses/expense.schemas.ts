import { z } from 'zod'
import type { TFunction } from 'i18next'

export function makeExpenseSchema(t: TFunction) {
  return z
    .object({
      amount: z
        .number({ error: t('validation.required') })
        .positive(t('expenses.errors.amountPositive')),
      currencyId: z.number({ error: t('validation.required') }),
      date: z
        .string()
        .min(1, t('validation.required'))
        .regex(/^\d{4}-\d{2}-\d{2}$/, t('expenses.errors.dateFormat')),
      categoryId: z.number().optional().catch(undefined),
      subcategoryId: z.number().optional().catch(undefined),
      description: z.string().max(500, t('expenses.errors.descriptionMax')).optional(),
      familyIds: z.array(z.number()).optional(),
      tagIds: z.array(z.number()).optional(),
    })
    .refine(d => !d.subcategoryId || d.categoryId != null, {
      path: ['subcategoryId'],
      message: t('expenses.errors.subcategoryRequiresCategory'),
    })
}

export type ExpenseFormData = z.infer<ReturnType<typeof makeExpenseSchema>>
