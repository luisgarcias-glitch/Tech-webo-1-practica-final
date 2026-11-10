using System.ComponentModel.DataAnnotations;
using CatalogoGalactico.Models;

namespace CatalogoGalactico.Dtos;

public record EventoCreateDto
{
    [Required, StringLength(150)] public string Nombre { get; init; } = "";

    [Required] public string Fecha { get; init; } = "";

    [Required, StringLength(100)] public string Ubicacion { get; init; } = "";
    [StringLength(1000)] public string? Descripcion { get; init; }

    public List<int>? Participantes { get; init; }

    public List<int>? PersonajesMuertos { get; init; }
}

public record EventoDto(
    int Id, string Nombre, string Fecha, int AnioGuardado, string Ubicacion, string Descripcion,
    List<int> Participantes, List<int> PersonajesMuertos, string? Resultado, string? Ganador);

public class EventoFiltro
{
    public string? Nombre { get; set; }
    public string? Ubicacion { get; set; }
    public int? PersonajeId { get; set; }
    public string? Desde { get; set; }
    public string? Hasta { get; set; }
}
