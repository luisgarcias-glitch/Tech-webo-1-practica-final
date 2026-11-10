namespace CatalogoGalactico.Models;

public class EventoParticipante
{
    public int EventoId { get; set; }
    public int PersonajeId { get; set; }
    public bool Murio { get; set; }

    public Evento Evento { get; set; } = null!;
    public Personaje Personaje { get; set; } = null!;
}
