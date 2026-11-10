using CatalogoGalactico.Data;
using CatalogoGalactico.Dtos;
using CatalogoGalactico.Infrastructure;
using CatalogoGalactico.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalactico.Services;

public interface IUsuarioService
{
    Task<List<UsuarioDto>> ListarAsync();
    Task<UsuarioDto> ObtenerAsync(int id);
    Task<UsuarioDto> CrearAsync(UsuarioCreateDto dto);
    Task<UsuarioDto> ActualizarAsync(int id, UsuarioUpdateDto dto);
    Task EliminarAsync(int id);
}


public class UsuarioService(AppDbContext db) : IUsuarioService
{
    private const int CostoBcrypt = 12;

    public async Task<List<UsuarioDto>> ListarAsync()
    {
        var lista = await db.Usuarios.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
        return lista.Select(u => u.ToDto()).ToList();
    }

    public async Task<UsuarioDto> ObtenerAsync(int id)
    {
        var u = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException($"No existe un usuario con id {id}.");
        return u.ToDto();
    }

    public async Task<UsuarioDto> CrearAsync(UsuarioCreateDto dto)
    {
        var nombre = dto.NombreUsuario.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre))
            throw new ConflictException($"El nombre de usuario '{nombre}' ya está en uso.");
        if (await db.Usuarios.AnyAsync(u => u.Email == email))
            throw new ConflictException($"El email '{email}' ya está registrado.");

        var usuario = new Usuario
        {
            NombreUsuario = nombre,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, CostoBcrypt),
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim()
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario.ToDto();
    }

    public async Task<UsuarioDto> ActualizarAsync(int id, UsuarioUpdateDto dto)
    {
        var u = await db.Usuarios.FindAsync(id)
                ?? throw new NotFoundException($"No existe un usuario con id {id}.");

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(x => x.Id != id && x.Email == email))
            throw new ConflictException($"El email '{email}' ya está registrado.");

        u.Email = email;
        u.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();

        await db.SaveChangesAsync();
        return u.ToDto();
    }

    public async Task EliminarAsync(int id)
    {
        var u = await db.Usuarios.FindAsync(id)
                ?? throw new NotFoundException($"No existe un usuario con id {id}.");
        db.Usuarios.Remove(u);
        await db.SaveChangesAsync();
    }
}
