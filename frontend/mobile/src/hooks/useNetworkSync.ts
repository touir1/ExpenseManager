import { useEffect, useRef, useState } from 'react'
import { useOfflineQueue } from '@/hooks/useOfflineQueue'

export type SyncResult = { ok: number; failed: number } | null

export function useNetworkSync() {
  const [isOnline, setIsOnline] = useState(true)
  const [lastSync, setLastSync] = useState<SyncResult>(null)
  const { drain } = useOfflineQueue()
  const drainRef = useRef(drain)
  drainRef.current = drain

  useEffect(() => {
    let listenerHandle: { remove: () => void } | null = null

    async function setup() {
      try {
        const { Network } = await import('@capacitor/network')

        const status = await Network.getStatus()
        setIsOnline(status.connected)

        const handle = await Network.addListener('networkStatusChange', async ({ connected }) => {
          setIsOnline(connected)
          if (connected) {
            const result = await drainRef.current()
            setLastSync(result)
          }
        })
        listenerHandle = handle
      } catch {
        // Fall back to browser navigator.onLine
        setIsOnline(navigator.onLine)
        const handleOnline = async () => {
          setIsOnline(true)
          const result = await drainRef.current()
          setLastSync(result)
        }
        const handleOffline = () => setIsOnline(false)
        window.addEventListener('online', handleOnline)
        window.addEventListener('offline', handleOffline)
      }
    }

    setup()

    return () => {
      listenerHandle?.remove()
    }
  }, [])

  return { isOnline, lastSync }
}
