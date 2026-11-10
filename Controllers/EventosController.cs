using CatalogoGalactico.Dtos;
using CatalogoGalactico.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoGalactico.Controllers;

[ApiController]
[Route("eventos")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public class EventosController(
    IEventoService eventos,
    ISimulacionService simulacion,
    IEstadisticasService estadisticas) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EventoDto>>> Listar([FromQuery] EventoFiltro filtro) =>
        Ok(await eventos.ListarAsync(filtro));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventoDto>> Obtener(int id) =>
        Ok(await eventos.ObtenerAsync(id));

    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<EventoDto>> Crear(EventoCreateDto dto)
    {
        var creado = await eventos.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventoDto>> Actualizar(int id, EventoCreateDto dto) =>
        Ok(await eventos.ActualizarAsync(id, dto));

    [HttpGet("{id:int}/mvp")]
    public async Task<ActionResult<MvpDto>> Mvp(int id) =>
        Ok(await estadisticas.MvpAsync(id));

    [HttpPost("{id:int}/simular")]
    public async Task<ActionResult<SimulacionDto>> Simular(int id, int? semilla) =>
        Ok(await simulacion.SimularAsync(id, semilla));
}
