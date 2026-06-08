import { IonApp, setupIonicReact } from '@ionic/react'
import { BrowserRouter } from 'react-router-dom'
import { AppProviders } from '@/providers/AppProviders'
import { AppRouter } from '@/router'

setupIonicReact()

export default function App() {
  return (
    <IonApp>
      <BrowserRouter>
        <AppProviders>
          <AppRouter />
        </AppProviders>
      </BrowserRouter>
    </IonApp>
  )
}
