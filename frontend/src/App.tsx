import { useState } from 'react'
import Login from './pages/Login'
import Perfil from './pages/Perfil'
import Registro from './pages/Registro'
import { cerrarSesion, type Sesion } from './services/api'

type PantallaPublica = 'login' | 'registro'

export default function App() {
  // La sesión vive solo en memoria: al recargar la página hay que volver a ingresar.
  const [sesion, setSesion] = useState<Sesion | null>(null)
  const [pantalla, setPantalla] = useState<PantallaPublica>('login')

  if (sesion) {
    return (
      <Perfil
        sesion={sesion}
        onCerrarSesion={() => {
          // Si el backend no responde igual salimos: el token expira solo en 2 horas.
          cerrarSesion(sesion.token).catch(() => {})
          setSesion(null)
          setPantalla('login')
        }}
      />
    )
  }

  return pantalla === 'login' ? (
    <Login onIngreso={setSesion} onIrARegistro={() => setPantalla('registro')} />
  ) : (
    <Registro onRegistro={setSesion} onIrALogin={() => setPantalla('login')} />
  )
}
