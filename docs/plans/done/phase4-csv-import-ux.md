# Phase 4 — CSV Import (Web) UX Implementation Plan

> Source: `docs/plans/ux-ui-improvements.md` §4 + §9 (accessibility)  
> Estimated effort: ~70 min  
> Files: `CsvImportPage.tsx`, 4 locale JSONs, `CsvImportPage.test.tsx`

---

## Items

| # | Priority | Item | Effort |
|---|----------|------|--------|
| 1 | 🔴 | Upload spinner — disable zone while loading | 10 min |
| 2 | 🔴 | Navigation warning for active import session | 20 min |
| 3 | 🟡 | "Edited" badge on user-modified rows | 15 min |
| 4 | 🟡 | Sort by Status (errors first toggle) | 10 min |
| 5 | 🟡 | Template download styled as button with description | 5 min |
| 6 | 🟡 | Status icons alongside color (accessibility) | 10 min |

---

## Item 1 — Upload Spinner / Disable Zone While Loading

**Problem:** `loadingPreview=true` only changes `<p>` text to "Loading…". Dropzone stays fully interactive — user can click again or double-submit.

**Current code:** `CsvImportPage.tsx` line 769 — single `<p>` text swap.

**Change:**
- Add `pointer-events-none opacity-75` to dropzone wrapper `<div>` when `loadingPreview`
- Replace single `<p>` loading text with conditional block:
  - **Loading:** `animate-spin` SVG + "Uploading file…" text
  - **Idle:** existing upload arrow icon + dropzone hint text

```tsx
// Dropzone div — add to existing className:
${loadingPreview ? 'pointer-events-none opacity-75 cursor-default' : 'cursor-pointer'}

// Replace the inner content:
{loadingPreview ? (
  <div className="flex flex-col items-center gap-3">
    <svg className="animate-spin h-8 w-8 text-brand-500" fill="none" viewBox="0 0 24 24">
      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
    </svg>
    <p className="text-sm text-ink-mute">{t('expenses.import.uploading')}</p>
  </div>
) : (
  <>
    <svg className="mx-auto h-10 w-10 text-ink-faint mb-3" ...existing upload icon... />
    <p className="text-sm text-ink-mute">{t('expenses.import.dropzone')}</p>
  </>
)}
```

**New i18n key:**
```json
"uploading": "Uploading file…"
```

---

## Item 2 — Navigation Warning for Active Import Session

**Problem:** No guard. User can click Back, a navbar link, or close the tab — all progress is lost silently.

**Change:**

### 2a — Browser refresh / tab close
```tsx
useEffect(() => {
  if (!preview) return
  const handler = (e: BeforeUnloadEvent) => {
    e.preventDefault()
    e.returnValue = ''
  }
  window.addEventListener('beforeunload', handler)
  return () => window.removeEventListener('beforeunload', handler)
}, [preview])
```

### 2b — In-app navigation (React Router v6)
```tsx
import { useBlocker } from 'react-router-dom'

const blocker = useBlocker(preview !== null)
```

### 2c — Confirmation modal (render at bottom of component)
Reuse existing modal pattern (`fixed inset-0 bg-black/40 z-50`):
```tsx
{blocker.state === 'blocked' && (
  <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
    <div className="bg-surface-card rounded-2xl shadow-warm border border-surface-border w-full max-w-sm mx-4 p-6">
      <h2 className="text-base font-semibold text-ink mb-2">
        {t('expenses.import.leaveTitle')}
      </h2>
      <p className="text-sm text-ink-mute mb-5">
        {t('expenses.import.leaveWarning')}
      </p>
      <div className="flex gap-3 justify-end">
        <button
          onClick={() => blocker.reset()}
          className="px-4 py-2 text-sm font-medium rounded-xl border border-surface-border text-ink hover:bg-surface-subtle transition-colors"
        >
          {t('expenses.import.stayHere')}
        </button>
        <button
          onClick={() => blocker.proceed()}
          className="px-4 py-2 text-sm font-medium rounded-xl bg-berry text-white hover:bg-berry/90 transition-colors"
        >
          {t('expenses.import.leaveConfirm')}
        </button>
      </div>
    </div>
  </div>
)}
```

**New i18n keys:**
```json
"leaveTitle":   "Leave import?",
"leaveWarning": "You have an active import session. Your progress will be lost if you leave.",
"leaveConfirm": "Leave anyway",
"stayHere":     "Stay on page"
```

---

## Item 3 — "Edited" Badge on User-Modified Rows

**Problem:** `saveAndValidateRow` (line 645) clears `editedRows[rowNumber]` after backend validation — no trace the row was ever edited. Row looks identical to never-touched rows.

**Change:**

### 3a — New state
```tsx
const [userEditedRows, setUserEditedRows] = useState<Set<number>>(new Set())
```

### 3b — Track on save
In `saveAndValidateRow`, after clearing from `editedRows` (line 645):
```tsx
setUserEditedRows(prev => new Set([...prev, rowNumber]))
```

### 3c — Clear on remove and on cancel-all
```tsx
// handleRemove:
setUserEditedRows(prev => { const s = new Set(prev); s.delete(rowNumber); return s })

// Cancel button (return to upload zone) — add:
setUserEditedRows(new Set())
```

### 3d — Pass prop to ImportRow
```tsx
// ImportRow props — add:
wasEdited: boolean

// In ImportRow read-only view, row number <td> (line 409):
<td className="px-2 py-2 text-ink-mute text-xs">
  <span>{row.rowNumber}</span>
  {wasEdited && !editing && (
    <span className="ml-1 px-1 py-0.5 text-[10px] font-medium bg-amber-100 text-amber-700 rounded">
      {t('expenses.import.columns.edited')}
    </span>
  )}
</td>
```

**New i18n key:**
```json
"columns": {
  "edited": "Edited"
}
```

---

## Item 4 — Sort by Status (Errors First)

**Problem:** Rows render in original CSV order. With 100+ rows, invalid rows require scrolling to find.

**Change:**

### 4a — New state
```tsx
const [sortErrors, setSortErrors] = useState(false)
```

### 4b — Computed display rows
```tsx
const displayRows = sortErrors
  ? [...preview.rows].sort((a, b) => Number(a.isValid) - Number(b.isValid))
  : preview.rows
```

### 4c — Toggle button in summary bar
Add after the error count badge (line 788):
```tsx
<button
  onClick={() => setSortErrors(s => !s)}
  className="ml-auto flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg border border-surface-border text-ink-mute hover:text-ink hover:bg-surface-subtle transition-colors"
>
  <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
    <path strokeLinecap="round" strokeLinejoin="round" d="M3 4h13M3 8h9m-9 4h6m4 0l4-4m0 0l4 4m-4-4v12" />
  </svg>
  {sortErrors ? t('expenses.import.sortNatural') : t('expenses.import.sortErrors')}
</button>
```

### 4d — Use displayRows in map (line 810)
```tsx
{displayRows.map(row => { ... })}
```

**Reset `sortErrors` when new file loaded** (in `handleFile` success path).

**New i18n keys:**
```json
"sortErrors":  "Errors first",
"sortNatural": "Row order"
```

---

## Item 5 — Template Download as Button with Description

**Problem:** Plain `<a>` text link at bottom of upload card, easy to overlook. No column hint.

**Current:** `CsvImportPage.tsx` lines 772–776.

**Change:** Replace the `<div className="mt-4 text-center">` block:
```tsx
<div className="mt-6 border-t border-surface-border pt-5 flex flex-col sm:flex-row items-start sm:items-center gap-3">
  <p className="text-xs text-ink-mute flex-1">
    {t('expenses.import.templateDescription')}
  </p>
  <a
    href={getImportTemplateUrl()}
    download="expenses-import-template.csv"
    className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-xl border border-surface-border bg-surface-card text-ink hover:bg-surface-subtle transition-colors shrink-0"
  >
    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
      <path strokeLinecap="round" strokeLinejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
    </svg>
    {t('expenses.import.templateLink')}
  </a>
</div>
```

**New i18n key:**
```json
"templateDescription": "Expected columns: date (YYYY-MM-DD), amount, currency_code — optional: category, subcategory, description, tags (semicolon-separated), families (semicolon-separated)"
```

---

## Item 6 — Status Icons (Accessibility)

**Problem:** Valid rows (white bg) vs invalid rows (red bg) differ only by color. Red-green color blind users cannot distinguish them.

**Change:** In `ImportRow` status `<td>` (lines 498–510), prepend SVG icon to each state:

```tsx
<td className="px-2 py-2 min-w-[7rem]">
  {isValidating ? (
    <span className="flex items-center gap-1 text-ink-faint text-xs">
      <svg className="animate-spin h-3.5 w-3.5" .../>
      {t('expenses.loading', 'Loading…')}
    </span>
  ) : row.isValid && !editing ? (
    <span className="flex items-center gap-1 text-emerald-600 text-xs font-medium">
      {/* checkmark circle */}
      <svg className="h-3.5 w-3.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
        <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
      {t('expenses.import.columns.valid')}
    </span>
  ) : editing ? (
    <span className="flex items-center gap-1 text-amber-600 text-xs font-medium">
      {/* pencil */}
      <svg className="h-3.5 w-3.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
        <path strokeLinecap="round" strokeLinejoin="round" d="M15.232 5.232l3.536 3.536M9 13l6.5-6.5 3.5 3.5L13 16.5H9V13z" />
      </svg>
      {t('expenses.import.columns.editing')}
    </span>
  ) : (
    <span className="flex items-start gap-1 text-red-600 text-xs">
      {/* X circle */}
      <svg className="h-3.5 w-3.5 shrink-0 mt-px" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
        <path strokeLinecap="round" strokeLinejoin="round" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
      {row.errors.map(e => t(`expenses.import.errors.${e}`, e)).join(', ')}
    </span>
  )}
</td>
```

No new i18n keys needed.

---

## i18n Summary — All New Keys

Add to `expenses.import` in all 4 locale files (`en`, `fr`, `es`, `de`):

```json
"uploading":           "Uploading file…",
"leaveTitle":          "Leave import?",
"leaveWarning":        "You have an active import session. Your progress will be lost if you leave.",
"leaveConfirm":        "Leave anyway",
"stayHere":            "Stay on page",
"sortErrors":          "Errors first",
"sortNatural":         "Row order",
"templateDescription": "Expected columns: date (YYYY-MM-DD), amount, currency_code — optional: category, subcategory, description, tags (semicolon-separated), families (semicolon-separated)",
"columns": {
  "edited": "Edited"
}
```

*(Translate appropriately for fr/es/de.)*

---

## Implementation Order

1. **Item 5** — template button (no state, safe warmup)
2. **Item 1** — upload spinner (simple state-driven UI)
3. **Item 6** — status icons (UI-only, no logic)
4. **Item 3** — edited badge (one new state + prop thread)
5. **Item 4** — sort toggle (one new state + `displayRows` compute)
6. **Item 2** — nav warning (two hooks + modal, last — touches routing)

---

## Unit Tests

### Mock Setup Changes

The existing `vi.mock('react-router-dom', ...)` block only mocks `useNavigate`. Item 2 requires also mocking `useBlocker`. Add to the module-level mock setup:

```tsx
// Add at module level alongside mockNavigate:
let mockBlocker: { state: string; proceed?: () => void; reset?: () => void } = { state: 'idle' }
const mockProceed = vi.fn()
const mockReset = vi.fn()

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useBlocker: () => mockBlocker,   // ← add this
  }
})
```

Reset in `beforeEach`:
```tsx
beforeEach(() => {
  vi.clearAllMocks()
  mockBlocker = { state: 'idle' }   // ← add this reset
  mockGetImportTemplateUrl.mockReturnValue('/api/expenses/import/template')
})
```

---

### Existing Tests — No Changes Required

All 25 existing tests pass without modification after Items 1–6 because:
- Template `<a>` still has `role="link"` and same `href` after Item 5 restyling
- `role="button"` on the dropzone div remains unchanged for Item 1
- `ImportRow` prop additions are internal to the `preview.rows.map(...)` call; no test directly constructs `ImportRow`
- `displayRows` compute (Item 4) does not change row identity, only order; no test asserts DOM row order
- Status icon SVGs are decorative — existing text-based status assertions still pass

---

### New Tests — Item 1 (Upload Spinner)

```tsx
it('shows spinner and "Uploading file…" text while file is processing', async () => {
  let resolveUpload!: (v: unknown) => void
  mockPreviewCsvImport.mockReturnValue(new Promise(r => { resolveUpload = r }))
  renderPage()
  uploadFile()

  await waitFor(() => {
    expect(screen.getByText(/uploading file/i)).toBeInTheDocument()
  })

  // Clean up — resolve so component does not remain in loading state
  resolveUpload({ ok: true, data: previewAllValid })
})

it('dropzone has pointer-events-none class while uploading', async () => {
  let resolveUpload!: (v: unknown) => void
  mockPreviewCsvImport.mockReturnValue(new Promise(r => { resolveUpload = r }))
  renderPage()
  uploadFile()

  await waitFor(() => screen.getByText(/uploading file/i))
  const dropzone = screen.getByRole('button', { name: /drag.*drop|csv/i })
  expect(dropzone).toHaveClass('pointer-events-none')

  resolveUpload({ ok: true, data: previewAllValid })
})

it('hides spinner and shows preview table after upload resolves', async () => {
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
  renderPage()
  uploadFile()

  await waitFor(() => {
    expect(screen.queryByText(/uploading file/i)).not.toBeInTheDocument()
    expect(screen.getByText('2025-01-15')).toBeInTheDocument()
  })
})
```

---

### New Tests — Item 2 (Navigation Warning)

These tests control `mockBlocker` state before rendering to exercise the modal UI. The `useBlocker` mock returns the module-level `mockBlocker` object on every render, making the modal appear/disappear based on `.state`.

```tsx
it('shows leave confirmation modal when blocker is triggered', async () => {
  mockBlocker = { state: 'blocked', proceed: mockProceed, reset: mockReset }
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
  renderPage()

  await waitFor(() => {
    expect(screen.getByText(/leave import\?/i)).toBeInTheDocument()
    expect(screen.getByText(/progress will be lost/i)).toBeInTheDocument()
  })
})

it('leave modal has "Leave anyway" and "Stay on page" buttons', async () => {
  mockBlocker = { state: 'blocked', proceed: mockProceed, reset: mockReset }
  renderPage()

  await waitFor(() => {
    expect(screen.getByRole('button', { name: /leave anyway/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /stay on page/i })).toBeInTheDocument()
  })
})

it('"Leave anyway" calls blocker.proceed()', async () => {
  mockBlocker = { state: 'blocked', proceed: mockProceed, reset: mockReset }
  renderPage()

  await waitFor(() => screen.getByRole('button', { name: /leave anyway/i }))
  fireEvent.click(screen.getByRole('button', { name: /leave anyway/i }))

  expect(mockProceed).toHaveBeenCalledOnce()
})

it('"Stay on page" calls blocker.reset()', async () => {
  mockBlocker = { state: 'blocked', proceed: mockProceed, reset: mockReset }
  renderPage()

  await waitFor(() => screen.getByRole('button', { name: /stay on page/i }))
  fireEvent.click(screen.getByRole('button', { name: /stay on page/i }))

  expect(mockReset).toHaveBeenCalledOnce()
})

it('leave modal does not show when blocker is idle', () => {
  // default mockBlocker = { state: 'idle' }
  renderPage()

  expect(screen.queryByText(/leave import\?/i)).not.toBeInTheDocument()
})
```

---

### New Tests — Item 3 ("Edited" Badge)

```tsx
it('shows "Edited" badge on row after save+validate succeeds', async () => {
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
  mockValidateCsvRows.mockResolvedValue(makeValidateResponse(validatedRow2))
  renderPage()
  uploadFile()
  await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

  fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
  await waitFor(() => screen.getByLabelText(/row 2 amount/i))
  fireEvent.change(screen.getByLabelText(/row 2 amount/i), { target: { value: '25.00' } })
  fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))

  await waitFor(() => {
    expect(screen.getByText(/edited/i)).toBeInTheDocument()
  })
})

it('does not show "Edited" badge on row that was never edited', async () => {
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
  mockValidateCsvRows.mockResolvedValue(makeValidateResponse(validatedRow2))
  renderPage()
  uploadFile()
  await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

  // Edit row 2 only
  fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
  await waitFor(() => screen.getByLabelText(/row 2 amount/i))
  fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))

  await waitFor(() => screen.getByText(/edited/i))

  // Row 1 was never edited — only one "Edited" badge
  expect(screen.getAllByText(/edited/i)).toHaveLength(1)
})

it('"Edited" badge clears when the row is removed', async () => {
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
  mockValidateCsvRows.mockResolvedValue(makeValidateResponse(validatedRow2))
  renderPage()
  uploadFile()
  await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

  fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
  await waitFor(() => screen.getByLabelText(/row 2 amount/i))
  fireEvent.click(screen.getByRole('button', { name: /save row 2/i }))
  await waitFor(() => screen.getByText(/edited/i))

  fireEvent.click(screen.getByRole('button', { name: /remove row 2/i }))

  await waitFor(() => {
    expect(screen.queryByText(/edited/i)).not.toBeInTheDocument()
  })
})

it('"Edited" badge does not appear while row is still in editing state', async () => {
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
  renderPage()
  uploadFile()
  await waitFor(() => screen.getByRole('button', { name: /edit row 2/i }))

  fireEvent.click(screen.getByRole('button', { name: /edit row 2/i }))
  await waitFor(() => screen.getByLabelText(/row 2 amount/i))

  // Still editing — badge must not appear yet
  expect(screen.queryByText(/edited/i)).not.toBeInTheDocument()
})
```

---

### New Tests — Item 4 (Sort Toggle)

```tsx
it('shows sort toggle button in preview header area', async () => {
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
  renderPage()
  uploadFile()

  await waitFor(() => {
    expect(screen.getByRole('button', { name: /errors first/i })).toBeInTheDocument()
  })
})

it('clicking "Errors first" puts error rows before valid rows in DOM', async () => {
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
  renderPage()
  uploadFile()
  await waitFor(() => screen.getByRole('button', { name: /errors first/i }))

  fireEvent.click(screen.getByRole('button', { name: /errors first/i }))

  await waitFor(() => {
    const rows = screen.getAllByRole('row')
    // First data row (index 1 — after thead) should be the error row (rowNumber 2)
    expect(rows[1]).toHaveTextContent('2')
    // Second data row should be the valid row (rowNumber 1)
    expect(rows[2]).toHaveTextContent('1')
  })
})

it('clicking "Row order" after sort restores original order', async () => {
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
  renderPage()
  uploadFile()
  await waitFor(() => screen.getByRole('button', { name: /errors first/i }))

  fireEvent.click(screen.getByRole('button', { name: /errors first/i }))
  await waitFor(() => screen.getByRole('button', { name: /row order/i }))

  fireEvent.click(screen.getByRole('button', { name: /row order/i }))

  await waitFor(() => {
    const rows = screen.getAllByRole('row')
    expect(rows[1]).toHaveTextContent('1') // valid row first (original order)
    expect(rows[2]).toHaveTextContent('2') // error row second
  })
})

it('sort resets to row order when a new file is uploaded', async () => {
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewWithErrors })
  renderPage()
  uploadFile()
  await waitFor(() => screen.getByRole('button', { name: /errors first/i }))

  fireEvent.click(screen.getByRole('button', { name: /errors first/i }))
  await waitFor(() => screen.getByRole('button', { name: /row order/i }))

  // Upload a second file
  mockPreviewCsvImport.mockResolvedValue({ ok: true, data: previewAllValid })
  uploadFile()

  await waitFor(() => {
    // After new upload, sort button label should reset to "Errors first"
    expect(screen.getByRole('button', { name: /errors first/i })).toBeInTheDocument()
  })
})
```

---

### New Tests — Item 5 (Template Button)

The existing test `'renders upload dropzone and template link'` still passes unchanged (the `<a>` keeps `role="link"` and same `href`). Add one new assertion:

```tsx
it('shows template column description text alongside download link', () => {
  renderPage()
  expect(screen.getByText(/expected columns/i)).toBeInTheDocument()
})
```

---

### New Tests — Item 6 (Status Icons)

Status icons are decorative SVGs — test via the surrounding `<span>` accessible text which already existed. No additional assertions needed beyond what the existing tests cover. The status text ("Valid", "Invalid amount") remains in the DOM alongside the icons. Existing tests such as `'shows error codes in status column'` and `'updates row in preview after per-row validation succeeds'` implicitly cover correct status text.

---

## Implementation Order

1. **Item 5** — template button (no state, safe warmup)
2. **Item 1** — upload spinner (simple state-driven UI)
3. **Item 6** — status icons (UI-only, no logic)
4. **Item 3** — edited badge (one new state + prop thread)
5. **Item 4** — sort toggle (one new state + `displayRows` compute)
6. **Item 2** — nav warning (two hooks + modal, touches routing — do last)

After all items implemented, run tests to confirm all pass:
```bash
npm test -- --run CsvImportPage
```

---

## Finish

After implementing all 6 items and confirming tests pass, run:

```
/done
```
