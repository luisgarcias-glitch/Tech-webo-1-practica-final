using CatalogoGalactico.Dtos;
using CatalogoGalactico.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoGalactico.Controllers;

[ApiController]
[Route("habilidades")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public class HabilidadesController(IHabilidadService habilidades) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<HabilidadDto>>> Listar([FromQuery] HabilidadFiltro filtro) =>
        Ok(await habilidades.ListarAsync(filtro));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HabilidadDto>> Obtener(int id) =>
        Ok(await habilidades.ObtenerAsync(id));

    [HttpGet("{id:int}/personajes")]
    public async Task<ActionResult<List<PersonajeConHabilidadDto>>> ListarPersonajes(int id) =>
        Ok(await habilidades.ListarPersonajesAsync(id));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<HabilidadDto>> Crear(HabilidadCreateDto dto)
    {
        var creada = await habilidades.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<HabilidadDto>> Actualizar(int id, HabilidadCreateDto dto) =>
        Ok(await habilidades.ActualizarAsync(id, dto));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await habilidades.EliminarAsync(id);
        return NoContent();
    }
}
