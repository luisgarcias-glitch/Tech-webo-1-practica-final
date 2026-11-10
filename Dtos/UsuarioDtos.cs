using System.ComponentModel.DataAnnotations;

namespace CatalogoGalactico.Dtos;

public record UsuarioCreateDto
{
    [Required, StringLength(50, MinimumLength = 3)] public string NombreUsuario { get; init; } = "";
    [Required, EmailAddress, StringLength(150)] public string Email { get; init; } = "";

    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; init; } = "";

    [StringLength(30)] public string? Telefono { get; init; }
}

public record UsuarioUpdateDto
{
    [Required, EmailAddress, StringLength(150)] public string Email { get; init; } = "";
    [StringLength(30)] public string? Telefono { get; init; }
}

public record UsuarioDto(int Id, string NombreUsuario, string Email, string? Telefono);
