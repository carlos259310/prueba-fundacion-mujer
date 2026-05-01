# Guía: Agregar el módulo de Clientes al proyecto

Sigue estos pasos en orden. Cada uno te explica qué hace y por qué,
para que entiendas cómo fluye el proyecto.

---

## Arquitectura que vas a seguir

```
Domain/Entities/          ← define qué ES un Cliente (solo propiedades)
Application/DTOs/         ← define qué datos entran y salen por la API
Application/Interfaces/   ← define QUÉ se puede hacer (contrato)
Application/Validators/   ← define las reglas de validación
Application/Services/     ← implementa la lógica de negocio
Infrastructure/Repositories/ ← habla con la base de datos
Infrastructure/AppDbContext  ← registra la tabla en EF Core
Endpoints/                ← expone los endpoints HTTP
Program.cs                ← registra todo en el contenedor de dependencias
wwwroot/                  ← frontend (HTML + JS)
```

Cada módulo nuevo sigue exactamente este mismo flujo. Una vez lo hagas
con Clientes, sabrás hacerlo con cualquier otra entidad.

---

## PASO 1 — Entidad de dominio

**Archivo a CREAR:** `Domain/Entities/Cliente.cs`

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

> La entidad es la representación pura del objeto. Sin lógica, sin anotaciones de API.
> Los campos con `?` son opcionales (pueden ser null en la BD).

---

## PASO 2 — DTOs (lo que entra y sale por la API)

**Archivo a CREAR:** `Application/DTOs/ClienteDtos.cs`

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

> `ClienteDto` es lo que devuelve la API (incluye el ID).
> `CreateClienteDto` es lo que recibe al crear (sin ID, lo genera la BD).
> `UpdateClienteDto` es lo que recibe al actualizar.

---

## PASO 3 — Interfaz del repositorio (contrato de BD)

**Archivo a CREAR:** `Application/Interfaces/IClienteRepository.cs`

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

> La interfaz define QUÉ operaciones existen, sin decir CÓMO se hacen.
> El servicio solo conoce esta interfaz, nunca la implementación concreta.

---

## PASO 4 — Interfaz del servicio (contrato de lógica)

**Archivo a CREAR:** `Application/Interfaces/IClienteService.cs`

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

---

## PASO 5 — Validadores

**Archivo a CREAR:** `Application/Validators/ClienteValidators.cs`

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
            .When(x => !string.IsNullOrEmpty(x.CliCorreo));

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

---

## PASO 6 — Servicio (lógica de negocio)

**Archivo a CREAR:** `Application/Services/ClienteService.cs`

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
        return clientes.Select(ToDto);
    }

    public async Task<ClienteDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var cliente = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cliente), id);
        return ToDto(cliente);
    }

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto, CancellationToken ct = default)
    {
        var cliente = new Cliente
        {
            CliNombre    = dto.CliNombre,
            CliApellido  = dto.CliApellido,
            CliCorreo    = dto.CliCorreo,
            CliCelular   = dto.CliCelular,
            CliDireccion = dto.CliDireccion
        };

        var creado = await repo.CreateAsync(cliente, ct);
        return ToDto(creado);
    }

    public async Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto, CancellationToken ct = default)
    {
        var cliente = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cliente), id);

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
            ?? throw new NotFoundException(nameof(Cliente), id);
        await repo.DeleteAsync(id, ct);
    }

    private static ClienteDto ToDto(Cliente c) =>
        new(c.CliId, c.CliNombre, c.CliApellido, c.CliCorreo, c.CliCelular, c.CliDireccion);
}
```

---

## PASO 7 — Repositorio (acceso a la BD)

**Archivo a CREAR:** `Infrastructure/Repositories/ClienteRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure.Repositories;

public sealed class ClienteRepository(AppDbContext db) : IClienteRepository
{
    public async Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default) =>
        await db.Clientes.OrderBy(x => x.CliApellido).ThenBy(x => x.CliNombre).ToListAsync(ct);

    public async Task<Cliente?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Clientes.FindAsync([id], ct);

    public async Task<Cliente> CreateAsync(Cliente cliente, CancellationToken ct = default)
    {
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        return cliente;
    }

    public async Task<Cliente> UpdateAsync(Cliente cliente, CancellationToken ct = default)
    {
        db.Clientes.Update(cliente);
        await db.SaveChangesAsync(ct);
        return cliente;
    }

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

## PASO 8 — Registrar en AppDbContext y configurar la tabla

**Archivo a MODIFICAR:** `Infrastructure/AppDbContext.cs`

Agrega `DbSet` después de la línea de `Movimientos`:
```csharp
public DbSet<Cliente> Clientes => Set<Cliente>();
```

Agrega la configuración de la tabla dentro de `OnModelCreating`, al final antes del cierre `}`:
```csharp
model.Entity<Cliente>(e =>
{
    e.ToTable("clientes");
    e.HasKey(x => x.CliId);
    e.Property(x => x.CliId).HasColumnName("cli_id").UseIdentityColumn();
    e.Property(x => x.CliNombre).HasColumnName("cli_nombre").HasMaxLength(100).IsRequired();
    e.Property(x => x.CliApellido).HasColumnName("cli_apellido").HasMaxLength(100).IsRequired();
    e.Property(x => x.CliCorreo).HasColumnName("cli_correo").HasMaxLength(150);
    e.Property(x => x.CliCelular).HasColumnName("cli_celular").HasMaxLength(20);
    e.Property(x => x.CliDireccion).HasColumnName("cli_direccion").HasMaxLength(200);
});
```

---

## PASO 9 — Crear la migración (genera la tabla en la BD)

Abre la terminal en la carpeta del proyecto y corre:

```bash
dotnet ef migrations add AddClientes
```

Esto crea un archivo nuevo en la carpeta `Migrations/`. Revísalo para confirmar
que tiene la tabla `clientes` con las columnas correctas.

Luego aplica la migración a la base de datos:

```bash
dotnet ef database update
```

> En producción (Railway) no necesitas correr esto manualmente —
> `MigrateAsync()` en `Program.cs` lo hace automáticamente al arrancar.

---

## PASO 10 — Endpoints HTTP

**Archivo a CREAR:** `Endpoints/ClienteEndpoints.cs`

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
                return Results.ValidationProblem(validation.ToDictionary());

            var creado = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/clientes/{creado.CliId}", creado);
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
            return Results.NoContent();
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

## PASO 11 — Registrar en Program.cs

**Archivo a MODIFICAR:** `Program.cs`

Busca el bloque donde están registradas las dependencias de Bodega y agrega las de Cliente justo después:

```csharp
// Después de las líneas de IBodegaRepository / IBodegaService
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
```

Busca donde están los `app.Map...Endpoints()` y agrega:

```csharp
app.MapClienteEndpoints();
```

---

## PASO 12 — Verificar que compila

```bash
dotnet build
```

Debe mostrar `0 Errores`. Si hay errores, revisa que los `using` estén correctos en cada archivo.

---

## PASO 13 — Probar en Swagger

```bash
dotnet run
```

Abre `http://localhost:5080/swagger` — debes ver el grupo **Clientes** con 5 endpoints.

Prueba en orden:
1. `POST /api/clientes` — crea un cliente
2. `GET /api/clientes` — lista todos
3. `GET /api/clientes/{id}` — obtén el que creaste
4. `PUT /api/clientes/{id}` — edítalo
5. `DELETE /api/clientes/{id}` — elimínalo

---

## PASO 14 — Frontend (HTML + JS)

Crea `wwwroot/clientes.html` copiando la estructura de `wwwroot/bodegas.html`
y cambia:
- El título, encabezado y clase `active` del nav
- Las columnas de la tabla: `#`, `Nombre`, `Apellido`, `Correo`, `Celular`, `Acciones`
- El modal: campos `cliNombre`, `cliApellido`, `cliCorreo`, `cliCelular`, `cliDireccion`
- El script al final: `<script src="/js/clientes.js"></script>`

Crea `wwwroot/js/clientes.js` copiando `wwwroot/js/bodegas.js` y adapta:
- La URL del fetch: `/api/clientes`
- Los campos del modal
- El mapeo de la tabla

Agrega el link en el nav de **todos** los HTML existentes:
```html
<li class="nav-item">
  <a class="nav-link" href="/clientes.html">
    <i class="bi bi-people me-1"></i>Clientes
  </a>
</li>
```

---

## Resumen de archivos

| # | Acción | Archivo |
|---|---|---|
| 1 | CREAR | `Domain/Entities/Cliente.cs` |
| 2 | CREAR | `Application/DTOs/ClienteDtos.cs` |
| 3 | CREAR | `Application/Interfaces/IClienteRepository.cs` |
| 4 | CREAR | `Application/Interfaces/IClienteService.cs` |
| 5 | CREAR | `Application/Validators/ClienteValidators.cs` |
| 6 | CREAR | `Application/Services/ClienteService.cs` |
| 7 | CREAR | `Infrastructure/Repositories/ClienteRepository.cs` |
| 8 | MODIFICAR | `Infrastructure/AppDbContext.cs` |
| 9 | TERMINAL | `dotnet ef migrations add AddClientes` |
| 10 | CREAR | `Endpoints/ClienteEndpoints.cs` |
| 11 | MODIFICAR | `Program.cs` |
| 12 | TERMINAL | `dotnet build` → 0 errores |
| 13 | TERMINAL | `dotnet run` → probar en Swagger |
| 14 | CREAR | `wwwroot/clientes.html` + `wwwroot/js/clientes.js` |
| 15 | MODIFICAR | Todos los `.html` → agregar link en el nav |
