import { useEffect, useRef } from 'react'

/**
 * Restores focus to whatever element was focused right before this component mounted.
 * Intended for modals/dialogs that are only rendered while open (unmount = closed),
 * so any close path (Escape, backdrop, confirm, cancel) returns focus to the trigger.
 */
export function useReturnFocusOnUnmount() {
  const triggerRef = useRef<HTMLElement | null>(null)

  useEffect(() => {
    triggerRef.current = document.activeElement as HTMLElement | null
    return () => {
      triggerRef.current?.focus?.()
    }
  }, [])
}
