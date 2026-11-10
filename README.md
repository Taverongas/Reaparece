# Reaparece

Reaparece es una aplicación web de **objetos perdidos organizada por comunidades**: una universidad, un sistema de transporte o un barrio. Cada persona pertenece a una o varias comunidades y, dentro de ellas, publica lo que encontró o lo que perdió para que vuelva a su dueño. La reputación de cada usuario refleja cuánto ha aportado.

Este repositorio contiene el **Sprint 1**: registro, inicio de sesión y perfil de usuario.

## Stack

| Capa | Tecnología |
| --- | --- |
| Backend | ASP.NET Core Web API, C#, .NET 10 |
| Frontend | React 19, Vite, TypeScript, Tailwind CSS 4 |
| Almacenamiento | En memoria (`UsuarioStore`, singleton). Se reemplazará por PostgreSQL |
| Seguridad | Contraseñas con hash bcrypt (`BCrypt.Net-Next`) y token de sesión aleatorio que expira a las 2 horas |

> Como los datos viven en memoria, **todo lo registrado se pierde al reiniciar el backend**. El usuario de prueba se vuelve a crear en cada arranque.

## Estructura

```
backend/Reaparece.Api/
  Controllers/  AuthController (registro, login, logout) · PerfilController
  Models/       Usuario · Comunidad · DTOs
  Services/     UsuarioStore (usuarios, intentos fallidos, sesiones) · TokenHelper
frontend/src/
  pages/        Registro · Login · Perfil
  components/   Encabezado · piezas de formulario
  services/     api.ts (llamadas al backend y tipos)
```

## Cómo ejecutarlo

Requisitos: [.NET SDK 10](https://dotnet.microsoft.com/download) y [Node.js 20 o superior](https://nodejs.org/).

**1. Backend** (terminal 1, déjala abierta):

```bash
cd backend/Reaparece.Api
dotnet run
```

Espera a ver `Now listening on: http://localhost:5062`.

**2. Frontend** (terminal 2):

```bash
cd frontend
npm install      # solo la primera vez
npm run dev
```

**3.** Abre **http://localhost:5173** en el navegador.

> El backend solo acepta peticiones desde `http://localhost:5173` (CORS). Si Vite arranca en otro puerto (por ejemplo 5174), es porque ya hay otro frontend abierto: ciérralo y vuelve a correr `npm run dev`.

## Credenciales de prueba

| Correo | Contraseña |
| --- | --- |
| `jeronimo@eia.edu.co` | `Reaparece123` |

Tiene reputación 4.6, 7 objetos publicados, 4 recuperados y 3 reclamados, y pertenece a dos comunidades: Universidad EIA (verificada, principal) y Barrio El Poblado (sin verificar).

## API

| Método | Ruta | Respuestas |
| --- | --- | --- |
| `POST` | `/api/auth/registro` | 200 sesión · 400 datos inválidos · 409 correo ya registrado |
| `POST` | `/api/auth/login` | 200 sesión · 400 campos vacíos · 401 credenciales incorrectas · 423 cuenta bloqueada |
| `POST` | `/api/auth/logout` | 204 (revoca el token) |
| `GET` | `/api/perfil` | 200 perfil · 401 sin token o token inválido |

El token se envía en el encabezado `Authorization: Bearer {token}`. El archivo `backend/Reaparece.Api/Reaparece.Api.http` trae ejemplos listos para probar desde VS Code o Rider.

## Historias del Sprint 1

| Historia | Puntos | Criterios de aceptación |
| --- | :---: | --- |
| **REAP-2** Registrarse a la aplicación | 5 | 1. Con correo válido y contraseña de mínimo 8 caracteres, la cuenta se crea y el usuario entra a la aplicación.<br>2. Si el correo ya está registrado (aunque cambien las mayúsculas) responde 409 con "Este correo ya tiene una cuenta" y no se duplica el usuario.<br>3. El formulario muestra el aviso de la Ley 1581 de 2012 y el botón queda deshabilitado hasta marcar el checkbox.<br>4. Campos: nombre completo, correo, contraseña, confirmar contraseña y comunidad principal (Universidad EIA, Metro de Medellín, Transmilenio Bogotá, Barrio El Poblado). |
| **REAP-3** Iniciar sesión | 3 | 1. Con credenciales correctas el usuario accede y recibe un token de sesión.<br>2. Con credenciales incorrectas muestra "Correo o contraseña incorrectos" sin indicar cuál falló (401).<br>3. Tras tres intentos fallidos con el mismo correo, la cuenta se bloquea 15 minutos y se informa al usuario (423).<br>4. Campos vacíos responden 400 con "Completa todos los campos". |
| **REAP-6** Ver perfil | 3 | 1. Muestra la reputación del usuario y los contadores de objetos publicados, recuperados y reclamados.<br>2. Lista las comunidades a las que pertenece, con categoría y sello de verificada.<br>3. Botón de cerrar sesión que devuelve al login.<br>4. `GET /api/perfil` exige token válido; sin él responde 401 sin entregar datos. |

**Total del sprint: 11 puntos.**
