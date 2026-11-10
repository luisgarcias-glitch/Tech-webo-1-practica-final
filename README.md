# Catálogo galáctico de personajes y eventos (Web API + MySQL + EF Core)

Web API con controladores (ASP.NET Core, .NET 10) que administra **personajes, cartas coleccionables, eventos,
habilidades y usuarios**. Guarda los datos en **MySQL con Entity Framework Core** y protege la información sensible:
secretos con **Secret Manager**, contraseñas con **hash BCrypt** y un campo sensible **cifrado con AES-GCM**.

Funciones principales: CRUD de las entidades, filtros combinables, ranking, MVP de un evento, simulación de batallas,
regla de consistencia temporal (un personaje no participa en eventos posteriores a su muerte) y actualización
automática del estado del personaje al registrar una muerte.

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL 8.x en ejecución
- Herramienta de EF Core (una sola vez): `dotnet tool install --global dotnet-ef --version 9.*`

> **Nota de versiones:** el proyecto apunta a `net10.0`, pero usa EF Core 9.x y Pomelo 9.0.0 porque el proveedor
> de MySQL (Pomelo) todavía no publica una versión para EF Core 10. EF Core 9 funciona sin problemas sobre .NET 10.
> Los paquetes de EF Core deben mantenerse en 9.x.

## Configurar los secretos (Secret Manager)

Los secretos **no** están en el código ni en `appsettings.json`. Se guardan fuera del repositorio con Secret Manager.
Desde la carpeta del proyecto:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=catalogo_galactico;User=mi_usuario;Password=mi_password_ficticia;"
dotnet user-secrets set "Cifrado:Clave" "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8="
dotnet user-secrets list
```

Los valores de arriba son **ficticios**. Cambia usuario y contraseña por los de tu MySQL, y genera tu propia clave de
cifrado (32 bytes aleatorios en Base64):

```powershell
# Windows PowerShell
$b = New-Object byte[] 32; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```
```bash
# macOS / Linux
openssl rand -base64 32
```

- `ConnectionStrings:DefaultConnection`: cadena de conexión a MySQL.
- `Cifrado:Clave`: clave AES-256 usada para cifrar el teléfono de los usuarios.

> Si pierdes la clave de cifrado, los teléfonos ya guardados no se pueden descifrar.

## Aplicar las migraciones

Las migraciones están en la carpeta `Migrations/` y forman parte del repositorio.

```bash
dotnet ef database update
```

Esto crea la base de datos `catalogo_galactico` (si no existe) y todas las tablas.

### Cómo se crearon las dos migraciones (para repetirlo desde cero)

Se parte con la carpeta `Migrations/` vacía.

**1. Migración `Inicial`** (con `TelefonoCifradoHabilitado = false` en `Data/AppDbContext.cs`, que es el valor por defecto):

```bash
dotnet ef migrations add Inicial
dotnet ef database update
git add . && git commit -m "Migración inicial: personajes, cartas, eventos, habilidades y usuarios"
```

**2. Migración `AgregarTelefonoCifrado`** (evoluciona el esquema: agrega la columna cifrada `Telefono` a `Usuarios`):

1. En `Data/AppDbContext.cs` cambia `TelefonoCifradoHabilitado` de `false` a `true`.
2. Ejecuta:

```bash
dotnet ef migrations add AgregarTelefonoCifrado
dotnet ef database update
git add . && git commit -m "Migración: agrega teléfono cifrado a usuarios"
```

### Si algo falla

- *"Falta el secreto ..."*: ejecuta los comandos desde la carpeta del proyecto y revisa con `dotnet user-secrets list`.
- *Error de conexión*: verifica que MySQL esté encendido y que usuario, contraseña y puerto de la cadena sean correctos.
- *Conflicto de versiones de EF Core (NU1107/NU1608)*: todos los paquetes de EF Core deben ser 9.x (ver nota de versiones).

## Ejecutar

```bash
dotnet run
```

Swagger: `http://localhost:5100/swagger`. En desarrollo, al iniciar se insertan datos de ejemplo si las tablas están vacías.

## Endpoints

| Recurso | Rutas |
|---|---|
| Personajes | `GET /personajes` (filtros), `GET /personajes/{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Personajes (extra) | `GET /personajes/ranking?por=poder`, `GET /personajes/{id}/eventos`, `GET /personajes/{id}/con-card` |
| Cartas | `GET /cartas`, `GET /cartas/{id}`, `POST /cartas`, `PUT /cartas/{id}` |
| Eventos | `GET /eventos`, `GET /eventos/{id}`, `POST`, `PUT /{id}`, `GET /eventos/{id}/mvp`, `POST /eventos/{id}/simular` |
| Habilidades (componente propio) | `GET/POST/PUT/DELETE /habilidades`, `GET /habilidades/{id}/personajes` |
| Habilidades de un personaje | `GET/POST /personajes/{id}/habilidades`, `PUT/DELETE /personajes/{id}/habilidades/{habilidadId}` |
| Usuarios | `GET /usuarios`, `GET /usuarios/{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |

Filtros de `GET /personajes`: `faccion`, `estado`, `fuerzaSensitivo`, `especie`, `afiliacion`, `nombre`
(ej. `/personajes?faccion=Imperio&fuerzaSensitivo=true`).

**Códigos HTTP:** `200` OK, `201` al crear, `204` al eliminar, `400` datos o reglas inválidas, `404` no existe y
`409` conflicto (nombre duplicado, recurso en uso, habilidad ya asignada, carta ya existente). Los errores usan el
formato ProblemDetails.

## Modelo de datos

| Tabla | Descripción |
|---|---|
| `Personajes` | Datos del personaje (incluye `Image`, la URL de su imagen). |
| `Cartas` | Atributos de juego. Relación **1:1** con Personajes. |
| `Eventos` | Hechos narrativos. La fecha se guarda como año entero (10 BBY = -10, Batalla de Yavin = 0, 3 ABY = 3). |
| `EventoParticipantes` | Tabla intermedia **N:N** Eventos-Personajes. Incluye `Murio`. |
| `Habilidades` | Catálogo de habilidades (**componente propio**). |
| `PersonajeHabilidades` | Tabla intermedia **N:N** Personajes-Habilidades, con `Nivel` (1-5). Clave compuesta: evita duplicados. |
| `Usuarios` | Usuarios con contraseña hasheada y teléfono cifrado. |

**Relaciones 1:N:** Evento→EventoParticipantes, Personaje→EventoParticipantes, Personaje→PersonajeHabilidades,
Habilidad→PersonajeHabilidades.

## Reglas de negocio

1. Al registrar una muerte en un evento (`personajesMuertos`), el personaje pasa a estado `Muerto` en la misma transacción.
2. Un personaje solo puede morir una vez y no puede participar en un evento **posterior** (año mayor) a su muerte.
3. No se elimina un personaje que participa en eventos; al eliminarlo se borran su carta y sus habilidades.
4. Las habilidades de tipo `Fuerza` solo se asignan a personajes con `fuerzaSensitivo = true`.
5. No se elimina una habilidad que algún personaje tiene asignada.
6. **Simulación:** suma el poder de las cartas por bando (Rebelde/Imperio), aplica un factor aleatorio entre 0.90 y 1.10
   por bando y declara ganador al mayor. Con `?semilla=42` el resultado es reproducible. Si faltan participantes o
   cartas responde `400` sin guardar nada.

## Protección de datos

- **Contraseñas:** se guardan como hash **BCrypt** (con sal incluida, costo 12) en `Usuarios.PasswordHash`. La API nunca las devuelve.
- **Campo cifrado:** `Usuarios.Telefono` se cifra con **AES-256-GCM** antes de guardarse y se descifra al leerlo
  (`ValueConverter` en `Data/AppDbContext.cs`, lógica en `Security/CifradorAesGcm.cs`).
- **Dónde está la clave:** en Secret Manager (`Cifrado:Clave`), fuera del repositorio.

**Cómo demostrarlo:**

```bash
curl -X POST http://localhost:5100/usuarios -H "Content-Type: application/json" \
  -d '{"nombreUsuario":"padawan","email":"padawan@ejemplo.com","password":"ClaveSegura123","telefono":"+591 70000000"}'
curl http://localhost:5100/usuarios/1      # la API devuelve el teléfono descifrado, sin hash
```
```sql
SELECT Id, NombreUsuario, PasswordHash, Telefono FROM Usuarios;
-- PasswordHash: empieza con $2a$12$... (hash BCrypt)   Telefono: texto Base64 ilegible (cifrado)
```

> El teléfono cifrado no se puede buscar ni filtrar con SQL, porque cada cifrado usa un valor aleatorio distinto.

## Estructura

| Carpeta | Contenido |
|---|---|
| `Models/` | Entidades de EF Core y enumeraciones |
| `Dtos/` | DTOs de solicitud y respuesta (con validaciones) y mapeos |
| `Data/` | `AppDbContext` (configuración del modelo) y datos de ejemplo |
| `Services/` | Lógica de negocio (interfaz + clase, inyectados por DI) |
| `Controllers/` | Controladores de la API |
| `Security/` | Cifrado AES-GCM |
| `Infrastructure/` | Excepciones, manejo de errores y fechas BBY/ABY |
| `Migrations/` | Migraciones de EF Core (se generan con `dotnet ef`) |

## Decisiones de diseño

- **Servicios Scoped:** dependen de `AppDbContext`, que es Scoped (una instancia por solicitud). Un servicio Singleton no puede depender de uno Scoped. `CifradorAesGcm` es Singleton porque solo lee la clave de configuración.
- **Excepciones → HTTP:** los servicios lanzan `NotFoundException` (404), `ReglaNegocioException` (400) y `ConflictException` (409); `ManejadorExcepciones` las convierte en respuestas.
- **Rutas sin prefijo `/api`:** se mantienen las rutas del proyecto original para que el frontend React siga funcionando.
- **Componente propio:** tabla de Habilidades con relación N:N a Personajes, con nivel y regla de Fuerza.
