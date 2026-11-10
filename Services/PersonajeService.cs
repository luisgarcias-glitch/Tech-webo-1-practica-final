using CatalogoGalactico.Data;
using CatalogoGalactico.Dtos;
using CatalogoGalactico.Infrastructure;
using CatalogoGalactico.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalactico.Services;

public interface IPersonajeService
{
    Task<List<PersonajeDto>> ListarAsync(PersonajeFiltro filtro);
    Task<PersonajeDto> ObtenerAsync(int id);
    Task<PersonajeConCardDto> ObtenerConCardAsync(int id);
    Task<PersonajeDto> CrearAsync(PersonajeCreateDto dto);
    Task<PersonajeDto> ActualizarAsync(int id, PersonajeCreateDto dto);
    Task EliminarAsync(int id);
}

public class PersonajeService(AppDbContext db) : IPersonajeService
{
    public async Task<List<PersonajeDto>> ListarAsync(PersonajeFiltro filtro)
    {
        IQueryable<Personaje> q = db.Personajes.AsNoTracking();

        if (filtro.Faccion is not null) q = q.Where(p => p.Faccion == filtro.Faccion);
        if (filtro.Estado is not null) q = q.Where(p => p.Estado == filtro.Estado);
        if (filtro.FuerzaSensitivo is not null) q = q.Where(p => p.FuerzaSensitivo == filtro.FuerzaSensitivo);

        if (!string.IsNullOrWhiteSpace(filtro.Especie))
        {
            var especie = filtro.Especie.Trim();
            q = q.Where(p => p.Especie.Contains(especie));
        }
        if (!string.IsNullOrWhiteSpace(filtro.Afiliacion))
        {
            var afiliacion = filtro.Afiliacion.Trim();
            q = q.Where(p => p.Afiliacion.Contains(afiliacion));
        }
        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            var nombre = filtro.Nombre.Trim();
            q = q.Where(p => p.Nombre.Contains(nombre));
        }

        var lista = await q.OrderBy(p => p.Id).ToListAsync();
        return lista.Select(p => p.ToDto()).ToList();
    }

    public async Task<PersonajeDto> ObtenerAsync(int id)
    {
        var p = await db.Personajes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException($"No existe un personaje con id {id}.");
        return p.ToDto();
    }

    public async Task<PersonajeConCardDto> ObtenerConCardAsync(int id)
    {
        var p = await db.Personajes.AsNoTracking().Include(x => x.Card).FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException($"No existe un personaje con id {id}.");

        if (p.Card is null)
            throw new NotFoundException($"{p.Nombre} todavía no tiene carta.");

        var habilidades = await db.PersonajeHabilidades.AsNoTracking()
            .Where(ph => ph.PersonajeId == id)
            .OrderBy(ph => ph.HabilidadId)
            .Select(ph => new HabilidadAsignadaDto(
                ph.HabilidadId, ph.Habilidad.Nombre, ph.Habilidad.Tipo,
                ph.Habilidad.Descripcion, ph.Habilidad.Bonus, ph.Nivel))
            .ToListAsync();

        var c = p.Card;
        return new PersonajeConCardDto(
            p.Id, p.Nombre, p.Especie, p.Faccion, p.Afiliacion, p.Estado, p.FuerzaSensitivo, p.Image,
            c.Poder, c.HabilidadEspecial, c.Arma, c.NivelPeligrosidad, c.ToDto(), habilidades);
    }

    public async Task<PersonajeDto> CrearAsync(PersonajeCreateDto dto)
    {
        var nombre = dto.Nombre.Trim();
        if (await db.Personajes.AnyAsync(p => p.Nombre == nombre))
            throw new ConflictException($"Ya existe un personaje llamado '{nombre}'.");

        var p = new Personaje
        {
            Nombre = nombre,
            Especie = dto.Especie.Trim(),
            Faccion = dto.Faccion!.Value,
            Afiliacion = dto.Afiliacion.Trim(),
            Estado = dto.Estado ?? EstadoPersonaje.Vivo,
            FuerzaSensitivo = dto.FuerzaSensitivo,
            Image = string.IsNullOrWhiteSpace(dto.Image) ? null : dto.Image.Trim()
        };

        db.Personajes.Add(p);
        await db.SaveChangesAsync();
        return p.ToDto();
    }

    public async Task<PersonajeDto> ActualizarAsync(int id, PersonajeCreateDto dto)
    {
        var p = await db.Personajes.FindAsync(id)
                ?? throw new NotFoundException($"No existe un personaje con id {id}.");

        var nombre = dto.Nombre.Trim();
        if (await db.Personajes.AnyAsync(x => x.Id != id && x.Nombre == nombre))
            throw new ConflictException($"Ya existe otro personaje llamado '{nombre}'.");

        // Si un evento registra su muerte, no se puede volver a marcar como vivo desde aquí.
        var nuevoEstado = dto.Estado ?? p.Estado;
        if (nuevoEstado != EstadoPersonaje.Muerto &&
            await db.EventoParticipantes.AnyAsync(ep => ep.PersonajeId == id && ep.Murio))
            throw new ConflictException(
                $"{p.Nombre} tiene su muerte registrada en un evento; no se puede cambiar su estado a '{nuevoEstado}'. " +
                "Modifique primero el evento correspondiente.");

        if (!dto.FuerzaSensitivo &&
            await db.PersonajeHabilidades.AnyAsync(ph => ph.PersonajeId == id && ph.Habilidad.Tipo == TipoHabilidad.Fuerza))
            throw new ConflictException(
                $"{p.Nombre} tiene habilidades de tipo Fuerza; quíteselas antes de marcarlo como no sensible a la Fuerza.");

        p.Nombre = nombre;
        p.Especie = dto.Especie.Trim();
        p.Faccion = dto.Faccion!.Value;
        p.Afiliacion = dto.Afiliacion.Trim();
        p.Estado = nuevoEstado;
        p.FuerzaSensitivo = dto.FuerzaSensitivo;
        p.Image = string.IsNullOrWhiteSpace(dto.Image) ? null : dto.Image.Trim();

        await db.SaveChangesAsync();
        return p.ToDto();
    }

    public async Task EliminarAsync(int id)
    {
        var p = await db.Personajes.FindAsync(id)
                ?? throw new NotFoundException($"No existe un personaje con id {id}.");

        var evento = await db.EventoParticipantes
            .Where(ep => ep.PersonajeId == id)
            .Select(ep => ep.Evento.Nombre)
            .FirstOrDefaultAsync();
        if (evento is not null)
            throw new ConflictException(
                $"No se puede eliminar a {p.Nombre}: participa en el evento '{evento}'. Quítelo de los eventos primero.");

        db.Personajes.Remove(p);
        await db.SaveChangesAsync();
    }
}
