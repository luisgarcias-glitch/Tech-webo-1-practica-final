namespace CatalogoGalactico.Models;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public string? Telefono { get; set; }
}
