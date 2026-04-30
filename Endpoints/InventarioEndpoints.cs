using FluentValidation;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;

namespace ProductCatalog.Api.Endpoints;

public static class InventarioEndpoints
{
    public static void MapInventarioEndpoints(this WebApplication app)
    {
        app.MapGet("/api/inventario", async (IInventarioService service, int? prodId, CancellationToken ct) =>
            Results.Ok(await service.GetAllStockAsync(prodId, ct)))
        .WithTags("Inventario")
        .WithName("ListarTodoStock")
        .WithSummary("Stock de todos los productos en todas las bodegas (prodId opcional para filtrar)");

        var group = app.MapGroup("/api/productos").WithTags("Inventario");

        group.MapGet("/{id:int}/stock", async (int id, IInventarioService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            var stock = await service.GetStockByProductoAsync(id, ct);
            return Results.Ok(stock);
        })
        .WithName("ObtenerStockProducto")
        .WithSummary("Ver stock del producto por bodega");

        group.MapPatch("/{id:int}/stock/{bodId:int}", async (
            int id,
            int bodId,
            AjustarStockDto dto,
            IValidator<AjustarStockDto> validator,
            IInventarioService service,
            CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID de producto debe ser mayor a cero." });
            if (bodId <= 0) return Results.BadRequest(new { error = "El ID de bodega debe ser mayor a cero." });

            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var resultado = await service.AjustarStockAsync(id, bodId, dto, ct);
            return Results.Ok(resultado);
        })
        .WithName("AjustarStock")
        .WithSummary("Ajuste directo de stock en una bodega (Entrada o Salida)");
    }
}
