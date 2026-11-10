import { useEffect, useState } from 'react'
import Encabezado from '../components/Encabezado'
import { claseBoton } from '../components/formulario'
import { ApiError, obtenerPerfil, type Perfil as PerfilDatos, type Sesion } from '../services/api'

interface Props {
  sesion: Sesion
  onCerrarSesion: () => void
}

function iniciales(nombre: string) {
  return nombre
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0].toUpperCase())
    .join('')
}

function Contador({ valor, etiqueta }: { valor: number; etiqueta: string }) {
  return (
    <div className="rounded-2xl bg-white p-4 text-center shadow-sm ring-1 ring-arena">
      <p className="font-serif text-3xl font-bold text-bosque">{valor}</p>
      <p className="mt-1 text-xs text-stone-600">{etiqueta}</p>
    </div>
  )
}

export default function Perfil({ sesion, onCerrarSesion }: Props) {
  const { usuario, token } = sesion
  const [perfil, setPerfil] = useState<PerfilDatos | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    obtenerPerfil(token)
      .then(setPerfil)
      .catch((err) => setError(err instanceof ApiError ? err.message : 'No pudimos cargar tu perfil.'))
  }, [token])

  return (
    <div className="min-h-svh">
      <Encabezado subtitulo="Mi perfil" />

      <main className="mx-auto max-w-md space-y-6 px-4 py-6">
        <section className="flex items-center gap-4 rounded-2xl bg-white p-5 shadow-sm ring-1 ring-arena">
          <div className="flex size-16 shrink-0 items-center justify-center rounded-full bg-bosque font-serif text-2xl text-crema">
            {iniciales(usuario.nombreCompleto)}
          </div>
          <div className="min-w-0 flex-1">
            <h2 className="truncate font-serif text-xl text-stone-900">{usuario.nombreCompleto}</h2>
            <p className="truncate text-sm text-stone-600">{usuario.correo}</p>
            {perfil && (
              <p className="mt-1 text-sm font-semibold text-stone-800" aria-label={`Reputación ${perfil.reputacion} de 5`}>
                <span className="text-amber-500">★</span> {perfil.reputacion.toFixed(1)}{' '}
                <span className="font-normal text-stone-500">/ 5</span>
              </p>
            )}
          </div>
        </section>

        {error && (
          <p role="alert" className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </p>
        )}

        {!perfil && !error && <p className="text-center text-sm text-stone-500">Cargando perfil…</p>}

        {perfil && (
          <>
            <section>
              <h3 className="mb-3 font-serif text-lg text-bosque">Mi actividad</h3>
              <div className="grid grid-cols-3 gap-3">
                <Contador valor={perfil.objetosPublicados} etiqueta="Publicados" />
                <Contador valor={perfil.objetosRecuperados} etiqueta="Recuperados" />
                <Contador valor={perfil.objetosReclamados} etiqueta="Reclamados" />
              </div>
            </section>

            <section>
              <h3 className="mb-3 font-serif text-lg text-bosque">Mis comunidades</h3>
              <ul className="divide-y divide-arena overflow-hidden rounded-2xl bg-white shadow-sm ring-1 ring-arena">
                {perfil.comunidades.map((c) => (
                  <li key={c.id} className="flex items-center justify-between gap-3 px-4 py-3">
                    <div className="min-w-0">
                      <p className="truncate font-medium text-stone-900">
                        {c.nombre}
                        {c.esPrincipal && <span className="ml-2 text-xs font-normal text-stone-500">(principal)</span>}
                      </p>
                      <p className="text-xs text-stone-500">{c.categoria}</p>
                    </div>
                    {c.verificada ? (
                      <span className="shrink-0 rounded-full bg-bosque/10 px-2.5 py-1 text-xs font-semibold text-bosque">
                        ✓ Verificada
                      </span>
                    ) : (
                      <span className="shrink-0 rounded-full bg-stone-100 px-2.5 py-1 text-xs font-medium text-stone-500">
                        Sin verificar
                      </span>
                    )}
                  </li>
                ))}
              </ul>
            </section>
          </>
        )}

        <button type="button" onClick={onCerrarSesion} className={claseBoton}>
          Cerrar sesión
        </button>
      </main>
    </div>
  )
}
