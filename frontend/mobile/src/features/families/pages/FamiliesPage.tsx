import { useState } from 'react'
import {
  IonPage,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonList,
  IonItem,
  IonLabel,
  IonBadge,
  IonButton,
  IonButtons,
  IonCard,
  IonCardContent,
  IonCardHeader,
  IonCardTitle,
  IonAlert,
  IonInput,
  IonModal,
  IonText,
  IonToast,
} from '@ionic/react'
import { useTranslation } from 'react-i18next'
import { useFamilies } from '@/features/families/FamilyContext'
import {
  createFamily,
  inviteMember,
  leaveFamily,
  getFamilyById,
} from '@/features/families/services/familyApi.service'
import type { Family, FamilyDetail } from '@/features/families/types/family.type'

export default function FamiliesPage() {
  const { t } = useTranslation()
  const { families, refresh } = useFamilies()
  const [createOpen, setCreateOpen] = useState(false)
  const [newFamilyName, setNewFamilyName] = useState('')
  const [inviteOpen, setInviteOpen] = useState<number | null>(null)
  const [inviteEmail, setInviteEmail] = useState('')
  const [expandedId, setExpandedId] = useState<number | null>(null)
  const [expandedDetail, setExpandedDetail] = useState<FamilyDetail | null>(null)
  const [leaveTarget, setLeaveTarget] = useState<number | null>(null)
  const [toast, setToast] = useState<{ message: string; color: string } | null>(null)

  const activeFamilies = families.filter((f: Family) => !f.isArchived)

  async function handleCreate() {
    if (!newFamilyName.trim()) return
    const res = await createFamily(newFamilyName.trim())
    if (res.ok) {
      setNewFamilyName('')
      setCreateOpen(false)
      refresh()
    } else {
      setToast({ message: res.error ?? t('common.error', 'Error'), color: 'danger' })
    }
  }

  async function handleExpand(family: Family) {
    if (expandedId === family.id) {
      setExpandedId(null)
      setExpandedDetail(null)
      return
    }
    setExpandedId(family.id)
    const res = await getFamilyById(family.id)
    if (res.ok && res.data) setExpandedDetail(res.data)
  }

  async function handleInvite() {
    if (!inviteOpen || !inviteEmail.trim()) return
    const res = await inviteMember(inviteOpen, inviteEmail.trim())
    if (res.ok) {
      setToast({ message: t('families.inviteSent', 'Invitation sent!'), color: 'success' })
      setInviteOpen(null)
      setInviteEmail('')
    } else {
      setToast({ message: res.error ?? t('common.error', 'Error'), color: 'danger' })
    }
  }

  async function handleLeave(familyId: number) {
    const res = await leaveFamily(familyId)
    if (res.ok) {
      setLeaveTarget(null)
      refresh()
    } else {
      setToast({ message: res.error ?? t('common.error', 'Error'), color: 'danger' })
    }
  }

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar color="light">
          <IonTitle>{t('nav.families', 'Families')}</IonTitle>
          <IonButtons slot="end">
            <IonButton onClick={() => setCreateOpen(true)}>
              {t('families.new', 'New')}
            </IonButton>
          </IonButtons>
        </IonToolbar>
      </IonHeader>

      <IonContent>
        {activeFamilies.length === 0 && (
          <div style={{ textAlign: 'center', padding: '40px 16px' }}>
            <IonText color="medium">
              <p>{t('families.empty', 'No families yet.')}</p>
            </IonText>
          </div>
        )}

        {activeFamilies.map((family: Family) => (
          <IonCard key={family.id} button onClick={() => handleExpand(family)}>
            <IonCardHeader>
              <IonCardTitle style={{ fontSize: 16, display: 'flex', alignItems: 'center', gap: 8 }}>
                {family.name}
                {family.isDefault && (
                  <IonBadge color="medium" style={{ fontSize: 10 }}>
                    {t('families.default', 'Default')}
                  </IonBadge>
                )}
                <IonBadge color={family.userRole === 'Head' ? 'primary' : 'light'} style={{ fontSize: 10 }}>
                  {family.userRole}
                </IonBadge>
              </IonCardTitle>
            </IonCardHeader>

            {expandedId === family.id && expandedDetail && (
              <IonCardContent onClick={e => e.stopPropagation()}>
                <IonList>
                  {expandedDetail.members.map(member => (
                    <IonItem key={member.userId} lines="none">
                      <IonLabel>
                        <h3>{member.firstName} {member.lastName}</h3>
                        <p>{member.email}</p>
                      </IonLabel>
                      <IonBadge slot="end" color={member.role === 'Head' ? 'primary' : 'medium'}>
                        {member.role}
                      </IonBadge>
                    </IonItem>
                  ))}
                </IonList>

                <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
                  {family.userRole === 'Head' && (
                    <IonButton size="small" fill="outline" onClick={() => setInviteOpen(family.id)}>
                      {t('families.invite', 'Invite')}
                    </IonButton>
                  )}
                  {!family.isDefault && (
                    <IonButton size="small" fill="outline" color="danger" onClick={() => setLeaveTarget(family.id)}>
                      {t('families.leave', 'Leave')}
                    </IonButton>
                  )}
                </div>
              </IonCardContent>
            )}
          </IonCard>
        ))}
      </IonContent>

      {/* Create family modal */}
      <IonModal isOpen={createOpen} onDidDismiss={() => setCreateOpen(false)} initialBreakpoint={0.5} breakpoints={[0, 0.5]}>
        <IonHeader>
          <IonToolbar>
            <IonButtons slot="start">
              <IonButton onClick={() => setCreateOpen(false)}>{t('common.cancel', 'Cancel')}</IonButton>
            </IonButtons>
            <IonTitle>{t('families.new', 'New Family')}</IonTitle>
            <IonButtons slot="end">
              <IonButton strong onClick={handleCreate}>{t('common.save', 'Save')}</IonButton>
            </IonButtons>
          </IonToolbar>
        </IonHeader>
        <IonContent className="ion-padding">
          <IonItem>
            <IonLabel position="stacked">{t('families.name', 'Name')}</IonLabel>
            <IonInput
              value={newFamilyName}
              onIonInput={e => setNewFamilyName(String(e.detail.value ?? ''))}
              autofocus
            />
          </IonItem>
        </IonContent>
      </IonModal>

      {/* Invite modal */}
      <IonModal isOpen={inviteOpen !== null} onDidDismiss={() => setInviteOpen(null)} initialBreakpoint={0.5} breakpoints={[0, 0.5]}>
        <IonHeader>
          <IonToolbar>
            <IonButtons slot="start">
              <IonButton onClick={() => setInviteOpen(null)}>{t('common.cancel', 'Cancel')}</IonButton>
            </IonButtons>
            <IonTitle>{t('families.invite', 'Invite Member')}</IonTitle>
            <IonButtons slot="end">
              <IonButton strong onClick={handleInvite}>{t('families.sendInvite', 'Send')}</IonButton>
            </IonButtons>
          </IonToolbar>
        </IonHeader>
        <IonContent className="ion-padding">
          <IonItem>
            <IonLabel position="stacked">{t('auth.email.label', 'Email')}</IonLabel>
            <IonInput
              type="email"
              value={inviteEmail}
              onIonInput={e => setInviteEmail(String(e.detail.value ?? ''))}
              autofocus
            />
          </IonItem>
        </IonContent>
      </IonModal>

      {/* Leave confirm */}
      <IonAlert
        isOpen={leaveTarget !== null}
        header={t('families.leaveConfirm', 'Leave family?')}
        message={t('families.leaveConfirmMessage', 'You will lose access to shared expenses.')}
        buttons={[
          { text: t('common.cancel', 'Cancel'), role: 'cancel', handler: () => setLeaveTarget(null) },
          { text: t('families.leave', 'Leave'), role: 'destructive', handler: () => { if (leaveTarget) handleLeave(leaveTarget) } },
        ]}
      />

      <IonToast
        isOpen={!!toast}
        message={toast?.message ?? ''}
        duration={2500}
        color={toast?.color}
        onDidDismiss={() => setToast(null)}
      />
    </IonPage>
  )
}
