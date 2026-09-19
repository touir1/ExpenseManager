import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import { fileURLToPath, URL } from 'node:url'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const port = Number(env.VITE_PORT) || 5173
  const apiProxyTarget = env.VITE_API_PROXY_TARGET
  return {
    plugins: [react()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
    server: {
      port,
      strictPort: true,
      // Proxying /api same-origin (instead of the app fetching an absolute cross-scheme URL
      // like https://localhost directly) keeps the dev server and API same-site from the
      // browser's perspective. Without this, Chrome's Schemeful Same-Site treats
      // http://localhost:5173 -> https://localhost as cross-site, and the auth cookie
      // (SameSite=Lax/Strict) gets silently stripped from every XHR/fetch after login,
      // even though the top-level navigation and Set-Cookie both go through fine — the
      // session check then 401s with MISSING_TOKEN and login looks like it silently fails.
      proxy: apiProxyTarget
        ? {
            '/api': {
              target: apiProxyTarget,
              changeOrigin: true,
              secure: false,
            },
          }
        : undefined,
    },
  }
})
