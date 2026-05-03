import { describe, it, expect } from 'vitest'
import i18next from 'i18next'
import '../index'

describe('i18n initialization', () => {
  it('initializes with supported languages', () => {
    expect(i18next.options.supportedLngs).toEqual(expect.arrayContaining(['en', 'fr', 'es', 'de']))
  })

  it('uses en as fallback language', () => {
    expect(i18next.options.fallbackLng).toEqual(['en'])
  })

  it('has all language resources loaded', () => {
    expect(i18next.hasResourceBundle('en', 'translation')).toBe(true)
    expect(i18next.hasResourceBundle('fr', 'translation')).toBe(true)
    expect(i18next.hasResourceBundle('es', 'translation')).toBe(true)
    expect(i18next.hasResourceBundle('de', 'translation')).toBe(true)
  })

  it('disables html escaping in interpolation', () => {
    expect((i18next.options.interpolation as { escapeValue: boolean }).escapeValue).toBe(false)
  })
})
