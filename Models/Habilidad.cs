namespace CatalogoGalactico.Models;

public class Habilidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public TipoHabilidad Tipo { get; set; }
    public string Descripcion { get; set; } = "";
    public int Bonus { get; set; }

    public ICollection<PersonajeHabilidad> PersonajeHabilidades { get; set; } = new List<PersonajeHabilidad>();
}
