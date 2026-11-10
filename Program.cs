using CatalogoGalactico.Data;
using CatalogoGalactico.Infrastructure;
using CatalogoGalactico.Security;
using CatalogoGalactico.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Falta el secreto 'ConnectionStrings:DefaultConnection'. Configúralo con: " +
        "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Server=localhost;Port=3306;Database=catalogo_galactico;User=TU_USUARIO;Password=TU_PASSWORD;\"");

builder.Services.AddSingleton<ICifrador, CifradorAesGcm>();

builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseMySql(cadenaConexion, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddScoped<IPersonajeService, PersonajeService>();
builder.Services.AddScoped<ICartaService, CartaService>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<ISimulacionService, SimulacionService>();
builder.Services.AddScoped<IEstadisticasService, EstadisticasService>();
builder.Services.AddScoped<IHabilidadService, HabilidadService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opciones =>
    opciones.AddDefaultPolicy(politica =>
        politica.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await DbSeeder.SembrarAsync(db);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "No se pudieron sembrar los datos de ejemplo. ¿Aplicaste las migraciones? (dotnet ef database update)");
    }
}

app.UseExceptionHandler();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

app.Run();
