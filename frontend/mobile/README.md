# ExpensesManager — Mobile App

Ionic + Capacitor + React native mobile app for iOS and Android. Shares the same backend as the web dashboard (`frontend/dashboard/`) via cookie-based auth.

---

## Overview

| Concern | Choice |
|---|---|
| UI framework | `@ionic/react` v8 |
| Native wrapper | `@capacitor/core` v7 + iOS/Android |
| Routing | `@ionic/react-router` (React Router v6) |
| Server state | `@tanstack/react-query` v5 |
| Forms | `react-hook-form` v7 + Zod v4 |
| i18n | `i18next` + `react-i18next` (en/fr/es/de) |
| Real-time | `@microsoft/signalr` |
| Offline queue | `idb` v8 (IndexedDB) |
| Build | Vite |
| Tests | Vitest + `@testing-library/react` |

---

## Prerequisites

- Node 20+, npm 10+
- `@capacitor/cli` (`npm i -g @capacitor/cli`)
- Android Studio (Android target)
- Xcode + macOS (iOS target)

---

## Local development

```bash
cd frontend/mobile
npm ci

# Point at your running nginx endpoint
# (create .env.local)
echo "VITE_API_BASE_URL=https://localhost" > .env.local

npm run dev        # Vite dev server at http://localhost:5174
```

---

## Running on device / emulator

```bash
npm run build              # Vite production build → dist/
npx cap add android        # first time only
npx cap add ios            # first time only
npx cap sync               # copy dist/ into native projects
npx cap open android       # open Android Studio
npx cap open ios           # open Xcode (macOS only)
```

---

## Environment variables

| Variable | Default | Description |
|---|---|---|
| `VITE_API_BASE_URL` | `""` (relative) | Full base URL of the nginx proxy (e.g. `https://localhost`) |
| `VITE_APPLICATION_CODE` | `EXPENSES_MANAGER` | Application code sent with auth requests |
| `VITE_PORT` | `5174` | Vite dev server port |

---

## Project structure

```
src/
├── App.tsx                    IonApp root
├── router.tsx                 IonTabs (5 tabs) + auth guard
├── theme/variables.css        Ionic CSS vars mapped to Hearth tokens
├── main.tsx                   Entry point
├── test-setup.ts              Vitest global mocks for Capacitor plugins
├── i18n/                      i18next config + locale JSON (en/fr/es/de)
├── providers/AppProviders.tsx Provider composition
├── services/api.service.ts    HTTP client (adapts VITE_API_BASE_URL)
├── types/api.type.ts          Shared ApiResponse<T> type
├── constants/                 Error code maps
├── hooks/
│   ├── useOfflineQueue.ts     IndexedDB queue for offline expense adds
│   └── useNetworkSync.ts      Capacitor Network → drain queue on reconnect
└── features/
    ├── auth/                  LoginPage + AuthContext + schemas + service
    ├── expenses/              ExpensesListPage + QuickAddModal + service
    ├── dashboard/             DashboardPage + service
    ├── families/              FamiliesPage + FamilyContext + service
    ├── notifications/         NotificationContext + NotificationBell + service
    ├── settings/              SettingsPage + service
    ├── currencies/            DisplayCurrencyContext + service
    └── tags/                  service + types
```

---

## Shared code policy

The following are **copied** from `frontend/dashboard/src/` (not symlinked) so each project has an independent build:

- All `features/*/services/*.ts` — API service functions
- All `features/*/types/*.ts` — TypeScript types
- `features/expenses/expense.schemas.ts`
- `i18n/locales/**` — all 4 locale JSON files
- `constants/apiErrors.constant.ts`
- `types/api.type.ts`

`api.service.ts` is adapted: uses `VITE_API_BASE_URL` instead of `VITE_API_BASE`.

---

## Offline queue

When the device is offline, quick-add expenses are stored in IndexedDB (`expense-manager` DB, `offline-expense-queue` store). On reconnect, `useNetworkSync` drains the queue by calling `addExpense` for each item in order.

To inspect the queue in Chrome DevTools: **Application → IndexedDB → expense-manager → offline-expense-queue**.

---

## Notifications

SignalR (`/api/notifications/ws/notifications`) connects on login and shows foreground toasts. Push tokens are registered via `POST /api/notifications/push-token` (Phase 14 stub — FCM/APNs dispatch is Phase 15).

---

## Tests

```bash
npm test            # Vitest watch mode
npm run test:ci     # Single run with coverage
```

Capacitor plugins are mocked globally in `src/test-setup.ts`. IDB tests use `fake-indexeddb`.

---

## Production build

```bash
npm run build
npx cap sync
```

**Android:** open Android Studio → build signed APK/AAB (set keystore in `android/app/build.gradle`).

**iOS:** open Xcode → set signing team + bundle ID → Archive.

---

## Known limitations

- Receipt photo upload is Phase 14 preview only — stored as base64 locally; backend upload is Phase 15.
- Push notification dispatch (FCM/APNs) deferred to Phase 15; token registration is stubbed.
- Admin screens are web-only — not included in this app.
