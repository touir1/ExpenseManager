import type { Config } from 'tailwindcss'

const config: Config = {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      fontFamily: {
        sans: [
          'Manrope',
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
        serif: [
          '"Instrument Serif"',
          '"Iowan Old Style"',
          'Georgia',
          'serif',
        ],
        mono: [
          '"JetBrains Mono"',
          'ui-monospace',
          '"SF Mono"',
          'Menlo',
          'monospace',
        ],
      },
      colors: {
        // Clay/Terracotta — primary brand accent
        brand: {
          50:  '#FDF4EF',
          100: '#F9E8DD',
          200: '#F2D6C4',
          300: '#E8B89A',
          400: '#D97D60',
          500: '#C8623E',  // clay — primary
          600: '#A24B2A',  // clayDeep
          700: '#7D3820',
          800: '#5E2817',
          900: '#3E1A0F',
          950: '#1E0C06',
        },
        // Warm cream surfaces — CSS-variable-driven so dark mode auto-adapts
        surface: {
          page:   'var(--color-surface-page)',
          card:   'var(--color-surface-card)',
          subtle: 'var(--color-surface-subtle)',
          border: 'var(--color-surface-border)',
          muted:  'var(--color-surface-muted)',
        },
        // Deep cocoa ink hierarchy — CSS-variable-driven
        ink: {
          DEFAULT: 'var(--color-ink)',
          body:    'var(--color-ink-body)',
          mute:    'var(--color-ink-mute)',
          faint:   'var(--color-ink-faint)',
        },
        // Signal palette — .soft uses CSS vars so dark mode overrides apply
        sage: {
          DEFAULT: '#6B8E5A',
          soft:    'var(--color-sage-soft)',
        },
        berry: {
          DEFAULT: '#B5443F',
          soft:    'var(--color-berry-soft)',
        },
        mustard: {
          DEFAULT: '#D6A23F',
          soft:    'var(--color-mustard-soft)',
        },
        sky: {
          DEFAULT: '#5C8C9E',
          soft:    'var(--color-sky-soft)',
        },
      },
      borderRadius: {
        xl:    '0.75rem',
        '2xl': '1rem',
        '3xl': '1.5rem',
      },
      boxShadow: {
        card:      '0 1px 0 rgba(0,0,0,0.02), 0 12px 28px -22px rgba(60,30,10,0.18)',
        'card-md': '0 4px 6px -1px rgba(60,30,10,0.10), 0 2px 4px -2px rgba(60,30,10,0.08)',
        warm:      '0 8px 20px -10px rgba(30,20,10,0.5)',
      },
    },
  },
  plugins: [],
}

export default config
