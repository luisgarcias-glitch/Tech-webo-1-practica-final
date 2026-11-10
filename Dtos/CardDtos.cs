using System.ComponentModel.DataAnnotations;

namespace CatalogoGalactico.Dtos;

public record CardCreateDto
{
    [Range(1, int.MaxValue)] public int PersonajeId { get; init; }
    [Range(1, 100)] public int Poder { get; init; }
    [Required, StringLength(100)] public string HabilidadEspecial { get; init; } = "";
    [Required, StringLength(100)] public string Arma { get; init; } = "";
    [Range(1, 10)] public int NivelPeligrosidad { get; init; }
    [Url, StringLength(500)] public string? ImagenUrl { get; init; }
}

public record CardDto(
    int Id, int PersonajeId, int Poder, string HabilidadEspecial, string Arma,
    int NivelPeligrosidad, string? ImagenUrl);

public class CardFiltro
{
    public int? PersonajeId { get; set; }
    public int? PoderMinimo { get; set; }
    public int? PoderMaximo { get; set; }
    public int? PeligrosidadMinima { get; set; }
}
