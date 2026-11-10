using CatalogoGalactico.Models;

namespace CatalogoGalactico.Dtos;

public record ParticipanteSimuladoDto(int PersonajeId, string Nombre, int CardId, int Poder);

public record BandoSimuladoDto(
    Faccion Bando, int Combatientes, int FuerzaBase, double FactorAleatorio, double FuerzaFinal,
    List<ParticipanteSimuladoDto> Participantes);

public record SimulacionDto(
    int EventoId, string NombreEvento, BandoSimuladoDto Rebelde, BandoSimuladoDto Imperio,
    List<string> NoCombatientes, string Ganador, string Resultado, string Criterio, int? Semilla);

public record RankingItemDto(
    int Posicion, int PersonajeId, string Nombre, Faccion Faccion, int Poder, int NivelPeligrosidad);

public record MvpDto(
    int EventoId, string NombreEvento, PersonajeDto Personaje, CardDto Carta,
    List<string> ParticipantesSinCarta, string Criterio);
