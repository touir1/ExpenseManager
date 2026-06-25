# Plan: Section 6 — Settings (Web) — Theme Toggle Refactor

> Scope: `frontend/dashboard`  
> Priority: 🟡 Medium  
> From: `docs/plans/ux-ui-improvements.md` §6  
> UX/UI: use `/ui-ux-pro-max` skill before implementing any UI component or making visual design decisions  
> **Status: ✅ Shipped in v0.122.0 — 2026-06-25**

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

| Stored theme | OS prefers dark | Icon shown | `title` tooltip | Click → sets theme |
|---|---|---|---|---|
| `'light'` | any | sun SVG | "Switch to dark mode" | `'dark'` |
| `'dark'` | any | moon SVG | "Switch to light mode" | `'light'` |
| `'system'` | false | sun SVG | "Switch to dark mode" | `'dark'` |
| `'system'` | true | moon SVG | "Switch to light mode" | `'light'` |

Toggle: `light ↔ dark` only. `'system'` never set from navbar. `'system'` stays available in SettingsPage only.

> Updated v0.122.1: original 3-state cycle replaced with 2-state toggle; system resolves via `window.matchMedia`.

Button styling: `h-8 w-8 rounded-lg text-ink-mute hover:text-ink hover:bg-surface-subtle transition-colors duration-150` — matches other utility icon buttons (no brand background). Meets 32×32px visual size; expand hit area via padding if needed.

---

## Files Changed

| File | Change |
|---|---|
| `src/components/NavBarThemeButton.tsx` | **Created** — new icon-only theme cycle button |
| `src/layouts/NavBar.tsx` | Removed dropdown ThemeToggle; added NavBarThemeButton to right-side controls |
| `src/i18n/locales/{en,fr,es,de}/translation.json` | Added `nav.toggleThemeLight`, `nav.toggleThemeDark`, `nav.toggleThemeSystem` |
| `src/layouts/__tests__/NavBar.test.tsx` | Added `theme toggle button` describe block (4 tests) |
| `src/components/__tests__/NavBarThemeButton.test.tsx` | **Created** — 8 unit tests |

`SettingsPage.tsx` and `ThemeToggle.tsx` — **unchanged**.

---

## UX/UI Design Notes (from ui-ux-pro-max)

- **Touch target:** `h-8 w-8` = 32×32px, below 44px minimum. Acceptable for desktop-first navbar; same as NotificationBell and avatar button. Consistent with existing pattern.
- **Tooltip:** `title` attribute provides native browser tooltip on hover — sufficient for icon-only button at this scale.
- **No background on idle:** matches `text-ink-mute hover:text-ink hover:bg-surface-subtle` — same as other utility icon buttons.
- **Icon-only with `aria-label`:** satisfies WCAG 2.1 SC 1.1.1 + SC 4.1.2. ✅
- **State clarity:** Each icon communicates current state. Tooltip communicates what next click will do (action-forward label pattern).
