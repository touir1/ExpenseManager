# Plan 5 — Families Management (Web)

> Source: `docs/plans/ux-ui-improvements.md` §5  
> Scope: `frontend/dashboard` + `backend/expenses`  
> Priority: 🟡 Medium (3 items) + 🟢 Low (1 already done)

---

## Items

| # | Priority | Item | Type |
|---|----------|------|------|
| A | 🟡 | Archive confirmation modal | Frontend only |
| B | 🟡 | FamilyCard expand/collapse animation | Frontend only |
| C | 🟡 | Invite revocation (pending invitations UI + API) | Full-stack |
| D | 🟢 | Tab badge counts update live | **Already done** — counts derived from `useMemo` over `families` context array; no work needed |

---

## Step 1 — Archive Confirmation Modal (Item A)

**Problem:** Clicking the archive icon in `FamilyCard` calls `handleArchive()` immediately. No confirmation. Archiving hides all expense attributions — hard to undo.

**Files:**
- [frontend/dashboard/src/features/families/pages/FamiliesPage.tsx](frontend/dashboard/src/features/families/pages/FamiliesPage.tsx)
- [frontend/dashboard/src/features/families/pages/__tests__/FamiliesPage.test.tsx](frontend/dashboard/src/features/families/pages/__tests__/FamiliesPage.test.tsx)
- 4× locale files (`en`, `fr`, `es`, `de`) under `frontend/dashboard/src/i18n/locales/*/translation.json`

### 1a. New i18n keys (all 4 locales)

Add under `families`:

```json
"archiveConfirmTitle": "Archive {{name}}?",
"archiveConfirmMessage": "Expenses attributed to this family will remain but the family will be hidden from filters and selectors.",
"archiveConfirmSubmit": "Archive",
"archiveConfirmCancel": "Cancel",
"archiveConfirmSubmitting": "Archiving…"
```

### 1b. FamiliesPage.tsx changes

1. Add state inside `FamilyCard`: `const [familyToArchiveConfirm, setFamilyToArchiveConfirm] = useState(false)`
2. Replace the direct `handleArchive` call from the archive button with `setFamilyToArchiveConfirm(true)`
3. Add `ConfirmArchiveModal` inline component (reuse the existing `Modal` wrapper):

```tsx
function ConfirmArchiveModal({
  family,
  onClose,
  onConfirm,
}: { family: Family; onClose: () => void; onConfirm: () => Promise<void> }) {
  const { t } = useTranslation()
  const [submitting, setSubmitting] = useState(false)

  const handleConfirm = async () => {
    setSubmitting(true)
    await onConfirm()
    setSubmitting(false)
    onClose()
  }

  return (
    <Modal title={t('families.archiveConfirmTitle', { name: family.name })} onClose={onClose}>
      <p className="text-sm text-ink-mute mb-5">{t('families.archiveConfirmMessage')}</p>
      <div className="flex justify-end gap-2">
        <button
          onClick={onClose}
          className="px-3.5 py-2 rounded-xl border border-surface-border text-sm font-medium text-ink-body hover:bg-surface-subtle transition-colors cursor-pointer"
        >
          {t('families.archiveConfirmCancel')}
        </button>
        <SubmitButton
          isSubmitting={submitting}
          label={t('families.archiveConfirmSubmit')}
          loadingLabel={t('families.archiveConfirmSubmitting')}
        />
      </div>
    </Modal>
  )
}
```

4. In `FamilyCard`, render `{familyToArchiveConfirm && <ConfirmArchiveModal ... />}` and wire `onConfirm` to the actual `handleArchive` logic (move API call out of `handleArchive` so modal calls it).

### 1c. Test updates (FamiliesPage.test.tsx)

- Add test: clicking archive button shows confirmation modal (not toast yet)
- Add test: confirming modal calls `archiveFamily` and shows toast
- Add test: cancelling modal does NOT call `archiveFamily`
- Existing "archive" tests may need updating if they assumed direct action

---

## Step 2 — Expand/Collapse Animation (Item B)

**Problem:** `{expanded && expandedContent}` — the member list appears/disappears instantly (DOM mount/unmount). No visual transition.

**Files:**
- [frontend/dashboard/src/features/families/pages/FamiliesPage.tsx](frontend/dashboard/src/features/families/pages/FamiliesPage.tsx)
- [frontend/dashboard/src/features/families/pages/__tests__/FamiliesPage.test.tsx](frontend/dashboard/src/features/families/pages/__tests__/FamiliesPage.test.tsx)

### 2a. Approach

Keep the content mounted once loaded (prevents re-fetch on re-expand). Wrap in a `div` with `max-height` CSS transition:

**In `FamilyCard`, replace:**
```tsx
{expanded && expandedContent}
```

**With:**
```tsx
{(expanded || detail || loadingDetail) && (
  <div
    style={{
      maxHeight: expanded ? '600px' : '0',
      overflow: 'hidden',
      transition: 'max-height 0.25s ease-in-out',
    }}
  >
    {expandedContent}
  </div>
)}
```

Logic:
- Rendered once detail/loading starts, never unmounted after first load
- `maxHeight: 0` + `overflow: hidden` when collapsed → invisible but mounted
- `maxHeight: 600px` when expanded → smoothly animates open (600 px far exceeds any realistic member list)

### 2b. Test updates

Existing tests that check member list visibility may need `await waitFor` adjustments since the content is now always in the DOM (just hidden). Use `toBeVisible()` or check `style.maxHeight` instead of presence/absence if needed.

---

## Step 3 — Invite Revocation: Backend (Item C)

**Problem:** No way to cancel a pending invitation once sent.

### 3a. New DTO — `FamilyPendingInvitationDto.cs`

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Controllers/DTO/FamilyPendingInvitationDto.cs`

```csharp
namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class FamilyPendingInvitationDto
    {
        public string Token { get; set; } = null!;
        public string InviteeEmail { get; set; } = null!;
        public DateTime InvitedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
```

### 3b. `IFamilyRepository` — add 2 methods

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Repositories/Contracts/IFamilyRepository.cs`

```csharp
Task<IEnumerable<FamilyInvitation>> GetPendingInvitationsByFamilyAsync(int familyId);
Task DeleteInvitationAsync(FamilyInvitation invitation);
```

"Pending" = `AcceptedAt IS NULL AND ExpiresAt > UtcNow`.

### 3c. `FamilyRepository` — implement

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Repositories/FamilyRepository.cs`

```csharp
public async Task<IEnumerable<FamilyInvitation>> GetPendingInvitationsByFamilyAsync(int familyId)
    => await _db.FamilyInvitations
        .Where(i => i.FamilyId == familyId
                 && i.AcceptedAt == null
                 && i.ExpiresAt > DateTime.UtcNow)
        .AsNoTracking()
        .ToListAsync();

public async Task DeleteInvitationAsync(FamilyInvitation invitation)
{
    _db.FamilyInvitations.Remove(invitation);
    await _db.SaveChangesAsync();
}
```

### 3d. `IFamilyService` — add 2 methods

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Services/Contracts/IFamilyService.cs`

```csharp
Task<IEnumerable<FamilyPendingInvitationDto>> GetPendingInvitationsAsync(int familyId, int userId);
Task RevokeInvitationAsync(int familyId, string token, int userId);
```

### 3e. `FamilyService` — implement

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Services/FamilyService.cs`

```csharp
public async Task<IEnumerable<FamilyPendingInvitationDto>> GetPendingInvitationsAsync(int familyId, int userId)
{
    var membership = await _familyRepo.GetMembershipAsync(familyId, userId)
        ?? throw new FamilyForbiddenException();

    var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
    if (membership.RoleId != headId)
        throw new FamilyForbiddenException();

    var invitations = await _familyRepo.GetPendingInvitationsByFamilyAsync(familyId);
    return invitations.Select(i => new FamilyPendingInvitationDto
    {
        Token = i.Token,
        InviteeEmail = i.InviteeEmail,
        InvitedAt = i.InvitedAt,
        ExpiresAt = i.ExpiresAt
    });
}

public async Task RevokeInvitationAsync(int familyId, string token, int userId)
{
    var membership = await _familyRepo.GetMembershipAsync(familyId, userId)
        ?? throw new FamilyForbiddenException();

    var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
    if (membership.RoleId != headId)
        throw new FamilyForbiddenException();

    var invitation = await _familyRepo.GetInvitationByTokenAsync(token)
        ?? throw new FamilyInvitationException(ServiceErrors.FamilyInvitationInvalid);

    if (invitation.FamilyId != familyId)
        throw new FamilyForbiddenException();

    if (invitation.AcceptedAt.HasValue)
        throw new FamilyInvitationException(ServiceErrors.FamilyInvitationAlreadyAccepted);

    await _familyRepo.DeleteInvitationAsync(invitation);
}
```

### 3f. `FamilyController` — add 2 endpoints

**File:** `backend/expenses/Touir.ExpensesManager.Expenses/Controllers/FamilyController.cs`

```csharp
/// <summary>List pending (not accepted, not expired) invitations for a family. Head only.</summary>
[HttpGet("{id:int}/invitations")]
[ProducesResponseType(typeof(IEnumerable<FamilyPendingInvitationDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
public async Task<IActionResult> GetPendingInvitationsAsync(int id)
{
    try
    {
        var userId = JwtCookieReader.GetUserId(Request);
        if (userId is null)
            return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

        var invitations = await _familyService.GetPendingInvitationsAsync(id, userId.Value);
        return Ok(invitations);
    }
    catch (FamilyForbiddenException ex)
    {
        return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
    }
    catch (Exception)
    {
        return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
    }
}

/// <summary>Revoke a pending invitation. Head only. Deletes the invitation record.</summary>
[HttpDelete("{id:int}/invitations/{token}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
public async Task<IActionResult> RevokeInvitationAsync(int id, string token)
{
    try
    {
        var userId = JwtCookieReader.GetUserId(Request);
        if (userId is null)
            return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

        await _familyService.RevokeInvitationAsync(id, token, userId.Value);
        return NoContent();
    }
    catch (FamilyInvitationException ex)
    {
        return BadRequest(new ErrorResponse { Message = ex.Message });
    }
    catch (FamilyForbiddenException ex)
    {
        return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
    }
    catch (Exception)
    {
        return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
    }
}
```

### 3g. Backend tests

**File:** `backend/expenses/Touir.ExpensesManager.Expenses.Tests/Controllers/FamilyControllerTests.cs`

Add test cases for:
- `GetPendingInvitations` — 200 returns list for Head
- `GetPendingInvitations` — 403 for non-Head (service throws `FamilyForbiddenException`)
- `GetPendingInvitations` — 401 when no auth token
- `RevokeInvitation` — 204 success for Head
- `RevokeInvitation` — 400 when invitation not found (`FamilyInvitationException`)
- `RevokeInvitation` — 400 when invitation already accepted
- `RevokeInvitation` — 403 for non-Head
- `RevokeInvitation` — 401 when no auth token

Use existing mock pattern: mock `IFamilyService`, inject into controller via constructor.

---

## Step 4 — Invite Revocation: Frontend (Item C continued)

### 4a. `family.type.ts` — add type

**File:** [frontend/dashboard/src/features/families/types/family.type.ts](frontend/dashboard/src/features/families/types/family.type.ts)

```ts
export type FamilyPendingInvitation = {
  token: string
  inviteeEmail: string
  invitedAt: string
  expiresAt: string
}
```

### 4b. `familyApi.service.ts` — add 2 functions

**File:** [frontend/dashboard/src/features/families/services/familyApi.service.ts](frontend/dashboard/src/features/families/services/familyApi.service.ts)

```ts
export function getPendingInvitations(familyId: number): Promise<ApiResponse<FamilyPendingInvitation[]>> {
  return get<FamilyPendingInvitation[]>(`${BASE}/${familyId}/invitations`)
}

export function revokeInvitation(familyId: number, token: string): Promise<ApiResponse<void>> {
  return del<void>(`${BASE}/${familyId}/invitations/${encodeURIComponent(token)}`)
}
```

### 4c. i18n keys (all 4 locales)

Add under `families`:

```json
"pendingInvitations": "Pending invitations ({{count}})",
"pendingInvitationsEmpty": "No pending invitations.",
"revokeAction": "Revoke",
"revokeSuccess": "Invitation revoked.",
"revokeConfirmTitle": "Revoke invitation?",
"revokeConfirmMessage": "The invitation for {{email}} will be cancelled and the link will no longer work.",
"revokeConfirmSubmit": "Revoke",
"revokeConfirmSubmitting": "Revoking…",
"invitedAt": "Invited {{date}}",
"expiresAt": "Expires {{date}}"
```

### 4d. `FamiliesPage.tsx` — update `FamilyDetailPanel`

`FamilyDetailPanel` currently receives `family`, `detail`, `onRefresh`, `onLeave`. Extend it with a pending invitations sub-section:

1. Add state:
   ```tsx
   const [pendingInvitations, setPendingInvitations] = useState<FamilyPendingInvitation[]>([])
   const [loadingInvitations, setLoadingInvitations] = useState(false)
   const [revokeTarget, setRevokeTarget] = useState<FamilyPendingInvitation | null>(null)
   ```

2. Load on mount (Head only, non-default):
   ```tsx
   const loadInvitations = useCallback(async () => {
     setLoadingInvitations(true)
     const res = await getPendingInvitations(family.id)
     if (res.ok && res.data) setPendingInvitations(res.data)
     setLoadingInvitations(false)
   }, [family.id])

   useEffect(() => {
     if (isHead && !family.isDefault) loadInvitations()
   }, [isHead, family.isDefault, loadInvitations])
   ```

3. Revoke handler:
   ```tsx
   const handleRevoke = async () => {
     if (!revokeTarget) return
     const res = await revokeInvitation(family.id, revokeTarget.token)
     if (res.ok) {
       show(t('families.revokeSuccess'), 'success')
       setRevokeTarget(null)
       loadInvitations()
     }
   }
   ```

4. Render pending invitations section (below the members list, above the "Leave" button, visible only to heads of non-default families):

   ```tsx
   {isHead && !family.isDefault && (
     <div className="mt-3 pt-3 border-t border-surface-border">
       <span className="text-xs font-semibold text-ink-mute uppercase tracking-wide">
         {t('families.pendingInvitations', { count: pendingInvitations.length })}
       </span>
       {loadingInvitations ? (
         <div className="mt-2 animate-pulse space-y-1.5">
           {[0, 1].map(i => <div key={i} className="h-3 bg-surface-subtle rounded w-48" />)}
         </div>
       ) : pendingInvitations.length === 0 ? (
         <p className="text-xs text-ink-faint mt-1">{t('families.pendingInvitationsEmpty')}</p>
       ) : (
         <ul className="mt-2 space-y-1.5">
           {pendingInvitations.map(inv => (
             <li key={inv.token} className="flex items-center justify-between gap-2">
               <div className="min-w-0">
                 <p className="text-sm text-ink truncate">{inv.inviteeEmail}</p>
                 <p className="text-xs text-ink-faint">
                   {t('families.expiresAt', { date: new Date(inv.expiresAt).toLocaleDateString() })}
                 </p>
               </div>
               <button
                 onClick={() => setRevokeTarget(inv)}
                 className="text-xs font-medium text-berry hover:text-berry/80 transition-colors cursor-pointer shrink-0"
               >
                 {t('families.revokeAction')}
               </button>
             </li>
           ))}
         </ul>
       )}
     </div>
   )}
   ```

5. Confirmation modal for revoke (reuse `Modal` wrapper):
   ```tsx
   {revokeTarget && (
     <ConfirmRevokeModal
       invitation={revokeTarget}
       onClose={() => setRevokeTarget(null)}
       onConfirm={handleRevoke}
     />
   )}
   ```

   `ConfirmRevokeModal` is a new inline component in `FamiliesPage.tsx` following the same pattern as `ConfirmArchiveModal` from Step 1.

### 4e. Update `familyApi.service.ts` mock in tests

**File:** [frontend/dashboard/src/features/families/pages/__tests__/FamiliesPage.test.tsx](frontend/dashboard/src/features/families/pages/__tests__/FamiliesPage.test.tsx)

Add to the `vi.mock` factory:
```ts
getPendingInvitations: vi.fn().mockResolvedValue({ ok: true, data: [] }),
revokeInvitation: vi.fn(),
```

And add import:
```ts
import * as familyApi from '@/features/families/services/familyApi.service'
```
(already imported — just add the new function refs)

### 4f. New frontend tests

Add test cases:

- **Pending invitations section visible to Head**: expand family card → pending invitations section appears (mocked with 1 pending invitation)
- **Pending invitations empty state**: expand card → `pendingInvitationsEmpty` text shown when `getPendingInvitations` returns `[]`
- **Revoke shows confirmation**: click "Revoke" → `ConfirmRevokeModal` appears with email
- **Revoke confirmed**: confirm modal → `revokeInvitation` called → toast shown → invitations reloaded
- **Revoke cancelled**: cancel modal → `revokeInvitation` NOT called
- **Pending invitations NOT shown for non-Head**: same card but `userRole: 'Member'` → section absent
- **Pending invitations NOT shown for default family**: `isDefault: true` → section absent

---

## Step 5 — `/done`

Run `/done` to update:
- `CHANGELOG.md` — add entry for v5 families management improvements
- `CLAUDE.md` — update if family system constraints change (they don't here)
- `FILE-TREE.md` — update if new files added (`FamilyPendingInvitationDto.cs`)
- `backend/expenses/README.md` — document the 2 new family endpoints
- `docs/plans/ux-ui-improvements.md` — mark 3 items as ✅ Done in §5

---

## Implementation Order

```
Step 1 (archive confirm)     → frontend-only, fast, no deps
Step 2 (expand animation)    → frontend-only, fast, no deps
Step 3 (revoke backend)      → backend full-stack (3a→3b→3c→3d→3e→3f→3g)
Step 4 (revoke frontend)     → depends on Step 3 API being defined
Step 5 (/done)               → last
```

Steps 1 and 2 are independent — can be done in either order.  
Step 3 must precede Step 4.

---

## File Change Summary

| File | Change |
|------|--------|
| `FamiliesPage.tsx` | ConfirmArchiveModal, expand animation, pending invitations section, ConfirmRevokeModal |
| `FamiliesPage.test.tsx` | New tests for all 3 items |
| `family.type.ts` | Add `FamilyPendingInvitation` type |
| `familyApi.service.ts` | Add `getPendingInvitations`, `revokeInvitation` |
| `familyApi.service.test.ts` | Add tests for 2 new functions |
| `en/translation.json` + 3 others | New i18n keys (archive confirm + revoke) |
| `FamilyPendingInvitationDto.cs` | **New file** |
| `IFamilyRepository.cs` | +2 method signatures |
| `FamilyRepository.cs` | +2 implementations |
| `IFamilyService.cs` | +2 method signatures |
| `FamilyService.cs` | +2 implementations |
| `FamilyController.cs` | +2 endpoints |
| `FamilyControllerTests.cs` | +8 test cases |
| `backend/expenses/README.md` | Document new endpoints |
| `CHANGELOG.md` | New entry |
| `FILE-TREE.md` | New DTO file |
| `docs/plans/ux-ui-improvements.md` | Mark §5 items done |

---

## Notes

- **No DB migration needed** for revoke: `FamilyInvitation` table already exists; revoke = hard delete of the row.
- **Accept flow unaffected**: `AcceptInviteAsync` checks `GetInvitationByTokenAsync` → null → throws `FamilyInvitationInvalid`. A revoked (deleted) invitation correctly returns 400 "invalid".
- **Tab badge counts (Item D)**: already correct — `activeFamilies` and `archivedFamilies` are `useMemo` over `families` from `FamilyContext`, which is refetched on every `refresh()` call. No change needed.
- Use `/ui-ux-pro-max` for any visual design decisions during implementation (colors, spacing, motion).
