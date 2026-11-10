namespace CatalogoGalactico.Models;

public class Evento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";

    public int Fecha { get; set; }

    public string Ubicacion { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string? Resultado { get; set; }
    public string? Ganador { get; set; }

    public ICollection<EventoParticipante> Participantes { get; set; } = new List<EventoParticipante>();
}
