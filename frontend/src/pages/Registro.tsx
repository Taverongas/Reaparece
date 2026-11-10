import { useState, type FormEvent } from 'react'
import Encabezado from '../components/Encabezado'
import { Campo, MensajeError, claseBoton, claseInput } from '../components/formulario'
import { ApiError, COMUNIDADES, registrar, type RegistroDatos, type Sesion } from '../services/api'

interface Props {
  onRegistro: (sesion: Sesion) => void
  onIrALogin: () => void
}

const CORREO_VALIDO = /^[^@\s]+@[^@\s]+\.[^@\s]+$/

const datosIniciales: RegistroDatos = {
  nombreCompleto: '',
  correo: '',
  contrasena: '',
  confirmarContrasena: '',
  comunidadPrincipal: '',
  aceptaHabeasData: false,
}

function validar(d: RegistroDatos): string | null {
  if (!d.nombreCompleto.trim() || !d.correo.trim() || !d.contrasena || !d.confirmarContrasena || !d.comunidadPrincipal)
    return 'Completa todos los campos'
  if (!CORREO_VALIDO.test(d.correo.trim())) return 'Ingresa un correo válido'
  if (d.contrasena.length < 8) return 'La contraseña debe tener mínimo 8 caracteres'
  if (d.contrasena !== d.confirmarContrasena) return 'Las contraseñas no coinciden'
  if (!d.aceptaHabeasData) return 'Debes aceptar el tratamiento de datos personales para continuar'
  return null
}

export default function Registro({ onRegistro, onIrALogin }: Props) {
  const [datos, setDatos] = useState(datosIniciales)
  const [error, setError] = useState<string | null>(null)
  const [enviando, setEnviando] = useState(false)

  function actualizar<K extends keyof RegistroDatos>(campo: K, valor: RegistroDatos[K]) {
    setDatos((d) => ({ ...d, [campo]: valor }))
  }

  async function manejarEnvio(e: FormEvent) {
    e.preventDefault()
    const errorValidacion = validar(datos)
    setError(errorValidacion)
    if (errorValidacion) return

    setEnviando(true)
    try {
      onRegistro(await registrar(datos))
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Ocurrió un error inesperado.')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="min-h-svh">
      <Encabezado subtitulo="Únete a tu comunidad y ayuda a que las cosas vuelvan a su dueño." />

      <main className="mx-auto max-w-md px-4 py-6">
        <form onSubmit={manejarEnvio} noValidate className="space-y-4 rounded-2xl bg-white p-6 shadow-sm ring-1 ring-arena">
          <h2 className="font-serif text-2xl text-bosque">Crear cuenta</h2>

          <Campo etiqueta="Nombre completo">
            <input
              autoComplete="name"
              className={claseInput}
              value={datos.nombreCompleto}
              onChange={(e) => actualizar('nombreCompleto', e.target.value)}
            />
          </Campo>

          <Campo etiqueta="Correo">
            <input
              type="email"
              autoComplete="email"
              className={claseInput}
              value={datos.correo}
              onChange={(e) => actualizar('correo', e.target.value)}
            />
          </Campo>

          <Campo etiqueta="Contraseña">
            <input
              type="password"
              autoComplete="new-password"
              className={claseInput}
              value={datos.contrasena}
              onChange={(e) => actualizar('contrasena', e.target.value)}
            />
            <span className="mt-1 block text-xs text-stone-500">Mínimo 8 caracteres.</span>
          </Campo>

          <Campo etiqueta="Confirmar contraseña">
            <input
              type="password"
              autoComplete="new-password"
              className={claseInput}
              value={datos.confirmarContrasena}
              onChange={(e) => actualizar('confirmarContrasena', e.target.value)}
            />
          </Campo>

          <Campo etiqueta="Comunidad principal">
            <select
              className={claseInput}
              value={datos.comunidadPrincipal}
              onChange={(e) => actualizar('comunidadPrincipal', e.target.value)}
            >
              <option value="" disabled>
                Selecciona tu comunidad
              </option>
              {COMUNIDADES.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </Campo>

          <div className="rounded-xl bg-crema p-4 text-xs leading-relaxed text-stone-600 ring-1 ring-arena">
            <p className="mb-2 font-semibold text-stone-700">Tratamiento de datos personales</p>
            <p>
              En cumplimiento de la Ley 1581 de 2012 (habeas data) y sus decretos reglamentarios, Reaparece tratará tus
              datos personales (nombre, correo y comunidad) únicamente para gestionar tu cuenta y la publicación y
              recuperación de objetos perdidos en tus comunidades. Puedes conocer, actualizar, rectificar y solicitar la
              supresión de tus datos en cualquier momento.
            </p>
          </div>

          <label className="flex items-start gap-3 text-sm text-stone-700">
            <input
              type="checkbox"
              className="mt-0.5 size-5 shrink-0 accent-bosque"
              checked={datos.aceptaHabeasData}
              onChange={(e) => actualizar('aceptaHabeasData', e.target.checked)}
            />
            <span>Acepto el tratamiento de mis datos personales según la Ley 1581 de 2012.</span>
          </label>

          <MensajeError mensaje={error} />

          <button type="submit" className={claseBoton} disabled={enviando || !datos.aceptaHabeasData}>
            {enviando ? 'Creando cuenta…' : 'Crear cuenta'}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-stone-600">
          ¿Ya tienes cuenta?{' '}
          <button type="button" onClick={onIrALogin} className="font-semibold text-bosque underline-offset-2 hover:underline">
            Inicia sesión
          </button>
        </p>
      </main>
    </div>
  )
}
