using System.Security.Cryptography;
using Reaparece.Api.Models;

namespace Reaparece.Api.Services;

public enum ResultadoLogin { Exitoso, CredencialesInvalidas, Bloqueado }

/// <summary>
/// Almacén de usuarios y sesiones en memoria. Se registra como singleton y se reemplazará por PostgreSQL.
/// </summary>
public class UsuarioStore
{
    public const int MaxIntentosFallidos = 3;
    public static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan DuracionSesion = TimeSpan.FromHours(2);

    private readonly List<Usuario> _usuarios = [];
    // Los intentos se cuentan por correo aunque el correo no exista, para no revelar qué cuentas están registradas.
    private readonly Dictionary<string, (int Fallidos, DateTime? BloqueadoHasta)> _intentos = [];
    private readonly Dictionary<string, (int UsuarioId, DateTime ExpiraEn)> _sesiones = [];
    private readonly Lock _lock = new();
    private readonly TimeProvider _reloj;
    private int _siguienteId = 1;

    public UsuarioStore(TimeProvider reloj)
    {
        _reloj = reloj;

        var prueba = Crear("Jerónimo Tavera", "jeronimo@eia.edu.co", "Reaparece123", comunidadPrincipalId: 1);
        prueba.ComunidadIds.Add(4); // Barrio El Poblado: no verificada, para mostrar ambos sellos en el perfil.
        prueba.Reputacion = 4.6;
        prueba.ObjetosPublicados = 7;
        prueba.ObjetosRecuperados = 4;
        prueba.ObjetosReclamados = 3;
    }

    public static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();

    private DateTime Ahora => _reloj.GetUtcNow().UtcDateTime;

    public Usuario? BuscarPorCorreo(string correo)
    {
        var normalizado = NormalizarCorreo(correo);
        lock (_lock)
        {
            return _usuarios.FirstOrDefault(u => u.Correo == normalizado);
        }
    }

    /// <summary>Crea el usuario o devuelve null si el correo ya tiene una cuenta.</summary>
    public Usuario? TryCrear(string nombreCompleto, string correo, string contrasena, int comunidadPrincipalId)
    {
        lock (_lock)
        {
            if (_usuarios.Any(u => u.Correo == NormalizarCorreo(correo)))
                return null;

            return Crear(nombreCompleto, correo, contrasena, comunidadPrincipalId);
        }
    }

    /// <summary>
    /// Verifica las credenciales. Al tercer fallo seguido con el mismo correo bloquea ese correo
    /// por 15 minutos y devuelve <see cref="ResultadoLogin.Bloqueado"/> desde ese mismo intento.
    /// </summary>
    public (ResultadoLogin Resultado, Usuario? Usuario, DateTime? BloqueadoHasta) ValidarCredenciales(string correo, string contrasena)
    {
        var normalizado = NormalizarCorreo(correo);
        var ahora = Ahora;

        lock (_lock)
        {
            _intentos.TryGetValue(normalizado, out var intento);

            if (intento.BloqueadoHasta is { } hasta)
            {
                if (hasta > ahora)
                    return (ResultadoLogin.Bloqueado, null, hasta);

                // El bloqueo expiró: se empieza de cero.
                intento = (0, null);
            }

            var usuario = _usuarios.FirstOrDefault(u => u.Correo == normalizado);
            if (usuario is not null && BCrypt.Net.BCrypt.Verify(contrasena, usuario.ContrasenaHash))
            {
                _intentos.Remove(normalizado);
                return (ResultadoLogin.Exitoso, usuario, null);
            }

            var fallidos = intento.Fallidos + 1;
            if (fallidos >= MaxIntentosFallidos)
            {
                var bloqueadoHasta = ahora + DuracionBloqueo;
                _intentos[normalizado] = (fallidos, bloqueadoHasta);
                return (ResultadoLogin.Bloqueado, null, bloqueadoHasta);
            }

            _intentos[normalizado] = (fallidos, null);
            return (ResultadoLogin.CredencialesInvalidas, null, null);
        }
    }

    /// <summary>Agrega la comunidad al usuario. Devuelve false si ya pertenecía a ella.</summary>
    public bool TryUnirse(Usuario usuario, int comunidadId)
    {
        lock (_lock)
        {
            if (usuario.ComunidadIds.Contains(comunidadId))
                return false;

            usuario.ComunidadIds.Add(comunidadId);
            return true;
        }
    }

    public bool EsMiembro(Usuario usuario, int comunidadId)
    {
        lock (_lock)
        {
            return usuario.ComunidadIds.Contains(comunidadId);
        }
    }

    public int ContarMiembros(int comunidadId)
    {
        lock (_lock)
        {
            return _usuarios.Count(u => u.ComunidadIds.Contains(comunidadId));
        }
    }

    public (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiraEn = Ahora + DuracionSesion;
        lock (_lock)
        {
            _sesiones[token] = (usuario.Id, expiraEn);
        }
        return (token, expiraEn);
    }

    /// <summary>Devuelve el usuario dueño del token, o null si no existe o ya expiró.</summary>
    public Usuario? ValidarToken(string token)
    {
        lock (_lock)
        {
            if (!_sesiones.TryGetValue(token, out var sesion))
                return null;

            if (sesion.ExpiraEn <= Ahora)
            {
                _sesiones.Remove(token);
                return null;
            }

            return _usuarios.FirstOrDefault(u => u.Id == sesion.UsuarioId);
        }
    }

    public void RevocarToken(string token)
    {
        lock (_lock)
        {
            _sesiones.Remove(token);
        }
    }

    private Usuario Crear(string nombreCompleto, string correo, string contrasena, int comunidadPrincipalId)
    {
        var usuario = new Usuario
        {
            Id = _siguienteId++,
            NombreCompleto = nombreCompleto.Trim(),
            Correo = NormalizarCorreo(correo),
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasena),
            ComunidadPrincipalId = comunidadPrincipalId,
            ComunidadIds = [comunidadPrincipalId],
        };
        _usuarios.Add(usuario);
        return usuario;
    }
}
