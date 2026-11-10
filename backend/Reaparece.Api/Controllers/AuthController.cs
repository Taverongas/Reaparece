using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Reaparece.Api.Models;
using Reaparece.Api.Services;

namespace Reaparece.Api.Controllers;

[ApiController]
[Route("api/auth")]
public partial class AuthController(UsuarioStore store, ComunidadStore comunidades, TimeProvider reloj) : ControllerBase
{
    private const int LongitudMinimaContrasena = 8;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex CorreoValido();

    // REAP-2
    [HttpPost("registro")]
    public ActionResult<SesionResponse> Registro(RegistroRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NombreCompleto) || string.IsNullOrWhiteSpace(req.Correo) ||
            string.IsNullOrEmpty(req.Contrasena) || string.IsNullOrEmpty(req.ConfirmarContrasena) ||
            string.IsNullOrWhiteSpace(req.ComunidadPrincipal))
            return Error(StatusCodes.Status400BadRequest, "Completa todos los campos");

        if (!CorreoValido().IsMatch(req.Correo.Trim()))
            return Error(StatusCodes.Status400BadRequest, "Ingresa un correo válido");

        if (req.Contrasena.Length < LongitudMinimaContrasena)
            return Error(StatusCodes.Status400BadRequest, "La contraseña debe tener mínimo 8 caracteres");

        if (req.Contrasena != req.ConfirmarContrasena)
            return Error(StatusCodes.Status400BadRequest, "Las contraseñas no coinciden");

        // Una comunidad privada solo se puede elegir entrando con su código (REAP-4), no al registrarse.
        var comunidad = comunidades.BuscarPorNombre(req.ComunidadPrincipal);
        if (comunidad is null || comunidad.EsPrivada)
            return Error(StatusCodes.Status400BadRequest, "Selecciona una comunidad válida");

        if (!req.AceptaHabeasData)
            return Error(StatusCodes.Status400BadRequest, "Debes aceptar el tratamiento de datos personales para continuar");

        var usuario = store.TryCrear(req.NombreCompleto, req.Correo, req.Contrasena, comunidad.Id);
        if (usuario is null)
            return Error(StatusCodes.Status409Conflict, "Este correo ya tiene una cuenta");

        return Ok(CrearSesion(usuario));
    }

    // REAP-3
    [HttpPost("login")]
    public ActionResult<SesionResponse> Login(LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Correo) || string.IsNullOrEmpty(req.Contrasena))
            return Error(StatusCodes.Status400BadRequest, "Completa todos los campos");

        var (resultado, usuario, bloqueadoHasta) = store.ValidarCredenciales(req.Correo, req.Contrasena);

        return resultado switch
        {
            ResultadoLogin.Exitoso => Ok(CrearSesion(usuario!)),
            ResultadoLogin.Bloqueado => Error(StatusCodes.Status423Locked,
                $"Tu cuenta está bloqueada temporalmente por demasiados intentos fallidos. Intenta de nuevo en {MinutosRestantes(bloqueadoHasta!.Value)} minutos."),
            _ => Error(StatusCodes.Status401Unauthorized, "Correo o contraseña incorrectos"),
        };
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        if (TokenHelper.LeerBearer(Request) is { } token)
            store.RevocarToken(token);

        return NoContent();
    }

    private SesionResponse CrearSesion(Usuario usuario)
    {
        var (token, expiraEn) = store.GenerarToken(usuario);
        return new SesionResponse(token, expiraEn, UsuarioResponse.Desde(usuario));
    }

    private int MinutosRestantes(DateTime hasta) =>
        Math.Max(1, (int)Math.Ceiling((hasta - reloj.GetUtcNow().UtcDateTime).TotalMinutes));

    private ObjectResult Error(int status, string mensaje) => StatusCode(status, new ErrorResponse(mensaje));
}
