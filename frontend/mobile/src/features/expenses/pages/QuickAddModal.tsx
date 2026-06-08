import { useEffect, useState } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  IonModal,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonButton,
  IonButtons,
  IonInput,
  IonItem,
  IonLabel,
  IonSelect,
  IonSelectOption,
  IonTextarea,
  IonDatetime,
  IonToast,
  IonFab,
  IonFabButton,
  IonIcon,
  IonChip,
  IonSpinner,
  IonText,
} from '@ionic/react'
import { cameraOutline } from 'ionicons/icons'
import { useTranslation } from 'react-i18next'
import { addExpense } from '@/features/expenses/services/expensesApi.service'
import { useExpensesData } from '@/features/expenses/ExpensesDataContext'
import { useFamilies } from '@/features/families/FamilyContext'
import { useNetworkSync } from '@/hooks/useNetworkSync'
import { useOfflineQueue } from '@/hooks/useOfflineQueue'
import { makeExpenseSchema, type ExpenseFormData } from '@/features/expenses/expense.schemas'
import { useQueryClient } from '@tanstack/react-query'
import type { Category } from '@/features/expenses/types/expenses.type'

interface Props {
  isOpen: boolean
  onClose: () => void
}

export default function QuickAddModal({ isOpen, onClose }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { categories, currencies, tags } = useExpensesData()
  const { families } = useFamilies()
  const { isOnline } = useNetworkSync()
  const { enqueue } = useOfflineQueue()
  const [toast, setToast] = useState<{ message: string; color: string } | null>(null)
  const [receiptDataUrl, setReceiptDataUrl] = useState<string | null>(null)
  const [selectedTagIds, setSelectedTagIds] = useState<number[]>([])

  const schema = makeExpenseSchema(t)
  const {
    control,
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ExpenseFormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      date: new Date().toISOString().substring(0, 10),
      currencyId: currencies[0]?.id,
    },
  })

  const selectedCategoryId = watch('categoryId')
  const selectedCategory = categories.find((c: Category) => c.id === selectedCategoryId)

  useEffect(() => {
    if (isOpen) {
      reset({
        date: new Date().toISOString().substring(0, 10),
        currencyId: currencies[0]?.id,
      })
      setSelectedTagIds([])
      setReceiptDataUrl(null)
    }
  }, [isOpen, currencies, reset])

  async function capturePhoto() {
    try {
      const { Camera, CameraResultType, CameraSource } = await import('@capacitor/camera')
      const photo = await Camera.getPhoto({
        resultType: CameraResultType.DataUrl,
        source: CameraSource.Prompt,
        quality: 80,
      })
      if (photo.dataUrl) setReceiptDataUrl(photo.dataUrl)
    } catch {
      // User cancelled or camera not available
    }
  }

  async function onSubmit(data: ExpenseFormData) {
    const payload = { ...data, tagIds: selectedTagIds }
    if (!isOnline) {
      await enqueue(payload)
      setToast({ message: t('expenses.queuedOffline', 'Saved offline — will sync when reconnected.'), color: 'warning' })
      try {
        const { Haptics, ImpactStyle } = await import('@capacitor/haptics')
        await Haptics.impact({ style: ImpactStyle.Light })
      } catch { /* ignore */ }
      onClose()
      return
    }
    const res = await addExpense(payload)
    if (res.ok) {
      try {
        const { Haptics, ImpactStyle } = await import('@capacitor/haptics')
        await Haptics.impact({ style: ImpactStyle.Medium })
      } catch { /* ignore */ }
      queryClient.invalidateQueries({ queryKey: ['expenses'] })
      setToast({ message: t('expenses.addSuccess', 'Expense added!'), color: 'success' })
      onClose()
    } else {
      setToast({ message: res.error ?? t('common.error', 'Something went wrong.'), color: 'danger' })
    }
  }

  return (
    <>
      <IonModal
        isOpen={isOpen}
        onDidDismiss={onClose}
        initialBreakpoint={0.75}
        breakpoints={[0, 0.75, 1]}
      >
        <IonHeader>
          <IonToolbar>
            <IonButtons slot="start">
              <IonButton onClick={onClose}>{t('common.cancel', 'Cancel')}</IonButton>
            </IonButtons>
            <IonTitle>{t('expenses.addExpense', 'Add Expense')}</IonTitle>
            <IonButtons slot="end">
              <IonButton strong onClick={handleSubmit(onSubmit)} disabled={isSubmitting}>
                {isSubmitting ? <IonSpinner name="crescent" /> : t('common.save', 'Save')}
              </IonButton>
            </IonButtons>
          </IonToolbar>
        </IonHeader>

        <IonContent className="ion-padding">
          <IonItem>
            <IonLabel position="stacked">{t('expenses.amount', 'Amount')} *</IonLabel>
            <Controller
              name="amount"
              control={control}
              render={({ field }) => (
                <IonInput
                  type="number"
                  inputmode="decimal"
                  autofocus
                  value={field.value ?? ''}
                  onIonInput={e => field.onChange(parseFloat(String(e.detail.value)) || undefined)}
                />
              )}
            />
          </IonItem>
          {errors.amount && (
            <IonText color="danger" style={{ fontSize: 12, paddingLeft: 16 }}>
              <p>{errors.amount.message}</p>
            </IonText>
          )}

          <IonItem>
            <IonLabel position="stacked">{t('expenses.currency', 'Currency')} *</IonLabel>
            <Controller
              name="currencyId"
              control={control}
              render={({ field }) => (
                <IonSelect
                  value={field.value}
                  onIonChange={e => field.onChange(e.detail.value)}
                  interface="action-sheet"
                >
                  {currencies.map(c => (
                    <IonSelectOption key={c.id} value={c.id}>{c.code} — {c.name}</IonSelectOption>
                  ))}
                </IonSelect>
              )}
            />
          </IonItem>

          <IonItem>
            <IonLabel position="stacked">{t('expenses.date', 'Date')} *</IonLabel>
            <Controller
              name="date"
              control={control}
              render={({ field }) => (
                <IonDatetime
                  presentation="date"
                  value={field.value}
                  onIonChange={e => {
                    const v = e.detail.value
                    field.onChange(typeof v === 'string' ? v.substring(0, 10) : v?.[0]?.substring(0, 10))
                  }}
                />
              )}
            />
          </IonItem>

          <IonItem>
            <IonLabel position="stacked">{t('expenses.category', 'Category')}</IonLabel>
            <Controller
              name="categoryId"
              control={control}
              render={({ field }) => (
                <IonSelect
                  value={field.value}
                  onIonChange={e => field.onChange(e.detail.value)}
                  interface="action-sheet"
                  placeholder={t('expenses.optional', 'Optional')}
                >
                  {categories.map((c: Category) => (
                    <IonSelectOption key={c.id} value={c.id}>{c.name}</IonSelectOption>
                  ))}
                </IonSelect>
              )}
            />
          </IonItem>

          {selectedCategory && selectedCategory.subcategories.length > 0 && (
            <IonItem>
              <IonLabel position="stacked">{t('expenses.subcategory', 'Subcategory')}</IonLabel>
              <Controller
                name="subcategoryId"
                control={control}
                render={({ field }) => (
                  <IonSelect
                    value={field.value}
                    onIonChange={e => field.onChange(e.detail.value)}
                    interface="action-sheet"
                    placeholder={t('expenses.optional', 'Optional')}
                  >
                    {selectedCategory.subcategories.map(s => (
                      <IonSelectOption key={s.id} value={s.id}>{s.name}</IonSelectOption>
                    ))}
                  </IonSelect>
                )}
              />
            </IonItem>
          )}

          <IonItem>
            <IonLabel position="stacked">{t('expenses.description', 'Description')}</IonLabel>
            <IonTextarea
              rows={2}
              maxlength={500}
              {...register('description')}
            />
          </IonItem>

          <IonItem>
            <IonLabel position="stacked">{t('expenses.families', 'Families')}</IonLabel>
            <Controller
              name="familyIds"
              control={control}
              render={({ field }) => (
                <IonSelect
                  multiple
                  value={field.value}
                  onIonChange={e => field.onChange(e.detail.value)}
                  interface="alert"
                  placeholder={t('expenses.defaultFamily', 'Default family')}
                >
                  {families.filter(f => !f.isArchived).map(f => (
                    <IonSelectOption key={f.id} value={f.id}>{f.name}</IonSelectOption>
                  ))}
                </IonSelect>
              )}
            />
          </IonItem>

          {tags.length > 0 && (
            <div style={{ padding: '8px 16px' }}>
              <IonLabel style={{ fontSize: 12, color: 'var(--ion-color-medium)' }}>
                {t('expenses.tags', 'Tags')}
              </IonLabel>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, marginTop: 6 }}>
                {tags.map(tag => (
                  <IonChip
                    key={tag.id}
                    color={selectedTagIds.includes(tag.id) ? 'primary' : 'medium'}
                    onClick={() =>
                      setSelectedTagIds(prev =>
                        prev.includes(tag.id) ? prev.filter(id => id !== tag.id) : [...prev, tag.id],
                      )
                    }
                  >
                    {tag.name}
                  </IonChip>
                ))}
              </div>
            </div>
          )}

          {receiptDataUrl && (
            <div style={{ padding: '8px 16px' }}>
              <img src={receiptDataUrl} alt="receipt" style={{ width: '100%', borderRadius: 8 }} />
            </div>
          )}
        </IonContent>

        <IonFab vertical="bottom" horizontal="end" slot="fixed">
          <IonFabButton color="light" size="small" onClick={capturePhoto}>
            <IonIcon icon={cameraOutline} />
          </IonFabButton>
        </IonFab>
      </IonModal>

      <IonToast
        isOpen={!!toast}
        message={toast?.message ?? ''}
        duration={2500}
        color={toast?.color}
        onDidDismiss={() => setToast(null)}
      />
    </>
  )
}
