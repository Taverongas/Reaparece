using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Reaparece.Api.Models;

namespace Reaparece.Api.Services;

/// <summary>
/// Almacén de comunidades en memoria. Se registra como singleton y se reemplazará por PostgreSQL.
/// </summary>
public partial class ComunidadStore
{
    public static readonly IReadOnlyList<string> Categorias = ["Universidad", "Transporte", "Barrio", "Empresa"];
    public const int LongitudCodigo = 6;
    // Sin 0/O ni 1/I para que el código no se confunda al dictarlo.
    private const string CaracteresCodigo = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly List<Comunidad> _comunidades =
    [
        new() { Id = 1, Nombre = "Universidad EIA", Categoria = "Universidad", Verificada = true,
                Descripcion = "Objetos perdidos en las sedes de Las Palmas y Zúñiga." },
        new() { Id = 2, Nombre = "Metro de Medellín", Categoria = "Transporte", Verificada = true,
                Descripcion = "Objetos olvidados en estaciones, trenes y metrocables." },
        new() { Id = 3, Nombre = "Transmilenio Bogotá", Categoria = "Transporte", Verificada = true,
                Descripcion = "Objetos perdidos en buses y estaciones de Transmilenio." },
        new() { Id = 4, Nombre = "Barrio El Poblado", Categoria = "Barrio", Verificada = false,
                Descripcion = "Vecinos de El Poblado que se ayudan a recuperar lo perdido." },
    ];
    private readonly Lock _lock = new();
    private int _siguienteId = 5;

    [GeneratedRegex(@"\s+")]
    private static partial Regex Espacios();

    /// <summary>Quita espacios repetidos: "  Club   de Lectura " → "Club de Lectura".</summary>
    public static string LimpiarNombre(string nombre) => Espacios().Replace(nombre.Trim(), " ");

    private static string Clave(string nombre) => LimpiarNombre(nombre).ToLowerInvariant();

    public IReadOnlyList<Comunidad> Todas()
    {
        lock (_lock)
        {
            return [.. _comunidades];
        }
    }

    public Comunidad? BuscarPorId(int id)
    {
        lock (_lock)
        {
            return _comunidades.FirstOrDefault(c => c.Id == id);
        }
    }

    public Comunidad? BuscarPorNombre(string nombre)
    {
        var clave = Clave(nombre);
        lock (_lock)
        {
            return _comunidades.FirstOrDefault(c => Clave(c.Nombre) == clave);
        }
    }

    /// <summary>Crea la comunidad, sin verificar, o devuelve null si ya existe una con ese nombre.</summary>
    public Comunidad? TryCrear(string nombre, string categoria, string descripcion, bool esPrivada, int creadorId)
    {
        lock (_lock)
        {
            if (_comunidades.Any(c => Clave(c.Nombre) == Clave(nombre)))
                return null;

            var comunidad = new Comunidad
            {
                Id = _siguienteId++,
                Nombre = LimpiarNombre(nombre),
                Categoria = categoria,
                Descripcion = descripcion.Trim(),
                Verificada = false,
                EsPrivada = esPrivada,
                CodigoInvitacion = esPrivada ? RandomNumberGenerator.GetString(CaracteresCodigo, LongitudCodigo) : null,
                CreadorId = creadorId,
            };
            _comunidades.Add(comunidad);
            return comunidad;
        }
    }
}
