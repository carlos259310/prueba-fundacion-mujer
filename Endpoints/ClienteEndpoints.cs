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
