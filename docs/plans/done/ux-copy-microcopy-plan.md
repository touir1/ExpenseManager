# Plan: Section 12 — UX Copy & Microcopy

> Source: [ux-ui-improvements.md § 12](ux-ui-improvements.md#12-ux-copy--microcopy)
> Scope: `frontend/dashboard`

Use `/ui-ux-pro-max` for design decisions in this plan (empty-state illustration style, icon choice, spacing/CTA hierarchy) and again during implementation of each item (component styling, accessibility, dark-mode contrast).

---

## Item 1 — 🟡 Error messages use internal error codes exposed to users

**Files:**
- [api.service.ts](../../frontend/dashboard/src/services/api.service.ts) — `getErrorMessage()` (line ~22-31), `buildErrorResponse()` (line ~54-58), `getRawCode()` (line ~50-52)
- [apiErrors.constant.ts](../../frontend/dashboard/src/constants/apiErrors.constant.ts) — `BACKEND_KEYS`/`BACKEND_ERROR_CODES` Proxy (line ~45-49), `API_ERRORS` (line ~3-11)
- i18n locales: `frontend/dashboard/src/i18n/locales/{en,fr,es,de}/translation.json`

Current: `getErrorMessage` falls back to `backendCode || statusText || 'Request failed'` — an unmapped backend code (e.g. `FAMILY_NAME_ALREADY_EXISTS`) is shown to the user raw, untranslated, if missing from `BACKEND_KEYS`.

Steps:
1. Run `/ui-ux-pro-max` to confirm tone/wording for a generic fallback message (e.g. "Something went wrong. Please try again.") vs a dev-only console warning strategy.
2. Change the fallback in `getErrorMessage` to a translated generic message (`apiErrors.generic` key) instead of the raw `backendCode` string.
3. Add a `console.warn`/dev-only log when a backend code reaches the fallback branch (`if (!BACKEND_KEYS[backendCode]) console.warn('Missing i18n mapping for error code:', backendCode)`) so gaps are caught during development, per the audit's "alert developers" requirement.
4. Add the new `apiErrors.generic` key to all four locale files (`en`, `fr`, `es`, `de`).
5. Do not touch `rawCode` — `CsvImportPage.tsx:761` depends on the untranslated code for branching logic; only the *displayed* message changes.

## Item 2 — 🟡 Empty states use only text — no illustration or CTA

**Files (all plain `<p>` text, no icon):**
- [ExpensesPage.tsx:422](../../frontend/dashboard/src/features/expenses/pages/ExpensesPage.tsx) (`expenses.noExpenses`, has CTA link already)
- [RecentExpenses.tsx:74](../../frontend/dashboard/src/features/dashboard/components/RecentExpenses.tsx) (`dashboard.recent.empty`)
- [SpendChart.tsx:68](../../frontend/dashboard/src/features/dashboard/components/SpendChart.tsx) / [CategoryDonut.tsx:59](../../frontend/dashboard/src/features/dashboard/components/CategoryDonut.tsx) (`dashboard.empty`)
- [HomeDashboardPage.tsx:196-208](../../frontend/dashboard/src/features/dashboard/pages/HomeDashboardPage.tsx) (`EmptyDashboard` — has title/subtitle/CTA, no icon)
- [NotificationsPage.tsx:68-69](../../frontend/dashboard/src/features/notifications/pages/NotificationsPage.tsx), [NotificationBell.tsx:83](../../frontend/dashboard/src/features/notifications/components/NotificationBell.tsx)
- [FamiliesPage.tsx:469,728](../../frontend/dashboard/src/features/families/pages/FamiliesPage.tsx) (pendingInvitationsEmpty, emptyActive/emptyArchived)
- [SettingsPage.tsx:273](../../frontend/dashboard/src/features/dashboard/pages/SettingsPage.tsx)

Steps:
1. Run `/ui-ux-pro-max` to design one shared empty-state pattern: icon/illustration (inline SVG, consistent style with existing `NotificationBell` icon set — no new asset pipeline), bold heading, subtext, optional CTA button slot.
2. Build `components/EmptyState.tsx`: props `{icon?: ReactNode, title: string, subtitle?: string, action?: {label:string, onClick:()=>void}}`, Tailwind-styled per `@layer components` conventions (card look optional — some usages are inline in a list, not a card).
3. Replace each plain `<p>` above with `<EmptyState>`, keeping existing i18n keys as `title`/`subtitle` (chart panels like `SpendChart`/`CategoryDonut` use compact variant — smaller icon, no CTA; list pages like `ExpensesPage`/`FamiliesPage` get full variant with CTA).
4. For panels/lists that already have a CTA (`ExpensesPage`, `HomeDashboardPage` EmptyDashboard), wire the existing button/link into `EmptyState`'s `action` prop rather than duplicating markup.
5. Keep icons decorative (`aria-hidden="true"`) — the text content still carries the accessible name.

## Item 3 — 🟢 Form submit button label is static ("Save", "Add")

**Files:**
- [SubmitButton.tsx](../../frontend/dashboard/src/components/SubmitButton.tsx) — existing reusable component (spinner + `loadingLabel` when `isSubmitting`), already used correctly by `ExpenseForm.tsx:391-395` and `FamiliesPage.tsx:218,271,324`.
- [AdminCategoriesPage.tsx:172-174](../../frontend/dashboard/src/features/admin/pages/AdminCategoriesPage.tsx) — plain `<button>{t('common.save','Save')}</button>`, no `disabled`/`isSubmitting`/loading label.

Steps:
1. Run `/ui-ux-pro-max` to confirm loading-label wording for category/subcategory save ("Saving…") matches the existing tone used elsewhere.
2. Identify the submit handler's async state in `AdminCategoriesPage.tsx` (add local `isSubmitting` state if none exists around the category/subcategory create-edit mutation).
3. Replace the static `<button>` with `<SubmitButton isSubmitting={...} label={t('common.save','Save')} loadingLabel={t('common.saving','Saving…')} />`, matching the prop shape already used in `ExpenseForm.tsx`.
4. Add `common.saving` key to all locale files if not already present (check `common.save` siblings).
5. Audit for any other raw `<button>` submit elements missed by this sweep (grep `type="submit"` across `frontend/dashboard/src` for stragglers outside the three files already using `SubmitButton`).

---

## Unit Tests

**Item 1** — [api.service.test.ts](../../frontend/dashboard/src/services/__tests__/api.service.test.ts)
- Given a backend code not present in `BACKEND_KEYS`, assert `getErrorMessage` returns the translated generic fallback string, not the raw code.
- Assert `console.warn` fires once with the unmapped code (spy on `console.warn`).
- Assert `rawCode` on the returned `ApiResponse` still carries the original untranslated code (regression guard for `CsvImportPage`'s `MISSING_HEADERS` branching).

**Item 2** — new `EmptyState.test.tsx` + update existing tests touching replaced markup:
- `EmptyState.tsx`: renders icon (`aria-hidden`), title, optional subtitle, optional action button that fires `onClick`.
- `ExpensesPage.test.tsx`, `RecentExpenses.test.tsx`, `SpendChart.test.tsx`, `CategoryDonut.test.tsx`, `HomeDashboardPage.test.tsx`, `NotificationsPage.test.tsx`, `FamiliesPage.test.tsx`, `SettingsPage.test.tsx`: update empty-state assertions to query by the new structure (heading/text role) instead of the old bare `<p>` text node — confirm existing i18n key text still renders.

**Item 3** — [AdminCategoriesPage.test.tsx](../../frontend/dashboard/src/features/admin/pages/__tests__/AdminCategoriesPage.test.tsx) (check existing file; add if absent)
- While submitting, button shows `loadingLabel` ("Saving…") and is disabled.
- On success/error resolution, button reverts to static "Save" label and re-enables.
- Snapshot/structural parity check against `FamiliesPage`'s existing `SubmitButton` usage pattern (no divergent prop shape).

Run: `npm test` from `frontend/dashboard` (full suite with coverage, per [commands.md](../../.claude/commands.md)). Confirm no coverage regression on touched files.

---

/done
