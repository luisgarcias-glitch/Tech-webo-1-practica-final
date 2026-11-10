using CatalogoGalactico.Data;
using CatalogoGalactico.Dtos;
using CatalogoGalactico.Infrastructure;
using CatalogoGalactico.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalactico.Services;

public interface IEventoService
{
    Task<List<EventoDto>> ListarAsync(EventoFiltro filtro);
    Task<EventoDto> ObtenerAsync(int id);
    Task<List<EventoDto>> ListarPorPersonajeAsync(int personajeId);
    Task<EventoDto> CrearAsync(EventoCreateDto dto);
    Task<EventoDto> ActualizarAsync(int id, EventoCreateDto dto);
}

public class EventoService(AppDbContext db) : IEventoService
{
    public async Task<List<EventoDto>> ListarAsync(EventoFiltro f)
    {
        int? desde = string.IsNullOrWhiteSpace(f.Desde) ? null : FechaGalactica.Parse(f.Desde);
        int? hasta = string.IsNullOrWhiteSpace(f.Hasta) ? null : FechaGalactica.Parse(f.Hasta);
        if (desde is not null && hasta is not null && desde > hasta)
            throw new ReglaNegocioException("El filtro 'desde' no puede ser posterior a 'hasta'.");

        if (f.PersonajeId is not null && !await db.Personajes.AnyAsync(p => p.Id == f.PersonajeId))
            throw new NotFoundException($"No existe un personaje con id {f.PersonajeId}.");

        IQueryable<Evento> q = db.Eventos.AsNoTracking().Include(e => e.Participantes);

        if (f.PersonajeId is not null)
        {
            var pid = f.PersonajeId.Value;
            q = q.Where(e => e.Participantes.Any(p => p.PersonajeId == pid));
        }
        if (desde is not null) { var d = desde.Value; q = q.Where(e => e.Fecha >= d); }
        if (hasta is not null) { var h = hasta.Value; q = q.Where(e => e.Fecha <= h); }
        if (!string.IsNullOrWhiteSpace(f.Nombre))
        {
            var nombre = f.Nombre.Trim();
            q = q.Where(e => e.Nombre.Contains(nombre));
        }
        if (!string.IsNullOrWhiteSpace(f.Ubicacion))
        {
            var ubicacion = f.Ubicacion.Trim();
            q = q.Where(e => e.Ubicacion.Contains(ubicacion));
        }

        var lista = await q.OrderBy(e => e.Fecha).ThenBy(e => e.Id).ToListAsync();
        return lista.Select(e => e.ToDto()).ToList();
    }

    public async Task<EventoDto> ObtenerAsync(int id)
    {
        var e = await db.Eventos.AsNoTracking().Include(x => x.Participantes).FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException($"No existe un evento con id {id}.");
        return e.ToDto();
    }

    public async Task<List<EventoDto>> ListarPorPersonajeAsync(int personajeId)
    {
        if (!await db.Personajes.AnyAsync(p => p.Id == personajeId))
            throw new NotFoundException($"No existe un personaje con id {personajeId}.");

        var lista = await db.Eventos.AsNoTracking().Include(e => e.Participantes)
            .Where(e => e.Participantes.Any(p => p.PersonajeId == personajeId))
            .OrderBy(e => e.Fecha).ThenBy(e => e.Id)
            .ToListAsync();
        return lista.Select(e => e.ToDto()).ToList();
    }

    public async Task<EventoDto> CrearAsync(EventoCreateDto dto)
    {
        var (anio, participantes, muertos) = await ValidarAsync(0, dto);

        var ev = new Evento
        {
            Nombre = dto.Nombre.Trim(),
            Fecha = anio,
            Ubicacion = dto.Ubicacion.Trim(),
            Descripcion = dto.Descripcion?.Trim() ?? ""
        };
        foreach (var pid in participantes)
            ev.Participantes.Add(new EventoParticipante { PersonajeId = pid, Murio = muertos.Contains(pid) });

        db.Eventos.Add(ev);

        await MarcarMuertosAsync(muertos);
        await db.SaveChangesAsync();
        return ev.ToDto();
    }

    public async Task<EventoDto> ActualizarAsync(int id, EventoCreateDto dto)
    {
        var ev = await db.Eventos.Include(e => e.Participantes).FirstOrDefaultAsync(e => e.Id == id)
                 ?? throw new NotFoundException($"No existe un evento con id {id}.");

        var (anio, participantes, muertos) = await ValidarAsync(id, dto);

        var participantesPrevios = ev.Participantes.Select(p => p.PersonajeId).ToHashSet();
        var muertosPrevios = ev.Participantes.Where(p => p.Murio).Select(p => p.PersonajeId).ToList();

        ev.Nombre = dto.Nombre.Trim();
        ev.Fecha = anio;
        ev.Ubicacion = dto.Ubicacion.Trim();
        ev.Descripcion = dto.Descripcion?.Trim() ?? "";

        var existentes = ev.Participantes.ToList();
        foreach (var ep in existentes)
        {
            if (!participantes.Contains(ep.PersonajeId)) ev.Participantes.Remove(ep); // queda huérfano -> EF lo elimina
            else ep.Murio = muertos.Contains(ep.PersonajeId);
        }
        foreach (var pid in participantes.Where(x => !participantesPrevios.Contains(x)))
            ev.Participantes.Add(new EventoParticipante { PersonajeId = pid, Murio = muertos.Contains(pid) });

        if (!participantesPrevios.SetEquals(participantes))
        {
            ev.Resultado = null;
            ev.Ganador = null;
        }

        await MarcarMuertosAsync(muertos.Except(muertosPrevios).ToList());
        foreach (var pid in muertosPrevios.Except(muertos))
        {
            var otraMuerte = await db.EventoParticipantes
                .AnyAsync(ep => ep.PersonajeId == pid && ep.Murio && ep.EventoId != id);
            if (otraMuerte) continue;

            var personaje = await db.Personajes.FindAsync(pid);
            if (personaje is not null) personaje.Estado = EstadoPersonaje.Vivo;
        }

        await db.SaveChangesAsync();
        return ev.ToDto();
    }


    private sealed record Fila(int EventoId, string Evento, int Fecha, int PersonajeId, bool Murio);

    private async Task<(int anio, List<int> participantes, List<int> muertos)> ValidarAsync(int idActual, EventoCreateDto dto)
    {
        var anio = FechaGalactica.Parse(dto.Fecha);
        var participantes = (dto.Participantes ?? new List<int>()).ToList();
        var muertos = (dto.PersonajesMuertos ?? new List<int>()).ToList();

        if (participantes.Count != participantes.Distinct().Count())
            throw new ReglaNegocioException("La lista de participantes contiene IDs repetidos.");
        if (muertos.Count != muertos.Distinct().Count())
            throw new ReglaNegocioException("La lista de personajesMuertos contiene IDs repetidos.");

        var nombres = await db.Personajes
            .Where(p => participantes.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Nombre);

        var faltantes = participantes.Where(pid => !nombres.ContainsKey(pid)).ToList();
        if (faltantes.Count > 0)
            throw new ReglaNegocioException($"Los siguientes participantes no existen: {string.Join(", ", faltantes)}.");

        var sinParticipar = muertos.Where(pid => !participantes.Contains(pid)).ToList();
        if (sinParticipar.Count > 0)
            throw new ReglaNegocioException(
                $"Figuran como muertos pero no son participantes del evento: {string.Join(", ", sinParticipar)}.");

        var filas = await db.EventoParticipantes.AsNoTracking()
            .Where(ep => ep.EventoId != idActual && participantes.Contains(ep.PersonajeId))
            .Select(ep => new Fila(ep.EventoId, ep.Evento.Nombre, ep.Evento.Fecha, ep.PersonajeId, ep.Murio))
            .ToListAsync();
        filas.AddRange(participantes.Select(pid =>
            new Fila(idActual, dto.Nombre.Trim(), anio, pid, muertos.Contains(pid))));

        foreach (var grupo in filas.GroupBy(f => f.PersonajeId))
        {
            var nombre = nombres[grupo.Key];
            var muertes = grupo.Where(f => f.Murio).ToList();

            if (muertes.Count > 1)
                throw new ReglaNegocioException(
                    $"{nombre} tendría su muerte consignada dos veces ('{muertes[0].Evento}' y '{muertes[1].Evento}').");

            if (muertes.Count == 1)
            {
                var muerte = muertes[0];
                var posterior = grupo.FirstOrDefault(f => f.EventoId != muerte.EventoId && f.Fecha > muerte.Fecha);
                if (posterior is not null)
                    throw new ReglaNegocioException(
                        $"{nombre} no puede participar en '{posterior.Evento}' ({FechaGalactica.Format(posterior.Fecha)}): " +
                        $"murió en '{muerte.Evento}' ({FechaGalactica.Format(muerte.Fecha)}).");
            }
        }

        return (anio, participantes, muertos);
    }

    private async Task MarcarMuertosAsync(List<int> ids)
    {
        if (ids.Count == 0) return;
        var personajes = await db.Personajes.Where(p => ids.Contains(p.Id)).ToListAsync();
        foreach (var p in personajes) p.Estado = EstadoPersonaje.Muerto;
    }
}
