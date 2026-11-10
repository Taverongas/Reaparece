import { useState, type FormEvent } from 'react'
import Encabezado from '../components/Encabezado'
import { Campo, MensajeError, claseBoton, claseInput } from '../components/formulario'
import { ApiError, iniciarSesion, type Sesion } from '../services/api'

interface Props {
  onIngreso: (sesion: Sesion) => void
  onIrARegistro: () => void
}

export default function Login({ onIngreso, onIrARegistro }: Props) {
  const [correo, setCorreo] = useState('')
  const [contrasena, setContrasena] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [enviando, setEnviando] = useState(false)

  async function manejarEnvio(e: FormEvent) {
    e.preventDefault()
    setError(null)

    if (!correo.trim() || !contrasena) {
      setError('Completa todos los campos')
      return
    }

    setEnviando(true)
    try {
      onIngreso(await iniciarSesion(correo, contrasena))
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Ocurrió un error inesperado.')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="min-h-svh">
      <Encabezado subtitulo="Lo que se pierde en tu comunidad, reaparece." />

      <main className="mx-auto max-w-md px-4 py-6">
        <form onSubmit={manejarEnvio} noValidate className="space-y-4 rounded-2xl bg-white p-6 shadow-sm ring-1 ring-arena">
          <h2 className="font-serif text-2xl text-bosque">Iniciar sesión</h2>

          <Campo etiqueta="Correo">
            <input
              type="email"
              autoComplete="email"
              className={claseInput}
              value={correo}
              onChange={(e) => setCorreo(e.target.value)}
            />
          </Campo>

          <Campo etiqueta="Contraseña">
            <input
              type="password"
              autoComplete="current-password"
              className={claseInput}
              value={contrasena}
              onChange={(e) => setContrasena(e.target.value)}
            />
          </Campo>

          <MensajeError mensaje={error} />

          <button type="submit" className={claseBoton} disabled={enviando}>
            {enviando ? 'Ingresando…' : 'Ingresar'}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-stone-600">
          ¿No tienes cuenta?{' '}
          <button type="button" onClick={onIrARegistro} className="font-semibold text-bosque underline-offset-2 hover:underline">
            Regístrate
          </button>
        </p>
      </main>
    </div>
  )
}
