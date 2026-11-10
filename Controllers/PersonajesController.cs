using CatalogoGalactico.Dtos;
using CatalogoGalactico.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoGalactico.Controllers;

[ApiController]
[Route("personajes")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public class PersonajesController(
    IPersonajeService personajes,
    IEventoService eventos,
    IEstadisticasService estadisticas) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PersonajeDto>>> Listar([FromQuery] PersonajeFiltro filtro) =>
        Ok(await personajes.ListarAsync(filtro));

    [HttpGet("ranking")]
    public async Task<ActionResult<List<RankingItemDto>>> Ranking(string? por, string? orden) =>
        Ok(await estadisticas.RankingAsync(por, orden));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PersonajeDto>> Obtener(int id) =>
        Ok(await personajes.ObtenerAsync(id));

    [HttpGet("{id:int}/con-card")]
    public async Task<ActionResult<PersonajeConCardDto>> ObtenerConCard(int id) =>
        Ok(await personajes.ObtenerConCardAsync(id));

    [HttpGet("{id:int}/eventos")]
    public async Task<ActionResult<List<EventoDto>>> ListarEventos(int id) =>
        Ok(await eventos.ListarPorPersonajeAsync(id));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<PersonajeDto>> Crear(PersonajeCreateDto dto)
    {
        var creado = await personajes.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PersonajeDto>> Actualizar(int id, PersonajeCreateDto dto) =>
        Ok(await personajes.ActualizarAsync(id, dto));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await personajes.EliminarAsync(id);
        return NoContent();
    }
}
