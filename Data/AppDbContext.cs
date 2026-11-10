using CatalogoGalactico.Models;
using CatalogoGalactico.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CatalogoGalactico.Data;

public class AppDbContext : DbContext
{
   
    private static readonly bool TelefonoCifradoHabilitado = false;

    private readonly ICifrador _cifrador;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICifrador cifrador) : base(options)
    {
        _cifrador = cifrador;
    }

    public DbSet<Personaje> Personajes => Set<Personaje>();
    public DbSet<CardPersonaje> Cartas => Set<CardPersonaje>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<EventoParticipante> EventoParticipantes => Set<EventoParticipante>();
    public DbSet<Habilidad> Habilidades => Set<Habilidad>();
    public DbSet<PersonajeHabilidad> PersonajeHabilidades => Set<PersonajeHabilidad>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Personaje>(e =>
        {
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            e.HasIndex(p => p.Nombre).IsUnique();
            e.Property(p => p.Especie).IsRequired().HasMaxLength(60);
            e.Property(p => p.Faccion).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Afiliacion).IsRequired().HasMaxLength(100);
            e.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Image).HasMaxLength(500);
        });

        mb.Entity<CardPersonaje>(e =>
        {
            e.Property(c => c.HabilidadEspecial).IsRequired().HasMaxLength(100);
            e.Property(c => c.Arma).IsRequired().HasMaxLength(100);
            e.Property(c => c.ImagenUrl).HasMaxLength(500);
            e.HasOne(c => c.Personaje)
                .WithOne(p => p.Card)
                .HasForeignKey<CardPersonaje>(c => c.PersonajeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<Evento>(e =>
        {
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(150);
            e.Property(x => x.Ubicacion).IsRequired().HasMaxLength(100);
            e.Property(x => x.Descripcion).IsRequired().HasMaxLength(1000);
            e.Property(x => x.Resultado).HasMaxLength(300);
            e.Property(x => x.Ganador).HasMaxLength(20);
        });

        mb.Entity<EventoParticipante>(e =>
        {
            e.HasKey(ep => new { ep.EventoId, ep.PersonajeId });
            e.HasOne(ep => ep.Evento)
                .WithMany(ev => ev.Participantes)
                .HasForeignKey(ep => ep.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ep => ep.Personaje)
                .WithMany(p => p.Participaciones)
                .HasForeignKey(ep => ep.PersonajeId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

        mb.Entity<Habilidad>(e =>
        {
            e.Property(h => h.Nombre).IsRequired().HasMaxLength(100);
            e.HasIndex(h => h.Nombre).IsUnique();
            e.Property(h => h.Tipo).HasConversion<string>().HasMaxLength(20);
            e.Property(h => h.Descripcion).IsRequired().HasMaxLength(300);
        });

       
        mb.Entity<PersonajeHabilidad>(e =>
        {
            e.HasKey(ph => new { ph.PersonajeId, ph.HabilidadId });
            e.HasOne(ph => ph.Personaje)
                .WithMany(p => p.PersonajeHabilidades)
                .HasForeignKey(ph => ph.PersonajeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ph => ph.Habilidad)
                .WithMany(h => h.PersonajeHabilidades)
                .HasForeignKey(ph => ph.HabilidadId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

        mb.Entity<Usuario>(e =>
        {
            e.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
            e.HasIndex(u => u.NombreUsuario).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(150);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100); 
        });

        if (!TelefonoCifradoHabilitado)
        {
            mb.Entity<Usuario>().Ignore(u => u.Telefono);
        }
        else
        {
            
            var convertidorCifrado = new ValueConverter<string?, string?>(
                valor => valor == null ? null : _cifrador.Cifrar(valor),
                valor => valor == null ? null : _cifrador.Descifrar(valor));

            mb.Entity<Usuario>()
                .Property(u => u.Telefono)
                .HasMaxLength(500)
                .HasConversion(convertidorCifrado);
        }
    }
}
