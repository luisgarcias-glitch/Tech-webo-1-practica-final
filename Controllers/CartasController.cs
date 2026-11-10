using CatalogoGalactico.Dtos;
using CatalogoGalactico.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoGalactico.Controllers;

[ApiController]
[Route("cartas")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public class CartasController(ICartaService cartas) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CardDto>>> Listar([FromQuery] CardFiltro filtro) =>
        Ok(await cartas.ListarAsync(filtro));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CardDto>> Obtener(int id) =>
        Ok(await cartas.ObtenerAsync(id));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<CardDto>> Crear(CardCreateDto dto)
    {
        var creada = await cartas.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CardDto>> Actualizar(int id, CardCreateDto dto) =>
        Ok(await cartas.ActualizarAsync(id, dto));
}
