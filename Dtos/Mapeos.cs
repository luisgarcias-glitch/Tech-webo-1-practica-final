using CatalogoGalactico.Infrastructure;
using CatalogoGalactico.Models;

namespace CatalogoGalactico.Dtos;

public static class Mapeos
{
    public static PersonajeDto ToDto(this Personaje p) =>
        new(p.Id, p.Nombre, p.Especie, p.Faccion, p.Afiliacion, p.Estado, p.FuerzaSensitivo, p.Image);

    public static CardDto ToDto(this CardPersonaje c) =>
        new(c.Id, c.PersonajeId, c.Poder, c.HabilidadEspecial, c.Arma, c.NivelPeligrosidad, c.ImagenUrl);

    public static HabilidadDto ToDto(this Habilidad h) =>
        new(h.Id, h.Nombre, h.Tipo, h.Descripcion, h.Bonus);

    public static EventoDto ToDto(this Evento e) =>
        new(e.Id, e.Nombre, FechaGalactica.Format(e.Fecha), e.Fecha, e.Ubicacion, e.Descripcion,
            e.Participantes.Select(p => p.PersonajeId).OrderBy(x => x).ToList(),
            e.Participantes.Where(p => p.Murio).Select(p => p.PersonajeId).OrderBy(x => x).ToList(),
            e.Resultado, e.Ganador);

    public static UsuarioDto ToDto(this Usuario u) =>
        new(u.Id, u.NombreUsuario, u.Email, u.Telefono);
}
