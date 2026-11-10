namespace Reaparece.Api.Models;

public record RegistroRequest(
    string? NombreCompleto,
    string? Correo,
    string? Contrasena,
    string? ConfirmarContrasena,
    string? ComunidadPrincipal,
    bool AceptaHabeasData);

public record LoginRequest(string? Correo, string? Contrasena);

public record UsuarioResponse(int Id, string NombreCompleto, string Correo)
{
    public static UsuarioResponse Desde(Usuario u) => new(u.Id, u.NombreCompleto, u.Correo);
}

/// <summary>Respuesta de registro y login: el token de sesión y el usuario que entró.</summary>
public record SesionResponse(string Token, DateTime ExpiraEn, UsuarioResponse Usuario);

/// <summary>Comunidad tal como aparece en "Mis comunidades". El código solo se entrega a quien creó la comunidad.</summary>
public record ComunidadResponse(
    int Id,
    string Nombre,
    string Categoria,
    bool Verificada,
    bool EsPrincipal,
    bool EsPrivada,
    string? CodigoInvitacion);

// REAP-5
public record CrearComunidadRequest(string? Nombre, string? Categoria, string? Descripcion, bool EsPrivada);

// REAP-4
public record UnirseRequest(string? CodigoInvitacion);

/// <summary>Comunidad en la pantalla "Unirme a una comunidad".</summary>
public record ComunidadListadoResponse(
    int Id,
    string Nombre,
    string Categoria,
    string Descripcion,
    bool Verificada,
    bool EsPrivada,
    int Miembros,
    bool EsMiembro);

public record PerfilResponse(
    int Id,
    string NombreCompleto,
    string Correo,
    double Reputacion,
    int ObjetosPublicados,
    int ObjetosRecuperados,
    int ObjetosReclamados,
    IReadOnlyList<ComunidadResponse> Comunidades);

public record ErrorResponse(string Mensaje);
