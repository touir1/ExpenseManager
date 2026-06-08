import { IonApp, IonReactRouter, setupIonicReact } from '@ionic/react'
import { AppProviders } from '@/providers/AppProviders'
import { AppRouter } from '@/router'

setupIonicReact()

export default function App() {
  return (
    <IonApp>
      <IonReactRouter>
        <AppProviders>
          <AppRouter />
        </AppProviders>
      </IonReactRouter>
    </IonApp>
  )
}
