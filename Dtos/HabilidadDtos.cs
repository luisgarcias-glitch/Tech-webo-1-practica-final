using System.ComponentModel.DataAnnotations;
using CatalogoGalactico.Models;

namespace CatalogoGalactico.Dtos;

public record HabilidadCreateDto
{
    [Required, StringLength(100)] public string Nombre { get; init; } = "";
    [Required] public TipoHabilidad? Tipo { get; init; }
    [StringLength(300)] public string? Descripcion { get; init; }
    [Range(1, 20)] public int Bonus { get; init; }
}

public record HabilidadDto(int Id, string Nombre, TipoHabilidad Tipo, string Descripcion, int Bonus);

public class HabilidadFiltro
{
    public TipoHabilidad? Tipo { get; set; }
    public string? Nombre { get; set; }
    public int? BonusMinimo { get; set; }
}

public record AsignarHabilidadDto
{
    [Range(1, int.MaxValue)] public int HabilidadId { get; init; }
    [Range(1, 5)] public int Nivel { get; init; }
}

public record ActualizarNivelDto
{
    [Range(1, 5)] public int Nivel { get; init; }
}

public record HabilidadAsignadaDto(
    int HabilidadId, string Nombre, TipoHabilidad Tipo, string Descripcion, int Bonus, int Nivel);

public record PersonajeConHabilidadDto(int PersonajeId, string Nombre, Faccion Faccion, int Nivel);
