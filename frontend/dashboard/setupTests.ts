import '@testing-library/jest-dom/vitest'
import '@/i18n'
import { act } from '@testing-library/react'

globalThis.IS_REACT_ACT_ENVIRONMENT = true
// React 18.3+ scheduler looks for globalThis.act when async updates fire outside act()
;(globalThis as unknown as Record<string, unknown>).act = act
