# Plan: Section 6 — Settings (Web) — Theme Toggle Refactor

> Scope: `frontend/dashboard`  
> Priority: 🟡 Medium  
> From: `docs/plans/ux-ui-improvements.md` §6  
> UX/UI: use `/ui-ux-pro-max` skill before implementing any UI component or making visual design decisions

---

## Problem

`ThemeToggle` appears in two places:
1. **NavBar user dropdown** — `<ThemeToggle showLabel={false} />` (lines 209–212 of `NavBar.tsx`)
2. **SettingsPage** — `<ThemeToggle />` in the Appearance card (line 152 of `SettingsPage.tsx`)

Duplication creates confusion about which is canonical. The dropdown slot is wasted on a toggle that already lives in Settings.

---

## Solution

- **Remove** `ThemeToggle` from the user dropdown.
- **Add** a standalone `NavBarThemeButton` icon-only button directly in the navbar (right-side controls row, after `NotificationBell`, before the avatar button).
- **Keep** `ThemeToggle` in `SettingsPage` — that remains the canonical 3-way control.
- **Default** to `'system'` when no preference saved (already the case in `ThemeContext.tsx`).

### NavBarThemeButton behavior

| Current theme | Icon shown | `title` tooltip | Click → next theme |
|---|---|---|---|
| `'system'` | monitor SVG | "Switch to light mode" | `'light'` |
| `'light'` | sun SVG | "Switch to dark mode" | `'dark'` |
| `'dark'` | moon SVG | "Back to system default" | `'system'` |

Cycle: `system → light → dark → system`

Button styling: `h-8 w-8 rounded-lg text-ink-mute hover:text-ink hover:bg-surface-subtle transition-colors duration-150` — matches other utility icon buttons (no brand background). Meets 32×32px visual size; expand hit area via padding if needed.

---

## Files to Change

| File | Change |
|---|---|
| `src/components/NavBarThemeButton.tsx` | **Create** — new icon-only theme cycle button |
| `src/layouts/NavBar.tsx` | Remove dropdown ThemeToggle; add NavBarThemeButton to right-side controls |
| `src/i18n/locales/en/translation.json` | Add `nav.toggleThemeLight`, `nav.toggleThemeDark`, `nav.toggleThemeSystem` |
| `src/i18n/locales/fr/translation.json` | Same 3 keys in French |
| `src/i18n/locales/es/translation.json` | Same 3 keys in Spanish |
| `src/i18n/locales/de/translation.json` | Same 3 keys in German |
| `src/layouts/__tests__/NavBar.test.tsx` | Add theme button tests |
| `src/components/__tests__/NavBarThemeButton.test.tsx` | **Create** — unit tests for new component |

`SettingsPage.tsx` and `ThemeToggle.tsx` are **unchanged**.

---

## Implementation Steps

### 1. Create `NavBarThemeButton.tsx`

**File:** `frontend/dashboard/src/components/NavBarThemeButton.tsx`

```tsx
import { useTranslation } from 'react-i18next'
import { useTheme, type Theme } from '@/features/settings/ThemeContext'

const CYCLE: Record<Theme, Theme> = {
  system: 'light',
  light: 'dark',
  dark: 'system',
}

const TOOLTIP_KEY: Record<Theme, string> = {
  system: 'nav.toggleThemeLight',
  light: 'nav.toggleThemeDark',
  dark: 'nav.toggleThemeSystem',
}

export default function NavBarThemeButton() {
  const { theme, setTheme } = useTheme()
  const { t } = useTranslation()

  return (
    <button
      onClick={() => setTheme(CYCLE[theme])}
      aria-label={t(TOOLTIP_KEY[theme])}
      title={t(TOOLTIP_KEY[theme])}
      className="h-8 w-8 rounded-lg text-ink-mute hover:text-ink hover:bg-surface-subtle transition-colors duration-150 flex items-center justify-center cursor-pointer"
    >
      {theme === 'light' && (
        // Sun icon
        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M12 3v1m0 16v1m8.66-9h-1M4.34 12h-1m15.07-6.07-.71.71M6.34 17.66l-.71.71m12.73 0-.71-.71M6.34 6.34l-.71-.71M12 7a5 5 0 100 10A5 5 0 0012 7z" />
        </svg>
      )}
      {theme === 'dark' && (
        // Moon icon
        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
        </svg>
      )}
      {theme === 'system' && (
        // Monitor icon
        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
          <path strokeLinecap="round" strokeLinejoin="round" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
        </svg>
      )}
    </button>
  )
}
```

---

### 2. Update `NavBar.tsx`

**File:** `frontend/dashboard/src/layouts/NavBar.tsx`

**a) Replace import** — swap `ThemeToggle` for `NavBarThemeButton`:
```diff
-import ThemeToggle from '@/components/ThemeToggle'
+import NavBarThemeButton from '@/components/NavBarThemeButton'
```

**b) Add button to right-side controls** — after `<NotificationBell />`, before the avatar `<div className="relative" ref={userMenuRef}>`:
```diff
  <NotificationBell />
+ <NavBarThemeButton />
  <div className="relative" ref={userMenuRef}>
```

**c) Remove ThemeToggle from user dropdown** — delete lines 209–212:
```diff
-  <div className="px-3 py-1.5 flex items-center gap-2">
-    <span className="text-sm font-semibold text-ink-mute shrink-0">{t('settings.theme.label')}</span>
-    <ThemeToggle showLabel={false} />
-  </div>
```
Also remove the `<div className="border-t border-surface-border my-1" />` separator above it if it becomes a double-separator (verify visually).

---

### 3. Add i18n keys

Add to **all 4 locale files** under `"nav"`:

**`en/translation.json`**
```json
"toggleThemeLight": "Switch to light mode",
"toggleThemeDark": "Switch to dark mode",
"toggleThemeSystem": "Back to system default"
```

**`fr/translation.json`**
```json
"toggleThemeLight": "Passer en mode clair",
"toggleThemeDark": "Passer en mode sombre",
"toggleThemeSystem": "Revenir au thème système"
```

**`es/translation.json`**
```json
"toggleThemeLight": "Cambiar a modo claro",
"toggleThemeDark": "Cambiar a modo oscuro",
"toggleThemeSystem": "Volver al tema del sistema"
```

**`de/translation.json`**
```json
"toggleThemeLight": "Zum hellen Modus wechseln",
"toggleThemeDark": "Zum dunklen Modus wechseln",
"toggleThemeSystem": "Zur Systemvorgabe zurück"
```

---

### 4. Unit tests — `NavBarThemeButton.test.tsx`

**File:** `frontend/dashboard/src/components/__tests__/NavBarThemeButton.test.tsx`

```tsx
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import NavBarThemeButton from '@/components/NavBarThemeButton'

const mockSetTheme = vi.fn()

vi.mock('@/features/settings/ThemeContext', () => ({
  useTheme: () => ({ theme: mockTheme, setTheme: mockSetTheme }),
}))
```

Tests to cover:

| Test | Assertion |
|---|---|
| renders monitor icon when theme is `'system'` | button present, aria-label = "Switch to light mode" |
| renders sun icon when theme is `'light'` | button present, aria-label = "Switch to dark mode" |
| renders moon icon when theme is `'dark'` | button present, aria-label = "Back to system default" |
| click when `'system'` calls `setTheme('light')` | `mockSetTheme` called with `'light'` |
| click when `'light'` calls `setTheme('dark')` | `mockSetTheme` called with `'dark'` |
| click when `'dark'` calls `setTheme('system')` | `mockSetTheme` called with `'system'` |
| button has `title` matching `aria-label` | `title === aria-label` attribute |

**Implementation note:** Since `mockTheme` must vary per test, use a module-level `let mockTheme: Theme` that each test sets before rendering, combined with `vi.mock` factory that reads it:

```tsx
let mockTheme: Theme = 'system'
vi.mock('@/features/settings/ThemeContext', () => ({
  useTheme: () => ({ theme: mockTheme, setTheme: mockSetTheme }),
}))
```

---

### 5. Update `NavBar.test.tsx`

**File:** `frontend/dashboard/src/layouts/__tests__/NavBar.test.tsx`

Add a new describe block `theme toggle button`:

```tsx
describe('theme toggle button', () => {
  beforeEach(() => {
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      logout: vi.fn(),
      user: { firstName: 'John', lastName: 'Doe' },
    })
  })

  it('renders when authenticated', () => {
    // mock returns theme: 'system' already
    renderNavBar('/dashboard')
    // aria-label = "Switch to light mode" (system → light tooltip)
    expect(screen.getByRole('button', { name: /switch to light mode/i })).toBeInTheDocument()
  })

  it('does not render when unauthenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false, logout: vi.fn() })
    renderNavBar('/')
    expect(screen.queryByRole('button', { name: /switch to light mode/i })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /switch to dark mode/i })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /back to system default/i })).not.toBeInTheDocument()
  })

  it('theme toggle is not inside the user dropdown', () => {
    renderNavBar('/dashboard')
    // The dropdown ThemeToggle (3-segment) should not exist in NavBar anymore
    // Verify no role=group with label "Appearance" or "Theme" exists
    expect(screen.queryByRole('group', { name: /theme/i })).not.toBeInTheDocument()
  })
})
```

**Also update** the existing `vi.mock('@/features/settings/ThemeContext')` in the file to keep returning `{ theme: 'system', setTheme: vi.fn() }` (already done, no change needed).

---

## UX/UI Design Notes (from ui-ux-pro-max)

- **Touch target:** `h-8 w-8` = 32×32px, below 44px minimum. Acceptable for desktop-first navbar; same as NotificationBell and avatar button. No change needed (consistent with existing pattern).
- **Tooltip:** `title` attribute provides native browser tooltip on hover — sufficient for an icon-only button at this scale. No custom tooltip component needed.
- **No background on idle:** matches `text-ink-mute hover:text-ink hover:bg-surface-subtle` — same as other utility icon buttons. The Add Expense button uses `bg-brand-500` (primary action), theme toggle is secondary/utility.
- **Icon-only with `aria-label`:** satisfies WCAG 2.1 SC 1.1.1 (non-text content) and SC 4.1.2 (name/role/value). ✅
- **State clarity (ui-ux-pro-max rule `state-clarity`):** Each icon unambiguously communicates current state. Tooltip communicates what the next click will do (action-forward label pattern).
- **Animation:** No transition on icon swap needed — instant swap is fine for a mode toggle; user just clicked the button so no surprise. If desired, `transition-opacity duration-150` can be added but is not required.

---

## Completion

After implementation, run:
```bash
npm test -- --reporter=verbose
```

Then run:
```
/done
```
