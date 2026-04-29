using FluentValidation;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;

namespace ProductCatalog.Api.Endpoints;

public static class MovimientoEndpoints
{
    public static void MapMovimientoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/movimientos").WithTags("Movimientos");

        group.MapGet("/{prodId:int}", async (
            int prodId,
            IMovimientoService service,
            int page = 1,
            int pageSize = 10,
            CancellationToken ct = default) =>
        {
            if (prodId <= 0) return Results.BadRequest(new { error = "El ID del producto debe ser mayor a cero." });
            var result = await service.GetByProductoAsync(prodId, page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithName("HistorialMovimientos")
        .WithSummary("Historial de movimientos de un producto (paginado)");

        group.MapPost("/", async (
            CreateMovimientoDto dto,
            IValidator<CreateMovimientoDto> validator,
            IMovimientoService service,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var creado = await service.RegistrarAsync(dto, ct);
            return Results.Created($"/api/movimientos/{creado.ProdId}", creado);
        })
        .WithName("RegistrarMovimiento")
        .WithSummary("Registrar movimiento de inventario (Entrada, Salida o Traslado)");
    }
}
