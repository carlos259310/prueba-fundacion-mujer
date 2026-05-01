# Guía de Clientes — dos temas en uno

Este archivo cubre dos cosas distintas que comparten la palabra "cliente":

| Sección | Qué es |
|---|---|
| **A — Módulo de Clientes** | Agregar el CRUD de personas (clientes del negocio) al código |
| **B — Nueva instancia por empresa** | Desplegar una copia del sistema para otra empresa |

Empieza por la sección que necesitas ahora.

---

# SECCIÓN A — Módulo de Clientes (código)

Agrega al proyecto la entidad `Cliente` con su CRUD completo siguiendo
la misma arquitectura que ya tiene el proyecto (Domain → Application → Infrastructure → Endpoints).

## Arquitectura

```
Domain/Entities/             ← qué ES un Cliente
Application/DTOs/            ← qué entra y sale por la API
Application/Interfaces/      ← contrato (qué se puede hacer)
Application/Validators/      ← reglas de validación
Application/Services/        ← lógica de negocio
Infrastructure/Repositories/ ← acceso a la base de datos
Infrastructure/AppDbContext   ← registrar la tabla en EF Core
Endpoints/                   ← endpoints HTTP
Program.cs                   ← registrar todo en DI
wwwroot/                     ← frontend HTML + JS
```

---

## PASO A-1 — Entidad de dominio

**CREAR:** `Domain/Entities/Cliente.cs`

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

> Los campos con `?` son opcionales (pueden ser null en la BD).

---

## PASO A-2 — DTOs

**CREAR:** `Application/DTOs/ClienteDtos.cs`

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

> `ClienteDto` → lo que devuelve la API (incluye el ID).
> `CreateClienteDto` → lo que recibe al crear (sin ID, lo genera la BD).
> `UpdateClienteDto` → lo que recibe al actualizar.

---

## PASO A-3 — Interfaz del repositorio

**CREAR:** `Application/Interfaces/IClienteRepository.cs`

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

---

## PASO A-4 — Interfaz del servicio

**CREAR:** `Application/Interfaces/IClienteService.cs`

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

## PASO A-5 — Validadores

**CREAR:** `Application/Validators/ClienteValidators.cs`

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

## PASO A-6 — Servicio

**CREAR:** `Application/Services/ClienteService.cs`

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

## PASO A-7 — Repositorio

**CREAR:** `Infrastructure/Repositories/ClienteRepository.cs`

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

## PASO A-8 — AppDbContext

**MODIFICAR:** `Infrastructure/AppDbContext.cs`

Agrega `DbSet` después de la línea de `Movimientos`:
```csharp
public DbSet<Cliente> Clientes => Set<Cliente>();
```

Agrega la configuración dentro de `OnModelCreating`, antes del cierre `}`:
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

## PASO A-9 — Migración

```bash
dotnet ef migrations add AddClientes
dotnet ef database update
```

> En Railway no necesitas correr `database update` — `MigrateAsync()` en `Program.cs` lo hace al arrancar.

---

## PASO A-10 — Endpoints

**CREAR:** `Endpoints/ClienteEndpoints.cs`

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

## PASO A-11 — Program.cs

**MODIFICAR:** `Program.cs`

Después de las líneas de Bodega, agrega:
```csharp
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
```

Donde están los `app.Map...Endpoints()`, agrega:
```csharp
app.MapClienteEndpoints();
```

---

## PASO A-12 — Verificar

```bash
dotnet build   # → 0 errores
dotnet run     # → abre http://localhost:5080/swagger → grupo Clientes con 5 endpoints
```

Prueba en orden: `POST` → `GET /` → `GET /{id}` → `PUT` → `DELETE`.

---

## PASO A-13 — Frontend (opcional)

Crea `wwwroot/clientes.html` copiando `wwwroot/bodegas.html` y cambia:
- Título, encabezado, clase `active` del nav
- Columnas: `#`, `Nombre`, `Apellido`, `Correo`, `Celular`, `Acciones`
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

| # | Acción | Archivo |
|---|---|---|
| A-1 | CREAR | `Domain/Entities/Cliente.cs` |
| A-2 | CREAR | `Application/DTOs/ClienteDtos.cs` |
| A-3 | CREAR | `Application/Interfaces/IClienteRepository.cs` |
| A-4 | CREAR | `Application/Interfaces/IClienteService.cs` |
| A-5 | CREAR | `Application/Validators/ClienteValidators.cs` |
| A-6 | CREAR | `Application/Services/ClienteService.cs` |
| A-7 | CREAR | `Infrastructure/Repositories/ClienteRepository.cs` |
| A-8 | MODIFICAR | `Infrastructure/AppDbContext.cs` |
| A-9 | TERMINAL | `dotnet ef migrations add AddClientes` + `dotnet ef database update` |
| A-10 | CREAR | `Endpoints/ClienteEndpoints.cs` |
| A-11 | MODIFICAR | `Program.cs` |
| A-12 | TERMINAL | `dotnet build` → `dotnet run` → probar Swagger |
| A-13 | CREAR | `wwwroot/clientes.html` + `wwwroot/js/clientes.js` + nav en todos los HTML |

---

---

# SECCIÓN B — Nueva instancia por empresa (infraestructura)

Cuando quieras desplegar el sistema para **otra empresa**, cada una tiene
su propia rama Git + su propia BD en Supabase + su propio deploy en Railway.
Sus datos nunca se mezclan.

## PASO B-1 — Crear la rama de la empresa

```bash
git checkout main
git pull origin main
git checkout -b cliente-nombre-empresa
git push origin cliente-nombre-empresa
```

> Reemplaza `nombre-empresa` con el nombre real. Ejemplo: `cliente-bancolombia`

---

## PASO B-2 — Nueva base de datos en Supabase

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

> Las tablas se crean automáticamente al iniciar la API.

---

## PASO B-3 — Nuevo deploy en Railway

1. https://railway.app → **New project** → **Deploy from GitHub repo**
2. Repositorio: `prueba-fundacion-mujer`
3. Branch: `cliente-nombre-empresa`
4. Railway detecta el Dockerfile → **Deploy**

**Agregar la variable de entorno:**
- Pestaña **Variables** → agrega:
  ```
  ConnectionStrings__DefaultConnection = postgresql://postgres:[PASSWORD]@db.xxx.supabase.co:5432/postgres
  ```

**Obtener la URL pública:**
- Pestaña **Settings** → **Domains** → **Generate Domain**

---

## PASO B-4 — Verificar

| Qué probar | URL |
|---|---|
| Página principal | `https://tu-url.up.railway.app` |
| Swagger | `https://tu-url.up.railway.app/swagger` |
| API | `https://tu-url.up.railway.app/api/bodegas` |

Si Swagger carga con los endpoints → todo funciona.
La BD ya tiene datos iniciales (1 bodega + 5 productos de ejemplo).

---

## PASO B-5 — Propagar cambios de main a la empresa

```bash
git checkout cliente-nombre-empresa
git merge main
git push origin cliente-nombre-empresa
```

Railway hace el redeploy automáticamente en ~2 minutos.

---

## Resumen visual

```
GitHub
├── main                    ← código base
├── cliente-bancolombia     ← copia para Bancolombia
└── cliente-empresa-abc     ← copia para Empresa ABC

Railway
├── Deploy → main           → BD Supabase original
├── Deploy → bancolombia    → BD Supabase bancolombia
└── Deploy → empresa-abc    → BD Supabase empresa-abc
```

Cada deploy tiene su propia URL y sus propios datos.
