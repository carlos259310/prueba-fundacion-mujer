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
        .WithSummary("Listar todas las bodegas")
        .WithDescription("Devuelve todas las bodegas registradas en el sistema.")
        .Produces<IEnumerable<BodegaDto>>(200);

        group.MapGet("/{id:int}", async (int id, IBodegaService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            var bodega = await service.GetByIdAsync(id, ct);
            return Results.Ok(bodega);
        })
        .WithName("ObtenerBodega")
        .WithSummary("Obtener bodega por ID")
        .WithDescription("Errores: `400` si id ≤ 0 · `404` si la bodega no existe.")
        .Produces<BodegaDto>(200)
        .Produces<object>(400)
        .Produces<object>(404);

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
        .WithSummary("Crear una nueva bodega")
        .WithDescription("`bodNombre` requerido (máx. 100 caracteres). `bodPrincipal` indica si es la bodega principal del negocio.")
        .Produces<BodegaDto>(201)
        .Produces<object>(400);

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
        .WithSummary("Actualizar una bodega existente")
        .WithDescription("Errores: `400` validación de campos · `404` bodega no existe.")
        .Produces<BodegaDto>(200)
        .Produces<object>(400)
        .Produces<object>(404);

        group.MapDelete("/{id:int}", async (int id, IBodegaService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("EliminarBodega")
        .WithSummary("Eliminar una bodega")
        .WithDescription("Errores: `400` si id ≤ 0 · `404` si la bodega no existe.")
        .Produces(204)
        .Produces<object>(400)
        .Produces<object>(404);
    }
}
