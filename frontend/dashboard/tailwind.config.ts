import type { Config } from 'tailwindcss'

/**
 * Expenses Manager – Tailwind CSS v3 Configuration
 *
 * Palette: soft neutral base (slate) + muted indigo accent.
 * All colors are perceptually gentle – no harsh/poppy hues.
 *
 * Alternative palettes to swap in:
 *  A) Warm stone + teal accent  → replace brand with teal-* values, surface with stone-*
 *  B) Cool zinc + violet accent → replace brand with violet-* values, surface with zinc-*
 *  C) Slate + sky accent        → replace brand with sky-* values (already softer than indigo)
 */
const config: Config = {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        // Inter first, with full system-ui fallback chain
        sans: [
          'Inter',
          'ui-sans-serif',
          'system-ui',
          '-apple-system',
          'BlinkMacSystemFont',
          '"Segoe UI"',
          'Roboto',
          '"Helvetica Neue"',
          'Arial',
          'sans-serif',
        ],
      },
      colors: {
        // ── Brand (muted indigo) ───────────────────────────────────────
        brand: {
          50:  '#eef2ff',
          100: '#e0e7ff',
          200: '#c7d2fe',
          300: '#a5b4fc',
          400: '#818cf8',
          500: '#6366f1',
          600: '#4f46e5',   // primary action colour
          700: '#4338ca',
          800: '#3730a3',
          900: '#312e81',
          950: '#1e1b4b',
        },
        // ── Surface neutrals (slate) ───────────────────────────────────
        surface: {
          page:   '#f8fafc',   // slate-50  – page background
          card:   '#ffffff',   // white     – card / modal background
          subtle: '#f1f5f9',   // slate-100 – subtle fills, hover states
          border: '#e2e8f0',   // slate-200 – default borders
          muted:  '#cbd5e1',   // slate-300 – dividers, disabled borders
        },
      },
      borderRadius: {
        xl:  '0.75rem',
        '2xl': '1rem',
        '3xl': '1.5rem',
      },
      boxShadow: {
        card: '0 1px 3px 0 rgb(0 0 0 / 0.06), 0 1px 2px -1px rgb(0 0 0 / 0.06)',
        'card-md': '0 4px 6px -1px rgb(0 0 0 / 0.07), 0 2px 4px -2px rgb(0 0 0 / 0.07)',
      },
    },
  },
  plugins: [],
}

export default config
