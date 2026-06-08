import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: Number(process.env.VITE_PORT) || 5174,
    strictPort: true,
    proxy: {
      '/api': {
        target: process.env.VITE_API_BASE_URL || 'https://localhost',
        changeOrigin: true,
        secure: false,
        // Follow 301/302s server-side — prevents browser from following http→https
        // redirects directly (which breaks SameSite=Strict cookies across schemes)
        followRedirects: true,
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      exclude: [
        'src/**/__tests__/**',
        'src/test-setup.ts',
        'src/main.tsx',
        'src/i18n/**',
      ],
    },
  },
})
