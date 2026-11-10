using Microsoft.AspNetCore.Mvc;
using Reaparece.Api.Models;
using Reaparece.Api.Services;

namespace Reaparece.Api.Controllers;

[ApiController]
[Route("api/comunidades")]
public class ComunidadesController(UsuarioStore usuarios, ComunidadStore comunidades) : ControllerBase
{
    private const int LongitudMinimaNombre = 3;
    private const int LongitudMaximaNombre = 60;
    private const int LongitudMaximaDescripcion = 200;

    // REAP-4: todas las comunidades, ordenadas por nombre, indicando a cuáles ya pertenece el usuario.
    [HttpGet]
    public ActionResult<IReadOnlyList<ComunidadListadoResponse>> Listar()
    {
        if (UsuarioActual() is not { } usuario)
            return SesionInvalida();

        return Ok(comunidades.Todas()
            .OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .Select(c => new ComunidadListadoResponse(
                c.Id, c.Nombre, c.Categoria, c.Descripcion, c.Verificada, c.EsPrivada,
                usuarios.ContarMiembros(c.Id), usuarios.EsMiembro(usuario, c.Id)))
            .ToList());
    }

    // REAP-5
    [HttpPost]
    public ActionResult<ComunidadResponse> Crear(CrearComunidadRequest req)
    {
        if (UsuarioActual() is not { } usuario)
            return SesionInvalida();

        if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Categoria) ||
            string.IsNullOrWhiteSpace(req.Descripcion))
            return Error(StatusCodes.Status400BadRequest, "Completa todos los campos");

        var nombre = ComunidadStore.LimpiarNombre(req.Nombre);
        if (nombre.Length is < LongitudMinimaNombre or > LongitudMaximaNombre)
            return Error(StatusCodes.Status400BadRequest, "El nombre debe tener entre 3 y 60 caracteres");

        if (!ComunidadStore.Categorias.Contains(req.Categoria))
            return Error(StatusCodes.Status400BadRequest, "Selecciona una categoría válida");

        if (req.Descripcion.Trim().Length > LongitudMaximaDescripcion)
            return Error(StatusCodes.Status400BadRequest, "La descripción debe tener máximo 200 caracteres");

        var comunidad = comunidades.TryCrear(nombre, req.Categoria, req.Descripcion, req.EsPrivada, usuario.Id);
        if (comunidad is null)
            return Error(StatusCodes.Status409Conflict, "Ya existe una comunidad con ese nombre");

        // Quien crea la comunidad queda como su primer miembro.
        usuarios.TryUnirse(usuario, comunidad.Id);

        return Created($"/api/comunidades/{comunidad.Id}", ParaPerfil(comunidad, usuario));
    }

    // REAP-4
    [HttpPost("{id:int}/unirse")]
    public ActionResult<ComunidadResponse> Unirse(int id, UnirseRequest req)
    {
        if (UsuarioActual() is not { } usuario)
            return SesionInvalida();

        if (comunidades.BuscarPorId(id) is not { } comunidad)
            return Error(StatusCodes.Status404NotFound, "La comunidad no existe");

        if (usuarios.EsMiembro(usuario, comunidad.Id))
            return Error(StatusCodes.Status409Conflict, "Ya perteneces a esta comunidad");

        if (comunidad.EsPrivada)
        {
            var codigo = req.CodigoInvitacion?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(codigo))
                return Error(StatusCodes.Status400BadRequest, "Ingresa el código de invitación");

            if (codigo != comunidad.CodigoInvitacion)
                return Error(StatusCodes.Status403Forbidden, "El código de invitación no es correcto");
        }

        if (!usuarios.TryUnirse(usuario, comunidad.Id))
            return Error(StatusCodes.Status409Conflict, "Ya perteneces a esta comunidad");

        return Ok(ParaPerfil(comunidad, usuario));
    }

    internal static ComunidadResponse ParaPerfil(Comunidad c, Usuario usuario) => new(
        c.Id,
        c.Nombre,
        c.Categoria,
        c.Verificada,
        c.Id == usuario.ComunidadPrincipalId,
        c.EsPrivada,
        c.CreadorId == usuario.Id ? c.CodigoInvitacion : null);

    private Usuario? UsuarioActual() =>
        TokenHelper.LeerBearer(Request) is { } token ? usuarios.ValidarToken(token) : null;

    private ObjectResult SesionInvalida() =>
        Error(StatusCodes.Status401Unauthorized, TokenHelper.MensajeSesionInvalida);

    private ObjectResult Error(int status, string mensaje) => StatusCode(status, new ErrorResponse(mensaje));
}
