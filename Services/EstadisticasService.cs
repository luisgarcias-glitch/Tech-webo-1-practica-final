using CatalogoGalactico.Data;
using CatalogoGalactico.Dtos;
using CatalogoGalactico.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalactico.Services;

public interface IEstadisticasService
{
    Task<List<RankingItemDto>> RankingAsync(string? por, string? orden);
    Task<MvpDto> MvpAsync(int eventoId);
}

public class EstadisticasService(AppDbContext db) : IEstadisticasService
{
    public async Task<List<RankingItemDto>> RankingAsync(string? por, string? orden)
    {
        var criterio = string.IsNullOrWhiteSpace(por) ? "poder" : por.Trim().ToLowerInvariant();
        var direccion = string.IsNullOrWhiteSpace(orden) ? "desc" : orden.Trim().ToLowerInvariant();

        if (criterio is not ("poder" or "peligrosidad"))
            throw new ReglaNegocioException($"Criterio 'por' inválido: '{por}'. Valores permitidos: poder, peligrosidad.");
        if (direccion is not ("asc" or "desc"))
            throw new ReglaNegocioException($"Valor 'orden' inválido: '{orden}'. Valores permitidos: asc, desc.");

        var filas = await db.Cartas.AsNoTracking()
            .Select(c => new
            {
                c.PersonajeId,
                c.Personaje.Nombre,
                c.Personaje.Faccion,
                c.Poder,
                c.NivelPeligrosidad
            })
            .ToListAsync();

        var ordenadas = direccion == "desc"
            ? filas.OrderByDescending(x => criterio == "poder" ? x.Poder : x.NivelPeligrosidad).ThenBy(x => x.Nombre)
            : filas.OrderBy(x => criterio == "poder" ? x.Poder : x.NivelPeligrosidad).ThenBy(x => x.Nombre);

        return ordenadas
            .Select((x, i) => new RankingItemDto(i + 1, x.PersonajeId, x.Nombre, x.Faccion, x.Poder, x.NivelPeligrosidad))
            .ToList();
    }

    public async Task<MvpDto> MvpAsync(int eventoId)
    {
        var ev = await db.Eventos.AsNoTracking().Include(e => e.Participantes)
                     .FirstOrDefaultAsync(e => e.Id == eventoId)
                 ?? throw new NotFoundException($"No existe un evento con id {eventoId}.");

        if (ev.Participantes.Count == 0)
            throw new ReglaNegocioException($"El evento '{ev.Nombre}' no tiene participantes.");

        var ids = ev.Participantes.Select(p => p.PersonajeId).ToList();
        var personajes = await db.Personajes.AsNoTracking().Include(p => p.Card)
            .Where(p => ids.Contains(p.Id)).ToListAsync();

        var conCarta = personajes.Where(p => p.Card is not null).ToList();
        var sinCarta = personajes.Where(p => p.Card is null).Select(p => p.Nombre).ToList();

        if (conCarta.Count == 0)
            throw new ReglaNegocioException("Ningún participante del evento tiene carta; no se puede calcular el MVP.");

        var mvp = conCarta
            .OrderByDescending(p => p.Card!.Poder)
            .ThenByDescending(p => p.Card!.NivelPeligrosidad)
            .ThenBy(p => p.Id)
            .First();

        return new MvpDto(ev.Id, ev.Nombre, mvp.ToDto(), mvp.Card!.ToDto(), sinCarta,
            "Mayor poder de carta entre los participantes; desempate por nivel de peligrosidad y luego por id.");
    }
}
