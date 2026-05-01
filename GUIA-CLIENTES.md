# Guía de Clientes — dos temas en uno

Este archivo cubre dos cosas distintas que comparten la palabra "cliente":

| Sección | Qué es |
|---|---|
| **A — Módulo de Clientes** | Agregar el CRUD de personas (clientes del negocio) al código |
| **B — Nueva instancia por empresa** | Desplegar una copia del sistema para otra empresa |

---

# SECCIÓN A — Módulo de Clientes (código)

## Por qué este flujo existe

El proyecto usa **Clean Architecture**: el código está dividido en capas que solo
se conocen en una dirección. Cada capa tiene una responsabilidad única.

```
Domain          ← el núcleo. Define qué SON las cosas. No sabe nada del resto.
Application     ← define qué SE PUEDE HACER y cómo. No sabe cómo se guarda.
Infrastructure  ← habla con la base de datos. Implementa lo que Application pidió.
Endpoints       ← expone todo por HTTP. Solo llama al servicio, nada más.
```

**¿Por qué así y no todo en un solo archivo?**
- Si cambias la base de datos (de Postgres a MySQL), solo tocas Infrastructure.
- Si cambias el formato de la API, solo tocas Endpoints.
- Si cambias una regla de negocio, solo tocas Application.
- Cada pieza es fácil de probar por separado.

El flujo de una petición HTTP es siempre el mismo:

```
HTTP request
    → Endpoint         (recibe y valida la forma del dato)
    → Service          (aplica la lógica de negocio)
    → Repository       (va a la BD y devuelve la entidad)
    → Service          (convierte entidad → DTO)
    → Endpoint         (devuelve la respuesta HTTP)
```

Sigue los pasos en orden: cada uno depende del anterior.

---

## PASO A-1 — Entidad de dominio

**CREAR:** `Domain/Entities/Cliente.cs`

**Qué es:** La representación pura de un Cliente en C#. Son solo propiedades,
sin lógica, sin referencias a la API ni a la base de datos. Es el "molde" del objeto.

**Por qué está en Domain:** Domain es el núcleo del sistema. No depende de nada externo.
Si mañana cambias el framework o la BD, esta clase no se toca.

```csharp
namespace ProductCatalog.Api.Domain.Entities;

public sealed class Cliente
{
    public int CliId { get; set; }
    public string CliNombre { get; set; } = string.Empty;
    public string CliApellido { get; set; } = string.Empty;
    public string? CliCorreo { get; set; }
    public string? CliCelular { get; set; }
    public string? CliDireccion { get; set; }
}
```

> Los campos con `?` son opcionales — pueden ser null en la base de datos.
> El prefijo `Cli` es la convención del proyecto para identificar a qué tabla pertenece cada campo.

---

## PASO A-2 — DTOs (Data Transfer Objects)

**CREAR:** `Application/DTOs/ClienteDtos.cs`

**Qué es:** Los DTOs son los "formularios" de entrada y salida de la API.
Son distintos de la entidad porque la API no siempre muestra ni recibe los mismos campos que tiene la BD.

- `ClienteDto` → lo que **devuelve** la API (incluye el ID, que la BD generó).
- `CreateClienteDto` → lo que **recibe** al crear (sin ID, lo asigna la BD sola).
- `UpdateClienteDto` → lo que **recibe** al actualizar (sin ID, viene en la URL).

**Por qué no usar la entidad directamente:** La entidad podría tener campos internos
que no quieres exponer (contraseñas, flags internos). El DTO te da control total
sobre qué entra y qué sale.

**Por qué `[property: Description]`:** Esos atributos aparecen en Swagger como
descripción de cada campo. Sin ellos, Swagger solo muestra el nombre.

```csharp
using System.ComponentModel;

namespace ProductCatalog.Api.Application.DTOs;

public sealed record ClienteDto(
    [property: Description("ID único del cliente.")]
    int CliId,
    [property: Description("Nombre del cliente.")]
    string CliNombre,
    [property: Description("Apellido del cliente.")]
    string CliApellido,
    [property: Description("Correo electrónico (opcional).")]
    string? CliCorreo,
    [property: Description("Número de celular (opcional).")]
    string? CliCelular,
    [property: Description("Dirección (opcional).")]
    string? CliDireccion
);

public sealed record CreateClienteDto(
    [property: Description("Nombre. Requerido, máx. 100 caracteres.")]
    string CliNombre,
    [property: Description("Apellido. Requerido, máx. 100 caracteres.")]
    string CliApellido,
    [property: Description("Correo electrónico opcional. Máx. 150 caracteres.")]
    string? CliCorreo,
    [property: Description("Celular opcional. Máx. 20 caracteres.")]
    string? CliCelular,
    [property: Description("Dirección opcional. Máx. 200 caracteres.")]
    string? CliDireccion
);

public sealed record UpdateClienteDto(
    [property: Description("Nombre. Requerido, máx. 100 caracteres.")]
    string CliNombre,
    [property: Description("Apellido. Requerido, máx. 100 caracteres.")]
    string CliApellido,
    [property: Description("Correo electrónico opcional. Máx. 150 caracteres.")]
    string? CliCorreo,
    [property: Description("Celular opcional. Máx. 20 caracteres.")]
    string? CliCelular,
    [property: Description("Dirección opcional. Máx. 200 caracteres.")]
    string? CliDireccion
);
```

---

## PASO A-3 — Interfaz del repositorio

**CREAR:** `Application/Interfaces/IClienteRepository.cs`

**Qué es:** Un contrato que dice "alguien me va a dar estas operaciones de base de datos,
pero no me importa cómo las implementa". La `I` al inicio es convención de C# para interfaces.

**Por qué existe:** El Servicio (capa de lógica) necesita hablar con la BD,
pero no debe saber si es Postgres, MySQL o un archivo de texto. Solo conoce este contrato.
Cuando el proyecto arranca, `Program.cs` le dice: "cuando alguien pida `IClienteRepository`,
dale `ClienteRepository`" (que es el que sí habla con Postgres).

```csharp
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default);
    Task<Cliente?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Cliente> CreateAsync(Cliente cliente, CancellationToken ct = default);
    Task<Cliente> UpdateAsync(Cliente cliente, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
```

> `Task<>` significa que la operación es asíncrona (no bloquea el hilo mientras espera la BD).
> `CancellationToken` permite cancelar la operación si el cliente HTTP corta la conexión.

---

## PASO A-4 — Interfaz del servicio

**CREAR:** `Application/Interfaces/IClienteService.cs`

**Qué es:** El mismo concepto de contrato, pero para la capa de lógica de negocio.
El Endpoint solo conoce esta interfaz, nunca la implementación concreta.

**Por qué dos interfaces (repositorio Y servicio):** Son capas distintas con
responsabilidades distintas. El repositorio maneja datos crudos (entidades).
El servicio maneja lógica (valida que exista, convierte a DTO, aplica reglas).
El endpoint solo habla con el servicio.

```csharp
using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> GetAllAsync(CancellationToken ct = default);
    Task<ClienteDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClienteDto> CreateAsync(CreateClienteDto dto, CancellationToken ct = default);
    Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
```

> Nota que el repositorio trabaja con `Cliente` (entidad) y el servicio con `ClienteDto`.
> La conversión entre los dos la hace el servicio.

---

## PASO A-5 — Validadores

**CREAR:** `Application/Validators/ClienteValidators.cs`

**Qué es:** Reglas que se evalúan antes de que el dato llegue al servicio.
Si el dato no cumple las reglas, la API devuelve `400 Bad Request` con los errores
detallados, sin siquiera tocar la base de datos.

**Por qué en Application y no en el Endpoint:** Las reglas de negocio (largo máximo,
formato de correo, campo requerido) pertenecen a la lógica de la aplicación,
no al transporte HTTP. Así se pueden reutilizar si en el futuro agregas otra entrada
(por ejemplo, una importación por archivo).

**Por qué `FluentValidation` y no `DataAnnotations`:** FluentValidation permite
reglas condicionales (`When`), mensajes personalizados y encadenamiento de reglas,
todo en código limpio sin llenar los DTOs de atributos.

```csharp
using FluentValidation;
using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Validators;

public sealed class CreateClienteValidator : AbstractValidator<CreateClienteDto>
{
    public CreateClienteValidator()
    {
        RuleFor(x => x.CliNombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.CliApellido)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede superar 100 caracteres.");

        RuleFor(x => x.CliCorreo)
            .MaximumLength(150).WithMessage("El correo no puede superar 150 caracteres.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .When(x => !string.IsNullOrEmpty(x.CliCorreo)); // solo valida si viene un valor

        RuleFor(x => x.CliCelular)
            .MaximumLength(20).WithMessage("El celular no puede superar 20 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CliCelular));

        RuleFor(x => x.CliDireccion)
            .MaximumLength(200).WithMessage("La dirección no puede superar 200 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CliDireccion));
    }
}

public sealed class UpdateClienteValidator : AbstractValidator<UpdateClienteDto>
{
    public UpdateClienteValidator()
    {
        RuleFor(x => x.CliNombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.CliApellido)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede superar 100 caracteres.");

        RuleFor(x => x.CliCorreo)
            .MaximumLength(150).WithMessage("El correo no puede superar 150 caracteres.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .When(x => !string.IsNullOrEmpty(x.CliCorreo));

        RuleFor(x => x.CliCelular)
            .MaximumLength(20).WithMessage("El celular no puede superar 20 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CliCelular));

        RuleFor(x => x.CliDireccion)
            .MaximumLength(200).WithMessage("La dirección no puede superar 200 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CliDireccion));
    }
}
```

> Hay dos validadores separados (Create y Update) porque en el futuro podrían
> tener reglas distintas. Por ahora son iguales, pero la separación cuesta poco y da flexibilidad.

---

## PASO A-6 — Servicio (lógica de negocio)

**CREAR:** `Application/Services/ClienteService.cs`

**Qué es:** La implementación concreta de `IClienteService`. Aquí vive la lógica:
lanza excepciones si el cliente no existe, convierte entidades a DTOs, orquesta
las llamadas al repositorio.

**Por qué separar servicio de repositorio:** El repositorio solo sabe de base de datos.
El servicio sabe de reglas de negocio. Si mañana crear un cliente requiere también
enviar un correo de bienvenida, ese código va en el servicio, no en el repositorio.

**`NotFoundException`:** Ya existe en el proyecto (la usan Bodega y Producto).
Cuando el middleware global de errores la captura, devuelve `404 Not Found` automáticamente.
No necesitas crear nada nuevo.

```csharp
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;
using ProductCatalog.Api.Domain.Exceptions;

namespace ProductCatalog.Api.Application.Services;

public sealed class ClienteService(IClienteRepository repo) : IClienteService
{
    public async Task<IEnumerable<ClienteDto>> GetAllAsync(CancellationToken ct = default)
    {
        var clientes = await repo.GetAllAsync(ct);
        return clientes.Select(ToDto); // convierte cada entidad a DTO
    }

    public async Task<ClienteDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var cliente = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cliente), id); // lanza 404 si no existe
        return ToDto(cliente);
    }

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto, CancellationToken ct = default)
    {
        var cliente = new Cliente // construye la entidad desde el DTO
        {
            CliNombre    = dto.CliNombre,
            CliApellido  = dto.CliApellido,
            CliCorreo    = dto.CliCorreo,
            CliCelular   = dto.CliCelular,
            CliDireccion = dto.CliDireccion
        };
        var creado = await repo.CreateAsync(cliente, ct);
        return ToDto(creado); // devuelve el DTO con el ID que asignó la BD
    }

    public async Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto, CancellationToken ct = default)
    {
        var cliente = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cliente), id);

        // actualiza los campos sobre la entidad existente
        cliente.CliNombre    = dto.CliNombre;
        cliente.CliApellido  = dto.CliApellido;
        cliente.CliCorreo    = dto.CliCorreo;
        cliente.CliCelular   = dto.CliCelular;
        cliente.CliDireccion = dto.CliDireccion;

        var actualizado = await repo.UpdateAsync(cliente, ct);
        return ToDto(actualizado);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _ = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cliente), id); // verifica que existe antes de borrar
        await repo.DeleteAsync(id, ct);
    }

    // método privado de conversión: entidad → DTO
    private static ClienteDto ToDto(Cliente c) =>
        new(c.CliId, c.CliNombre, c.CliApellido, c.CliCorreo, c.CliCelular, c.CliDireccion);
}
```

---

## PASO A-7 — Repositorio (acceso a la BD)

**CREAR:** `Infrastructure/Repositories/ClienteRepository.cs`

**Qué es:** La implementación concreta de `IClienteRepository`. Es la única
clase del proyecto que sabe cómo hablar con la base de datos para la tabla `clientes`.
Usa Entity Framework Core (EF Core) para traducir el código C# a SQL automáticamente.

**Por qué en Infrastructure:** Infrastructure es la capa que depende de tecnología
externa (Postgres, Redis, correo, etc.). Si mañana cambias de EF Core a Dapper,
solo tocas esta carpeta.

```csharp
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure.Repositories;

public sealed class ClienteRepository(AppDbContext db) : IClienteRepository
{
    // SELECT * FROM clientes ORDER BY cli_apellido, cli_nombre
    public async Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default) =>
        await db.Clientes.OrderBy(x => x.CliApellido).ThenBy(x => x.CliNombre).ToListAsync(ct);

    // SELECT * FROM clientes WHERE cli_id = @id  (devuelve null si no existe)
    public async Task<Cliente?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Clientes.FindAsync([id], ct);

    // INSERT INTO clientes (...) VALUES (...)
    public async Task<Cliente> CreateAsync(Cliente cliente, CancellationToken ct = default)
    {
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct); // aquí EF ejecuta el SQL y llena el CliId generado
        return cliente;
    }

    // UPDATE clientes SET ... WHERE cli_id = @id
    public async Task<Cliente> UpdateAsync(Cliente cliente, CancellationToken ct = default)
    {
        db.Clientes.Update(cliente);
        await db.SaveChangesAsync(ct);
        return cliente;
    }

    // DELETE FROM clientes WHERE cli_id = @id
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var cliente = await db.Clientes.FindAsync([id], ct);
        if (cliente is not null)
        {
            db.Clientes.Remove(cliente);
            await db.SaveChangesAsync(ct);
        }
    }
}
```

---

## PASO A-8 — AppDbContext

**MODIFICAR:** `Infrastructure/AppDbContext.cs`

**Qué es:** El `AppDbContext` es el "mapa" entre las clases C# y las tablas de la BD.
Cada `DbSet<T>` le dice a EF Core: "existe una tabla para este tipo de objeto".
La configuración en `OnModelCreating` le dice cómo mapear cada propiedad a su columna.

**Por qué hay que tocarlo:** EF Core no descubre solo la entidad `Cliente`.
Hay que registrarla explícitamente para que pueda hacer queries y migraciones sobre ella.

Agrega `DbSet` después de la línea de `Movimientos`:
```csharp
public DbSet<Cliente> Clientes => Set<Cliente>();
```

Agrega la configuración dentro de `OnModelCreating`, antes del cierre `}`:
```csharp
model.Entity<Cliente>(e =>
{
    e.ToTable("clientes");                                                        // nombre de la tabla en Postgres
    e.HasKey(x => x.CliId);                                                       // clave primaria
    e.Property(x => x.CliId).HasColumnName("cli_id").UseIdentityColumn();         // autoincremental
    e.Property(x => x.CliNombre).HasColumnName("cli_nombre").HasMaxLength(100).IsRequired();
    e.Property(x => x.CliApellido).HasColumnName("cli_apellido").HasMaxLength(100).IsRequired();
    e.Property(x => x.CliCorreo).HasColumnName("cli_correo").HasMaxLength(150);   // nullable
    e.Property(x => x.CliCelular).HasColumnName("cli_celular").HasMaxLength(20);  // nullable
    e.Property(x => x.CliDireccion).HasColumnName("cli_direccion").HasMaxLength(200); // nullable
});
```

> `UseIdentityColumn()` le dice a Postgres que use `SERIAL` / `GENERATED ALWAYS AS IDENTITY`
> para el ID. El valor se genera automáticamente al insertar.

---

## PASO A-9 — Migración

**Qué es:** Una migración es un archivo C# que EF Core genera automáticamente
comparando tu código con el estado actual de la BD. Contiene las instrucciones SQL
(`CREATE TABLE`, `ALTER TABLE`, etc.) para llevar la BD al estado del código.

**Por qué no escribir el SQL a mano:** EF Core rastrea el historial de migraciones.
Si en el futuro agregas una columna, genera solo el `ALTER TABLE` necesario,
sin borrar lo que ya existe.

```bash
# Genera el archivo de migración en la carpeta Migrations/
dotnet ef migrations add AddClientes

# Aplica la migración a tu BD local
dotnet ef database update
```

> Abre el archivo generado en `Migrations/` y confirma que tiene `CREATE TABLE clientes`
> con las columnas correctas antes de aplicarlo.
>
> En producción (Railway) no necesitas correr `database update` manualmente —
> `MigrateAsync()` en `Program.cs` lo hace automáticamente al arrancar el servidor.

---

## PASO A-10 — Endpoints HTTP

**CREAR:** `Endpoints/ClienteEndpoints.cs`

**Qué es:** La capa HTTP del módulo. Recibe la petición, llama al validador,
llama al servicio y devuelve la respuesta. No contiene lógica de negocio.

**Por qué Minimal APIs:** El proyecto usa Minimal APIs de .NET 8 (no Controllers).
Son más ligeras y directas. Cada endpoint es una función lambda registrada en una ruta.

**`WithTags("Clientes")`:** Le dice a Swagger que agrupe estos endpoints bajo
la etiqueta "Clientes" en la UI.

**`Produces<T>(statusCode)`:** Le dice a Swagger qué tipo de objeto y qué código
HTTP devuelve cada respuesta. Sin esto, Swagger no muestra el schema del response.

```csharp
using FluentValidation;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;

namespace ProductCatalog.Api.Endpoints;

public static class ClienteEndpoints
{
    public static void MapClienteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/clientes").WithTags("Clientes");

        group.MapGet("/", async (IClienteService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)))
        .WithName("ListarClientes")
        .WithSummary("Listar todos los clientes")
        .WithDescription("Devuelve todos los clientes ordenados por apellido y nombre.")
        .Produces<IEnumerable<ClienteDto>>(200);

        group.MapGet("/{id:int}", async (int id, IClienteService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser mayor a cero." });
            var cliente = await service.GetByIdAsync(id, ct);
            return Results.Ok(cliente);
        })
        .WithName("ObtenerCliente")
        .WithSummary("Obtener cliente por ID")
        .WithDescription("Errores: `400` si id ≤ 0 · `404` si el cliente no existe.")
        .Produces<ClienteDto>(200)
        .Produces<object>(400)
        .Produces<object>(404);

        group.MapPost("/", async (
            CreateClienteDto dto,
            IValidator<CreateClienteDto> validator,
            IClienteService service,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary()); // 400 con detalle de errores

            var creado = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/clientes/{creado.CliId}", creado); // 201 + Location header
        })
        .WithName("CrearCliente")
        .WithSummary("Crear un nuevo cliente")
        .WithDescription("`cliNombre` y `cliApellido` son requeridos. El correo se valida si se proporciona.")
        .Produces<ClienteDto>(201)
        .Produces<object>(400);

        group.MapPut("/{id:int}", async (
            int id,
            UpdateClienteDto dto,
            IValidator<UpdateClienteDto> validator,
            IClienteService service,
            CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser mayor a cero." });
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var actualizado = await service.UpdateAsync(id, dto, ct);
            return Results.Ok(actualizado);
        })
        .WithName("ActualizarCliente")
        .WithSummary("Actualizar un cliente existente")
        .WithDescription("Errores: `400` validación · `404` cliente no existe.")
        .Produces<ClienteDto>(200)
        .Produces<object>(400)
        .Produces<object>(404);

        group.MapDelete("/{id:int}", async (int id, IClienteService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser mayor a cero." });
            await service.DeleteAsync(id, ct);
            return Results.NoContent(); // 204: éxito sin cuerpo de respuesta
        })
        .WithName("EliminarCliente")
        .WithSummary("Eliminar un cliente")
        .WithDescription("Errores: `400` si id ≤ 0 · `404` si el cliente no existe.")
        .Produces(204)
        .Produces<object>(400)
        .Produces<object>(404);
    }
}
```

---

## PASO A-11 — Program.cs

**MODIFICAR:** `Program.cs`

**Qué es:** El punto de arranque de la aplicación. Aquí se "conectan" todas las
piezas: cuando alguien pida `IClienteService`, el sistema sabe que debe darle
`ClienteService`. Esto se llama **inyección de dependencias** (DI).

**Por qué `AddScoped`:** Significa que se crea una instancia del servicio/repositorio
por cada petición HTTP y se destruye al terminar. Es el ciclo de vida correcto
para operaciones de base de datos.

Después de las líneas de Bodega, agrega:
```csharp
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
```

Donde están los `app.Map...Endpoints()`, agrega:
```csharp
app.MapClienteEndpoints();
```

> Sin estas líneas el proyecto compila pero al llamar `/api/clientes` falla en runtime
> porque el sistema no sabe qué clase usar cuando alguien pide `IClienteService`.

---

## PASO A-12 — Verificar

```bash
dotnet build   # debe mostrar: 0 Errores
dotnet run     # abre http://localhost:5080/swagger → grupo "Clientes" con 5 endpoints
```

Prueba en este orden (cada paso depende del anterior):
1. `POST /api/clientes` — crea un cliente, copia el `cliId` de la respuesta
2. `GET /api/clientes` — confirma que aparece en la lista
3. `GET /api/clientes/{id}` — obtén el que creaste
4. `PUT /api/clientes/{id}` — edita algún campo
5. `DELETE /api/clientes/{id}` — elimínalo y confirma que el `GET` siguiente da 404

---

## PASO A-13 — Frontend (opcional)

**Qué es:** Las vistas HTML que consumen la API desde el navegador.
Son complementarias — la evaluación principal es el backend/Swagger,
pero el frontend muestra que el sistema funciona de extremo a extremo.

**Estrategia:** Copia los archivos de Bodegas (que ya funcionan) y adapta
los campos. Es más rápido que empezar desde cero y garantiza consistencia visual.

Crea `wwwroot/clientes.html` copiando `wwwroot/bodegas.html` y cambia:
- Título, encabezado, clase `active` del nav
- Columnas de la tabla: `#`, `Nombre`, `Apellido`, `Correo`, `Celular`, `Acciones`
- Modal: campos `cliNombre`, `cliApellido`, `cliCorreo`, `cliCelular`, `cliDireccion`
- Script al final: `<script src="/js/clientes.js"></script>`

Crea `wwwroot/js/clientes.js` copiando `wwwroot/js/bodegas.js` y adapta la URL y los campos.

Agrega el link en el nav de **todos** los HTML existentes:
```html
<li class="nav-item">
  <a class="nav-link" href="/clientes.html">
    <i class="bi bi-people me-1"></i>Clientes
  </a>
</li>
```

---

## Resumen Sección A

| # | Acción | Archivo | Qué hace |
|---|---|---|---|
| A-1 | CREAR | `Domain/Entities/Cliente.cs` | Molde del objeto |
| A-2 | CREAR | `Application/DTOs/ClienteDtos.cs` | Entrada/salida de la API |
| A-3 | CREAR | `Application/Interfaces/IClienteRepository.cs` | Contrato de BD |
| A-4 | CREAR | `Application/Interfaces/IClienteService.cs` | Contrato de lógica |
| A-5 | CREAR | `Application/Validators/ClienteValidators.cs` | Reglas de validación |
| A-6 | CREAR | `Application/Services/ClienteService.cs` | Lógica de negocio |
| A-7 | CREAR | `Infrastructure/Repositories/ClienteRepository.cs` | SQL real con EF Core |
| A-8 | MODIFICAR | `Infrastructure/AppDbContext.cs` | Registrar tabla en EF |
| A-9 | TERMINAL | `dotnet ef migrations add AddClientes` + `dotnet ef database update` | Crear tabla en BD |
| A-10 | CREAR | `Endpoints/ClienteEndpoints.cs` | Rutas HTTP |
| A-11 | MODIFICAR | `Program.cs` | Conectar las piezas (DI) |
| A-12 | TERMINAL | `dotnet build` + `dotnet run` | Verificar en Swagger |
| A-13 | CREAR | `wwwroot/clientes.html` + `wwwroot/js/clientes.js` + navs | Frontend opcional |

---

---

# SECCIÓN B — Nueva instancia por empresa (infraestructura)

## Por qué este flujo existe

Cada empresa que use el sistema necesita sus **propios datos** — los productos de
Bancolombia no deben aparecer en el sistema de Empresa ABC. La solución más simple
es darle a cada empresa:

- **Su propia rama de Git** → permite personalizar el código si lo necesita
- **Su propia BD en Supabase** → datos 100% separados, sin riesgo de mezcla
- **Su propio deploy en Railway** → su propia URL pública

Es más sencillo que un sistema multi-tenant (donde todos comparten BD con filtros),
y mucho más seguro para datos de clientes reales.

---

## PASO B-1 — Crear la rama de la empresa

**Qué hace:** Crea una copia del código de `main` en una rama separada.
Los cambios que hagas en esa rama no afectan a `main` ni a otras empresas.

```bash
git checkout main
git pull origin main
git checkout -b cliente-nombre-empresa   # crea la rama
git push origin cliente-nombre-empresa   # la sube a GitHub
```

> Reemplaza `nombre-empresa` con el nombre real. Ejemplo: `cliente-bancolombia`

---

## PASO B-2 — Nueva base de datos en Supabase

**Qué hace:** Crea una base de datos PostgreSQL vacía en la nube.
Las tablas se crean solas al primera vez que la API arranca (gracias a `MigrateAsync`).

1. Entra a https://supabase.com → **New project**
2. Nombre: `inventario-nombre-empresa`
3. Contraseña segura (guárdala)
4. Región: `South America (São Paulo)`
5. Espera ~2 minutos

**Obtener la connection string:**
- Menú lateral **Settings** → **Database** → sección **Connection string** → pestaña **URI**
- Copia la cadena y reemplaza `[TU-PASSWORD]`:
  ```
  postgresql://postgres:[TU-PASSWORD]@db.xxxxxxxxxxxx.supabase.co:5432/postgres
  ```

---

## PASO B-3 — Nuevo deploy en Railway

**Qué hace:** Railway lee el `Dockerfile` del repositorio, construye la imagen
y corre el servidor. La variable de entorno le dice a la API a qué BD conectarse.

1. https://railway.app → **New project** → **Deploy from GitHub repo**
2. Repositorio: `prueba-fundacion-mujer`
3. Branch: `cliente-nombre-empresa`
4. Railway detecta el Dockerfile → **Deploy**

**Agregar la variable de entorno** (pestaña **Variables**):
```
ConnectionStrings__DefaultConnection = postgresql://postgres:[PASSWORD]@db.xxx.supabase.co:5432/postgres
```

**Obtener la URL pública** (pestaña **Settings** → **Domains** → **Generate Domain**):
```
https://inventario-cliente-abc.up.railway.app
```

> Railway reinicia automáticamente cuando guardas la variable.
> En el primer arranque `MigrateAsync` crea todas las tablas en la BD nueva.

---

## PASO B-4 — Verificar

| Qué probar | URL |
|---|---|
| Página principal | `https://tu-url.up.railway.app` |
| Swagger | `https://tu-url.up.railway.app/swagger` |
| API | `https://tu-url.up.railway.app/api/bodegas` |

Si Swagger carga con los endpoints → todo funciona.
La BD ya tiene datos iniciales del seeder automático (1 bodega + 5 productos de ejemplo).

---

## PASO B-5 — Propagar cambios de main a la empresa

**Cuándo usarlo:** Cuando mejoras el código base en `main` (nuevo módulo, bug fix)
y quieres que la empresa también reciba ese cambio.

```bash
git checkout cliente-nombre-empresa
git merge main                         # trae los cambios de main
git push origin cliente-nombre-empresa # Railway hace redeploy en ~2 min
```

---

## Resumen visual

```
GitHub
├── main                    ← código base (siempre actualizado)
├── cliente-bancolombia     ← copia para Bancolombia
└── cliente-empresa-abc     ← copia para Empresa ABC

Supabase
├── proyecto original       ← BD del deploy principal
├── proyecto bancolombia    ← BD de Bancolombia (datos separados)
└── proyecto empresa-abc    ← BD de Empresa ABC (datos separados)

Railway
├── Deploy → main           → BD original
├── Deploy → bancolombia    → BD bancolombia  →  URL propia
└── Deploy → empresa-abc    → BD empresa-abc  →  URL propia
```

Cada deploy tiene su propia URL y sus propios datos. Ninguno interfiere con el otro.
