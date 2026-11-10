using CatalogoGalactico.Data;
using CatalogoGalactico.Dtos;
using CatalogoGalactico.Infrastructure;
using CatalogoGalactico.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalactico.Services;

public interface IHabilidadService
{
    // Catálogo
    Task<List<HabilidadDto>> ListarAsync(HabilidadFiltro filtro);
    Task<HabilidadDto> ObtenerAsync(int id);
    Task<HabilidadDto> CrearAsync(HabilidadCreateDto dto);
    Task<HabilidadDto> ActualizarAsync(int id, HabilidadCreateDto dto);
    Task EliminarAsync(int id);
    Task<List<PersonajeConHabilidadDto>> ListarPersonajesAsync(int habilidadId);

    Task<List<HabilidadAsignadaDto>> ListarPorPersonajeAsync(int personajeId);
    Task<HabilidadAsignadaDto> AsignarAsync(int personajeId, AsignarHabilidadDto dto);
    Task<HabilidadAsignadaDto> ActualizarNivelAsync(int personajeId, int habilidadId, int nivel);
    Task QuitarAsync(int personajeId, int habilidadId);
}

public class HabilidadService(AppDbContext db) : IHabilidadService
{

    public async Task<List<HabilidadDto>> ListarAsync(HabilidadFiltro f)
    {
        IQueryable<Habilidad> q = db.Habilidades.AsNoTracking();
        if (f.Tipo is not null) q = q.Where(h => h.Tipo == f.Tipo);
        if (f.BonusMinimo is not null) q = q.Where(h => h.Bonus >= f.BonusMinimo);
        if (!string.IsNullOrWhiteSpace(f.Nombre))
        {
            var nombre = f.Nombre.Trim();
            q = q.Where(h => h.Nombre.Contains(nombre));
        }

        var lista = await q.OrderBy(h => h.Id).ToListAsync();
        return lista.Select(h => h.ToDto()).ToList();
    }

    public async Task<HabilidadDto> ObtenerAsync(int id)
    {
        var h = await db.Habilidades.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException($"No existe una habilidad con id {id}.");
        return h.ToDto();
    }

    public async Task<HabilidadDto> CrearAsync(HabilidadCreateDto dto)
    {
        var nombre = dto.Nombre.Trim();
        if (await db.Habilidades.AnyAsync(h => h.Nombre == nombre))
            throw new ConflictException($"Ya existe una habilidad llamada '{nombre}'.");

        var h = new Habilidad
        {
            Nombre = nombre,
            Tipo = dto.Tipo!.Value,
            Descripcion = dto.Descripcion?.Trim() ?? "",
            Bonus = dto.Bonus
        };
        db.Habilidades.Add(h);
        await db.SaveChangesAsync();
        return h.ToDto();
    }

    public async Task<HabilidadDto> ActualizarAsync(int id, HabilidadCreateDto dto)
    {
        var h = await db.Habilidades.FindAsync(id)
                ?? throw new NotFoundException($"No existe una habilidad con id {id}.");

        var nombre = dto.Nombre.Trim();
        if (await db.Habilidades.AnyAsync(x => x.Id != id && x.Nombre == nombre))
            throw new ConflictException($"Ya existe otra habilidad llamada '{nombre}'.");

        // Si pasa a tipo Fuerza, quienes ya la tienen deben ser sensibles a la Fuerza.
        if (dto.Tipo == TipoHabilidad.Fuerza)
        {
            var invalidos = await db.PersonajeHabilidades
                .Where(ph => ph.HabilidadId == id && !ph.Personaje.FuerzaSensitivo)
                .Select(ph => ph.Personaje.Nombre)
                .ToListAsync();
            if (invalidos.Count > 0)
                throw new ConflictException(
                    $"No se puede cambiar a tipo Fuerza: la tienen personajes no sensibles a la Fuerza ({string.Join(", ", invalidos)}).");
        }

        h.Nombre = nombre;
        h.Tipo = dto.Tipo!.Value;
        h.Descripcion = dto.Descripcion?.Trim() ?? "";
        h.Bonus = dto.Bonus;

        await db.SaveChangesAsync();
        return h.ToDto();
    }

    public async Task EliminarAsync(int id)
    {
        var h = await db.Habilidades.FindAsync(id)
                ?? throw new NotFoundException($"No existe una habilidad con id {id}.");

        var usan = await db.PersonajeHabilidades
            .Where(ph => ph.HabilidadId == id)
            .Select(ph => ph.Personaje.Nombre)
            .ToListAsync();
        if (usan.Count > 0)
            throw new ConflictException(
                $"No se puede eliminar '{h.Nombre}': la usan {string.Join(", ", usan)}. Quítela primero de esos personajes.");

        db.Habilidades.Remove(h);
        await db.SaveChangesAsync();
    }

    public async Task<List<PersonajeConHabilidadDto>> ListarPersonajesAsync(int habilidadId)
    {
        if (!await db.Habilidades.AnyAsync(h => h.Id == habilidadId))
            throw new NotFoundException($"No existe una habilidad con id {habilidadId}.");

        return await db.PersonajeHabilidades.AsNoTracking()
            .Where(ph => ph.HabilidadId == habilidadId)
            .OrderByDescending(ph => ph.Nivel).ThenBy(ph => ph.Personaje.Nombre)
            .Select(ph => new PersonajeConHabilidadDto(ph.PersonajeId, ph.Personaje.Nombre, ph.Personaje.Faccion, ph.Nivel))
            .ToListAsync();
    }


    public async Task<List<HabilidadAsignadaDto>> ListarPorPersonajeAsync(int personajeId)
    {
        if (!await db.Personajes.AnyAsync(p => p.Id == personajeId))
            throw new NotFoundException($"No existe un personaje con id {personajeId}.");

        return await db.PersonajeHabilidades.AsNoTracking()
            .Where(ph => ph.PersonajeId == personajeId)
            .OrderBy(ph => ph.HabilidadId)
            .Select(ph => new HabilidadAsignadaDto(
                ph.HabilidadId, ph.Habilidad.Nombre, ph.Habilidad.Tipo,
                ph.Habilidad.Descripcion, ph.Habilidad.Bonus, ph.Nivel))
            .ToListAsync();
    }

    public async Task<HabilidadAsignadaDto> AsignarAsync(int personajeId, AsignarHabilidadDto dto)
    {
        var p = await db.Personajes.FindAsync(personajeId)
                ?? throw new NotFoundException($"No existe un personaje con id {personajeId}.");
        var h = await db.Habilidades.FindAsync(dto.HabilidadId)
                ?? throw new ReglaNegocioException($"La habilidad {dto.HabilidadId} no existe.");

        if (await db.PersonajeHabilidades.AnyAsync(x => x.PersonajeId == personajeId && x.HabilidadId == h.Id))
            throw new ConflictException($"{p.Nombre} ya tiene la habilidad '{h.Nombre}'. Use PUT para cambiar su nivel.");

        if (h.Tipo == TipoHabilidad.Fuerza && !p.FuerzaSensitivo)
            throw new ReglaNegocioException(
                $"{p.Nombre} no es sensible a la Fuerza y no puede aprender '{h.Nombre}' (tipo Fuerza).");

        db.PersonajeHabilidades.Add(new PersonajeHabilidad
        {
            PersonajeId = personajeId,
            HabilidadId = h.Id,
            Nivel = dto.Nivel
        });
        await db.SaveChangesAsync();

        return new HabilidadAsignadaDto(h.Id, h.Nombre, h.Tipo, h.Descripcion, h.Bonus, dto.Nivel);
    }

    public async Task<HabilidadAsignadaDto> ActualizarNivelAsync(int personajeId, int habilidadId, int nivel)
    {
        var ph = await db.PersonajeHabilidades.Include(x => x.Habilidad)
                     .FirstOrDefaultAsync(x => x.PersonajeId == personajeId && x.HabilidadId == habilidadId)
                 ?? throw new NotFoundException($"El personaje {personajeId} no tiene la habilidad {habilidadId}.");

        ph.Nivel = nivel;
        await db.SaveChangesAsync();

        var h = ph.Habilidad;
        return new HabilidadAsignadaDto(h.Id, h.Nombre, h.Tipo, h.Descripcion, h.Bonus, ph.Nivel);
    }

    public async Task QuitarAsync(int personajeId, int habilidadId)
    {
        var ph = await db.PersonajeHabilidades
                     .FirstOrDefaultAsync(x => x.PersonajeId == personajeId && x.HabilidadId == habilidadId)
                 ?? throw new NotFoundException($"El personaje {personajeId} no tiene la habilidad {habilidadId}.");

        db.PersonajeHabilidades.Remove(ph);
        await db.SaveChangesAsync();
    }
}
