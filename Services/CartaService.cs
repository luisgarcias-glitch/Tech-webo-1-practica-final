using CatalogoGalactico.Data;
using CatalogoGalactico.Dtos;
using CatalogoGalactico.Infrastructure;
using CatalogoGalactico.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalactico.Services;

public interface ICartaService
{
    Task<List<CardDto>> ListarAsync(CardFiltro filtro);
    Task<CardDto> ObtenerAsync(int id);
    Task<CardDto> CrearAsync(CardCreateDto dto);
    Task<CardDto> ActualizarAsync(int id, CardCreateDto dto);
}

public class CartaService(AppDbContext db) : ICartaService
{
    public async Task<List<CardDto>> ListarAsync(CardFiltro f)
    {
        IQueryable<CardPersonaje> q = db.Cartas.AsNoTracking();
        if (f.PersonajeId is not null) q = q.Where(c => c.PersonajeId == f.PersonajeId);
        if (f.PoderMinimo is not null) q = q.Where(c => c.Poder >= f.PoderMinimo);
        if (f.PoderMaximo is not null) q = q.Where(c => c.Poder <= f.PoderMaximo);
        if (f.PeligrosidadMinima is not null) q = q.Where(c => c.NivelPeligrosidad >= f.PeligrosidadMinima);

        var lista = await q.OrderBy(c => c.Id).ToListAsync();
        return lista.Select(c => c.ToDto()).ToList();
    }

    public async Task<CardDto> ObtenerAsync(int id)
    {
        var c = await db.Cartas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException($"No existe una carta con id {id}.");
        return c.ToDto();
    }

    public async Task<CardDto> CrearAsync(CardCreateDto dto)
    {
        if (!await db.Personajes.AnyAsync(p => p.Id == dto.PersonajeId))
            throw new ReglaNegocioException($"El personaje {dto.PersonajeId} no existe.");

        if (await db.Cartas.AnyAsync(c => c.PersonajeId == dto.PersonajeId))
            throw new ConflictException($"El personaje {dto.PersonajeId} ya tiene una carta (relación 1:1).");

        var c = new CardPersonaje
        {
            PersonajeId = dto.PersonajeId,
            Poder = dto.Poder,
            HabilidadEspecial = dto.HabilidadEspecial.Trim(),
            Arma = dto.Arma.Trim(),
            NivelPeligrosidad = dto.NivelPeligrosidad,
            ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim()
        };
        db.Cartas.Add(c);
        await db.SaveChangesAsync();
        return c.ToDto();
    }

    public async Task<CardDto> ActualizarAsync(int id, CardCreateDto dto)
    {
        var c = await db.Cartas.FindAsync(id)
                ?? throw new NotFoundException($"No existe una carta con id {id}.");

        if (dto.PersonajeId != c.PersonajeId)
            throw new ReglaNegocioException("No se puede cambiar el personaje al que pertenece la carta.");

        c.Poder = dto.Poder;
        c.HabilidadEspecial = dto.HabilidadEspecial.Trim();
        c.Arma = dto.Arma.Trim();
        c.NivelPeligrosidad = dto.NivelPeligrosidad;
        c.ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim();

        await db.SaveChangesAsync();
        return c.ToDto();
    }
}
