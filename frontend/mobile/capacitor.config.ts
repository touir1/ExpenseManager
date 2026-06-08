import type { CapacitorConfig } from '@capacitor/cli'

const config: CapacitorConfig = {
  appId: 'com.touir.expensemanager',
  appName: 'ExpensesManager',
  webDir: 'dist',
  server: {
    androidScheme: 'https',
  },
}

export default config
