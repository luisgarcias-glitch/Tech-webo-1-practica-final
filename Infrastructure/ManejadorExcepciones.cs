using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoGalactico.Infrastructure;

public class ManejadorExcepciones : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken ct)
    {
        var (status, titulo) = excepcion switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto con el estado actual"),
            ReglaNegocioException => (StatusCodes.Status400BadRequest, "Datos o regla de negocio inválidos"),
            _ => (0, "")
        };

        if (status == 0) return false; 

        contexto.Response.StatusCode = status;
        await contexto.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = status, Title = titulo, Detail = excepcion.Message }, ct);
        return true;
    }
}
