import { useEffect } from 'react'

const SUFFIX = 'Expenses Manager'

export function usePageTitle(title?: string) {
  useEffect(() => {
    document.title = title ? `${title} — ${SUFFIX}` : SUFFIX
  }, [title])
}
