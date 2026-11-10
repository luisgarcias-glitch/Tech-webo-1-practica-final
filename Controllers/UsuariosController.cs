using CatalogoGalactico.Dtos;
using CatalogoGalactico.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoGalactico.Controllers;

[ApiController]
[Route("usuarios")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public class UsuariosController(IUsuarioService usuarios) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> Listar() =>
        Ok(await usuarios.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Obtener(int id) =>
        Ok(await usuarios.ObtenerAsync(id));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<UsuarioDto>> Crear(UsuarioCreateDto dto)
    {
        var creado = await usuarios.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Actualizar(int id, UsuarioUpdateDto dto) =>
        Ok(await usuarios.ActualizarAsync(id, dto));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await usuarios.EliminarAsync(id);
        return NoContent();
    }
}
