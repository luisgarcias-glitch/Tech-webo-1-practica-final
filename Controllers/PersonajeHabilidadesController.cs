using CatalogoGalactico.Dtos;
using CatalogoGalactico.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoGalactico.Controllers;

[ApiController]
[Route("personajes/{personajeId:int}/habilidades")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public class PersonajeHabilidadesController(IHabilidadService habilidades) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<HabilidadAsignadaDto>>> Listar(int personajeId) =>
        Ok(await habilidades.ListarPorPersonajeAsync(personajeId));


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<HabilidadAsignadaDto>> Asignar(int personajeId, AsignarHabilidadDto dto)
    {
        var asignada = await habilidades.AsignarAsync(personajeId, dto);
        return Created($"/personajes/{personajeId}/habilidades", asignada);
    }

    [HttpPut("{habilidadId:int}")]
    public async Task<ActionResult<HabilidadAsignadaDto>> ActualizarNivel(
        int personajeId, int habilidadId, ActualizarNivelDto dto) =>
        Ok(await habilidades.ActualizarNivelAsync(personajeId, habilidadId, dto.Nivel));

    [HttpDelete("{habilidadId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Quitar(int personajeId, int habilidadId)
    {
        await habilidades.QuitarAsync(personajeId, habilidadId);
        return NoContent();
    }
}
