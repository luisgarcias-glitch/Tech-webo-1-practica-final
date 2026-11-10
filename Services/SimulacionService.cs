using CatalogoGalactico.Data;
using CatalogoGalactico.Dtos;
using CatalogoGalactico.Infrastructure;
using CatalogoGalactico.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalactico.Services;

public interface ISimulacionService
{
    Task<SimulacionDto> SimularAsync(int eventoId, int? semilla);
}

public class SimulacionService(AppDbContext db) : ISimulacionService
{
    public const double FactorMinimo = 0.90;
    public const double FactorMaximo = 1.10;

 
    public async Task<SimulacionDto> SimularAsync(int eventoId, int? semilla)
    {
        var ev = await db.Eventos.Include(e => e.Participantes).FirstOrDefaultAsync(e => e.Id == eventoId)
                 ?? throw new NotFoundException($"No existe un evento con id {eventoId}.");

        if (ev.Participantes.Count < 2)
            throw new ReglaNegocioException(
                $"El evento '{ev.Nombre}' necesita al menos 2 participantes para simular una batalla.");

        var ids = ev.Participantes.Select(p => p.PersonajeId).ToList();
        var personajes = await db.Personajes.Include(p => p.Card).Where(p => ids.Contains(p.Id)).ToListAsync();

        var sinCarta = personajes.Where(p => p.Card is null).Select(p => p.Nombre).ToList();
        if (sinCarta.Count > 0)
            throw new ReglaNegocioException($"No se puede simular: faltan cartas para {string.Join(", ", sinCarta)}.");

        var rebeldes = personajes.Where(p => p.Faccion == Faccion.Rebelde).ToList();
        var imperiales = personajes.Where(p => p.Faccion == Faccion.Imperio).ToList();
        var neutrales = personajes.Where(p => p.Faccion == Faccion.Neutral).Select(p => p.Nombre).ToList();

        if (rebeldes.Count == 0 || imperiales.Count == 0)
            throw new ReglaNegocioException(
                "No se puede simular: se necesita al menos un participante Rebelde y uno del Imperio " +
                $"(hay {rebeldes.Count} rebeldes y {imperiales.Count} imperiales; los Neutrales no combaten).");

        var rnd = semilla is null ? new Random() : new Random(semilla.Value);
        var bandoR = ArmarBando(Faccion.Rebelde, rebeldes, rnd);
        var bandoI = ArmarBando(Faccion.Imperio, imperiales, rnd);

        var ganador = bandoR.FuerzaFinal > bandoI.FuerzaFinal ? "Rebelde"
                    : bandoI.FuerzaFinal > bandoR.FuerzaFinal ? "Imperio"
                    : "Empate";

        var resultado = ganador == "Empate"
            ? $"Empate: ambos bandos alcanzan {bandoR.FuerzaFinal}."
            : $"Victoria {ganador}: Rebelde {bandoR.FuerzaFinal} vs Imperio {bandoI.FuerzaFinal}.";

        var criterio =
            $"Fuerza base = suma del poder de las cartas de cada bando. Se multiplica por un factor aleatorio " +
            $"entre {FactorMinimo:0.00} y {FactorMaximo:0.00} (uno por bando). Gana la mayor fuerza final; " +
            "si son iguales, hay empate. Los personajes Neutrales no combaten.";

        ev.Resultado = resultado;
        ev.Ganador = ganador;
        await db.SaveChangesAsync();

        return new SimulacionDto(ev.Id, ev.Nombre, bandoR, bandoI, neutrales, ganador, resultado, criterio, semilla);
    }

    private static BandoSimuladoDto ArmarBando(Faccion faccion, List<Personaje> miembros, Random rnd)
    {
        var baseFuerza = miembros.Sum(m => m.Card!.Poder);
        var factor = Math.Round(FactorMinimo + rnd.NextDouble() * (FactorMaximo - FactorMinimo), 4);
        var final = Math.Round(baseFuerza * factor, 2);
        var detalle = miembros
            .Select(m => new ParticipanteSimuladoDto(m.Id, m.Nombre, m.Card!.Id, m.Card.Poder))
            .ToList();
        return new BandoSimuladoDto(faccion, miembros.Count, baseFuerza, factor, final, detalle);
    }
}
