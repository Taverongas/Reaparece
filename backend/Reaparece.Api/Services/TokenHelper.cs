namespace Reaparece.Api.Services;

public static class TokenHelper
{
    public const string MensajeSesionInvalida = "Tu sesión no es válida o expiró. Inicia sesión de nuevo.";

    /// <summary>Extrae el token del encabezado <c>Authorization: Bearer {token}</c>, o null si no viene.</summary>
    public static string? LeerBearer(HttpRequest request)
    {
        const string prefijo = "Bearer ";
        var valor = request.Headers.Authorization.ToString();
        if (!valor.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
            return null;

        var token = valor[prefijo.Length..].Trim();
        return token.Length > 0 ? token : null;
    }
}
