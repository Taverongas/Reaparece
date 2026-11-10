using Microsoft.AspNetCore.Mvc;
using Reaparece.Api.Models;
using Reaparece.Api.Services;

namespace Reaparece.Api.Controllers;

// REAP-6
[ApiController]
[Route("api/perfil")]
public class PerfilController(UsuarioStore store, ComunidadStore comunidades) : ControllerBase
{
    [HttpGet]
    public ActionResult<PerfilResponse> Obtener()
    {
        var usuario = TokenHelper.LeerBearer(Request) is { } token ? store.ValidarToken(token) : null;
        if (usuario is null)
            return Unauthorized(new ErrorResponse(TokenHelper.MensajeSesionInvalida));

        var misComunidades = comunidades.Todas()
            .Where(c => store.EsMiembro(usuario, c.Id))
            .Select(c => ComunidadesController.ParaPerfil(c, usuario))
            .ToList();

        return Ok(new PerfilResponse(
            usuario.Id,
            usuario.NombreCompleto,
            usuario.Correo,
            usuario.Reputacion,
            usuario.ObjetosPublicados,
            usuario.ObjetosRecuperados,
            usuario.ObjetosReclamados,
            misComunidades));
    }
}
