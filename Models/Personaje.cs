namespace CatalogoGalactico.Models;

public class Personaje
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Especie { get; set; } = "";
    public Faccion Faccion { get; set; }
    public string Afiliacion { get; set; } = "";
    public EstadoPersonaje Estado { get; set; } = EstadoPersonaje.Vivo;
    public bool FuerzaSensitivo { get; set; }
    public string? Image { get; set; }

    public CardPersonaje? Card { get; set; }

    public ICollection<EventoParticipante> Participaciones { get; set; } = new List<EventoParticipante>();

    public ICollection<PersonajeHabilidad> PersonajeHabilidades { get; set; } = new List<PersonajeHabilidad>();
}
