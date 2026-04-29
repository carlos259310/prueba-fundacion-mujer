using FluentValidation;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;

namespace ProductCatalog.Api.Endpoints;

public static class BodegaEndpoints
{
    public static void MapBodegaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/bodegas").WithTags("Bodegas");

        group.MapGet("/", async (IBodegaService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(ct);
            return Results.Ok(result);
        })
        .WithName("ListarBodegas")
        .WithSummary("Listar todas las bodegas");

        group.MapGet("/{id:int}", async (int id, IBodegaService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            var bodega = await service.GetByIdAsync(id, ct);
            return Results.Ok(bodega);
        })
        .WithName("ObtenerBodega")
        .WithSummary("Obtener bodega por ID");

        group.MapPost("/", async (
            CreateBodegaDto dto,
            IValidator<CreateBodegaDto> validator,
            IBodegaService service,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var creada = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/bodegas/{creada.BodId}", creada);
        })
        .WithName("CrearBodega")
        .WithSummary("Crear bodega");

        group.MapPut("/{id:int}", async (
            int id,
            UpdateBodegaDto dto,
            IValidator<UpdateBodegaDto> validator,
            IBodegaService service,
            CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var actualizada = await service.UpdateAsync(id, dto, ct);
            return Results.Ok(actualizada);
        })
        .WithName("ActualizarBodega")
        .WithSummary("Actualizar bodega");

        group.MapDelete("/{id:int}", async (int id, IBodegaService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("EliminarBodega")
        .WithSummary("Eliminar bodega");
    }
}
