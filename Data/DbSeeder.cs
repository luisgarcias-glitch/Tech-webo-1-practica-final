using CatalogoGalactico.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalactico.Data;

public static class DbSeeder
{
    public static async Task SembrarAsync(AppDbContext db)
    {
        if (await db.Personajes.AnyAsync()) return;

        Personaje P(string nombre, string especie, Faccion faccion, string afiliacion, EstadoPersonaje estado,
                    bool fuerza, string slug, int poder, string habilidad, string arma, int nivel) => new()
        {
            Nombre = nombre, Especie = especie, Faccion = faccion, Afiliacion = afiliacion,
            Estado = estado, FuerzaSensitivo = fuerza,
            Image = $"https://example.com/personajes/{slug}.png",
            Card = new CardPersonaje
            {
                Poder = poder, HabilidadEspecial = habilidad, Arma = arma, NivelPeligrosidad = nivel,
                ImagenUrl = $"https://example.com/cartas/{slug}.png"
            }
        };

        var luke = P("Luke Skywalker", "Humano", Faccion.Rebelde, "Alianza Rebelde", EstadoPersonaje.Vivo, true, "luke", 88, "Empuje de la Fuerza", "Sable de luz verde", 8);
        var leia = P("Leia Organa", "Humana", Faccion.Rebelde, "Alianza Rebelde", EstadoPersonaje.Vivo, true, "leia", 55, "Liderazgo rebelde", "Bláster", 5);
        var han = P("Han Solo", "Humano", Faccion.Rebelde, "Alianza Rebelde", EstadoPersonaje.Vivo, false, "han", 60, "Puntería veloz", "Bláster DL-44", 6);
        var chewie = P("Chewbacca", "Wookiee", Faccion.Rebelde, "Alianza Rebelde", EstadoPersonaje.Vivo, false, "chewbacca", 72, "Fuerza wookiee", "Ballesta bowcaster", 7);
        var obiwan = P("Obi-Wan Kenobi", "Humano", Faccion.Rebelde, "Orden Jedi", EstadoPersonaje.Muerto, true, "obiwan", 85, "Mente Jedi", "Sable de luz azul", 8);
        var yoda = P("Yoda", "Desconocida", Faccion.Neutral, "Orden Jedi", EstadoPersonaje.Muerto, true, "yoda", 97, "Maestría de la Fuerza", "Sable de luz verde", 9);
        var vader = P("Darth Vader", "Humano", Faccion.Imperio, "Imperio Galáctico", EstadoPersonaje.Muerto, true, "vader", 95, "Estrangulamiento de la Fuerza", "Sable de luz rojo", 10);
        var palpatine = P("Emperador Palpatine", "Humano", Faccion.Imperio, "Imperio Galáctico", EstadoPersonaje.Muerto, true, "palpatine", 98, "Rayos de la Fuerza", "Rayos Sith", 10);
        var tarkin = P("Wilhuff Tarkin", "Humano", Faccion.Imperio, "Imperio Galáctico", EstadoPersonaje.Muerto, false, "tarkin", 45, "Estrategia imperial", "Superláser de la Estrella de la Muerte", 6);
        var boba = P("Boba Fett", "Humano", Faccion.Neutral, "Cazarrecompensas", EstadoPersonaje.Desconocido, false, "boba", 70, "Jetpack", "Bláster EE-3", 7);
        var lando = P("Lando Calrissian", "Humano", Faccion.Rebelde, "Alianza Rebelde", EstadoPersonaje.Vivo, false, "lando", 50, "Astucia de contrabandista", "Bláster", 4);

        Habilidad H(string nombre, TipoHabilidad tipo, string descripcion, int bonus) =>
            new() { Nombre = nombre, Tipo = tipo, Descripcion = descripcion, Bonus = bonus };

        var empuje = H("Empuje de la Fuerza", TipoHabilidad.Fuerza, "Mueve objetos y enemigos con la mente.", 15);
        var estrangulamiento = H("Estrangulamiento de la Fuerza", TipoHabilidad.Fuerza, "Asfixia a distancia.", 18);
        var punteria = H("Puntería veloz", TipoHabilidad.Combate, "Dispara con precisión bajo presión.", 8);
        var pilotaje = H("Pilotaje experto", TipoHabilidad.Pilotaje, "Maniobras avanzadas en combate espacial.", 10);
        var liderazgo = H("Liderazgo rebelde", TipoHabilidad.Liderazgo, "Inspira y coordina a las tropas.", 7);
        var duelo = H("Duelo de sables", TipoHabilidad.Combate, "Maestría en combate con sable de luz.", 12);
        var reparacion = H("Reparación de naves", TipoHabilidad.Tecnica, "Repara sistemas en pleno vuelo.", 6);
        var rayos = H("Rayos de la Fuerza", TipoHabilidad.Fuerza, "Descargas de energía oscura.", 20);

        void A(Personaje p, Habilidad h, int nivel) =>
            p.PersonajeHabilidades.Add(new PersonajeHabilidad { Habilidad = h, Nivel = nivel });

        A(luke, empuje, 4); A(luke, pilotaje, 3); A(luke, duelo, 4);
        A(leia, liderazgo, 5); A(leia, empuje, 2);
        A(han, punteria, 4); A(han, pilotaje, 5); A(han, reparacion, 3);
        A(chewie, reparacion, 5); A(chewie, pilotaje, 4);
        A(obiwan, duelo, 5); A(obiwan, empuje, 5);
        A(yoda, empuje, 5);
        A(vader, estrangulamiento, 5); A(vader, duelo, 5);
        A(palpatine, rayos, 5);
        A(tarkin, liderazgo, 4);
        A(boba, punteria, 5);
        A(lando, pilotaje, 4); A(lando, liderazgo, 3);

        Evento E(string nombre, int fecha, string ubicacion, string descripcion,
                 params (Personaje personaje, bool murio)[] partes)
        {
            var ev = new Evento { Nombre = nombre, Fecha = fecha, Ubicacion = ubicacion, Descripcion = descripcion };
            foreach (var (personaje, murio) in partes)
                ev.Participantes.Add(new EventoParticipante { Personaje = personaje, Murio = murio });
            return ev;
        }

        var eventos = new[]
        {
            E("Duelo en Mustafar", -19, "Mustafar", "Duelo entre maestro y aprendiz junto a ríos de lava.",
                (obiwan, false), (vader, false)),
            E("Duelo en la Estrella de la Muerte", 0, "Estrella de la Muerte", "Obi-Wan se sacrifica para permitir el escape.",
                (obiwan, true), (vader, false)),
            E("Batalla de Yavin", 0, "Yavin 4", "Ataque rebelde que destruye la primera Estrella de la Muerte.",
                (luke, false), (leia, false), (han, false), (chewie, false), (vader, false), (tarkin, true)),
            E("Batalla de Hoth", 3, "Hoth", "El Imperio asalta la base Echo.",
                (luke, false), (leia, false), (han, false), (chewie, false), (vader, false)),
            E("Muerte de Yoda", 4, "Dagobah", "Yoda se despide de Luke.",
                (luke, false), (yoda, true)),
            E("Batalla de Endor", 4, "Endor", "Batalla final contra el Emperador y la segunda Estrella de la Muerte.",
                (luke, false), (leia, false), (han, false), (chewie, false), (lando, false), (vader, true), (palpatine, true)),
        };

        db.Personajes.AddRange(luke, leia, han, chewie, obiwan, yoda, vader, palpatine, tarkin, boba, lando);
        db.Habilidades.AddRange(empuje, estrangulamiento, punteria, pilotaje, liderazgo, duelo, reparacion, rayos);
        db.Eventos.AddRange(eventos);

        await db.SaveChangesAsync();
    }
}
