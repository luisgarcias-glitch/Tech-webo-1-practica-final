namespace CatalogoGalactico.Models;

public class PersonajeHabilidad
{
    public int PersonajeId { get; set; }
    public int HabilidadId { get; set; }
    public int Nivel { get; set; }

    public Personaje Personaje { get; set; } = null!;
    public Habilidad Habilidad { get; set; } = null!;
}
