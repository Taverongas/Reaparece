namespace Reaparece.Api.Models;

public class Comunidad
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public required string Categoria { get; init; }
    public required string Descripcion { get; init; }
    public bool Verificada { get; init; }

    // REAP-5: las comunidades privadas solo aceptan miembros con el código de invitación.
    public bool EsPrivada { get; init; }
    public string? CodigoInvitacion { get; init; }

    // Null en las comunidades precargadas.
    public int? CreadorId { get; init; }
}
