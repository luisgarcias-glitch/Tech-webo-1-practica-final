using System.ComponentModel.DataAnnotations;
using CatalogoGalactico.Models;

namespace CatalogoGalactico.Dtos;

public record PersonajeCreateDto
{
    [Required, StringLength(100)] public string Nombre { get; init; } = "";
    [Required, StringLength(60)] public string Especie { get; init; } = "";
    [Required] public Faccion? Faccion { get; init; }
    [Required, StringLength(100)] public string Afiliacion { get; init; } = "";
    public EstadoPersonaje? Estado { get; init; }
    public bool FuerzaSensitivo { get; init; }
    [Url, StringLength(500)] public string? Image { get; init; }
}

public record PersonajeDto(
    int Id, string Nombre, string Especie, Faccion Faccion, string Afiliacion,
    EstadoPersonaje Estado, bool FuerzaSensitivo, string? Image);

public class PersonajeFiltro
{
    public Faccion? Faccion { get; set; }
    public EstadoPersonaje? Estado { get; set; }
    public bool? FuerzaSensitivo { get; set; }
    public string? Especie { get; set; }
    public string? Afiliacion { get; set; }
    public string? Nombre { get; set; }
}

public record PersonajeConCardDto(
    int Id, string Nombre, string Especie, Faccion Faccion, string Afiliacion,
    EstadoPersonaje Estado, bool FuerzaSensitivo, string? Image,
    int Poder, string HabilidadEspecial, string Arma, int NivelPeligrosidad,
    CardDto Card, List<HabilidadAsignadaDto> Habilidades);
