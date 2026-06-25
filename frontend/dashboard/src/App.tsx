import { RouterProvider } from 'react-router-dom'
import { router } from '@/router'

export default function App() {
  return (
    <div className="min-h-screen flex flex-col bg-surface-page font-sans">
      <RouterProvider router={router} />
    </div>
  )
}
