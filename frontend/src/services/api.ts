const API_URL = 'http://localhost:5062/api'

export const COMUNIDADES = [
  'Universidad EIA',
  'Metro de Medellín',
  'Transmilenio Bogotá',
  'Barrio El Poblado',
] as const

export interface Usuario {
  id: number
  nombreCompleto: string
  correo: string
}

export interface Sesion {
  token: string
  expiraEn: string
  usuario: Usuario
}

export interface RegistroDatos {
  nombreCompleto: string
  correo: string
  contrasena: string
  confirmarContrasena: string
  comunidadPrincipal: string
  aceptaHabeasData: boolean
}

export interface Comunidad {
  id: number
  nombre: string
  categoria: string
  verificada: boolean
  esPrincipal: boolean
}

export interface Perfil extends Usuario {
  reputacion: number
  objetosPublicados: number
  objetosRecuperados: number
  objetosReclamados: number
  comunidades: Comunidad[]
}

export class ApiError extends Error {
  status: number

  constructor(status: number, mensaje: string) {
    super(mensaje)
    this.status = status
  }
}

async function solicitar<T>(ruta: string, init?: RequestInit): Promise<T> {
  let respuesta: Response
  try {
    respuesta = await fetch(`${API_URL}${ruta}`, {
      ...init,
      headers: { 'Content-Type': 'application/json', ...init?.headers },
    })
  } catch {
    throw new ApiError(0, 'No pudimos conectarnos con el servidor. Intenta de nuevo.')
  }

  const cuerpo = await respuesta.json().catch(() => null)
  if (!respuesta.ok) {
    throw new ApiError(respuesta.status, cuerpo?.mensaje ?? 'Ocurrió un error inesperado.')
  }
  return cuerpo as T
}

const conToken = (token: string) => ({ Authorization: `Bearer ${token}` })

export function registrar(datos: RegistroDatos) {
  return solicitar<Sesion>('/auth/registro', { method: 'POST', body: JSON.stringify(datos) })
}

export function iniciarSesion(correo: string, contrasena: string) {
  return solicitar<Sesion>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ correo, contrasena }),
  })
}

export function cerrarSesion(token: string) {
  return solicitar<null>('/auth/logout', { method: 'POST', headers: conToken(token) })
}

export function obtenerPerfil(token: string) {
  return solicitar<Perfil>('/perfil', { headers: conToken(token) })
}
