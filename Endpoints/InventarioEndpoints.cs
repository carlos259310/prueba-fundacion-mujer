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
        .WithSummary("Stock de todos los productos en todas las bodegas")
        .WithDescription("""
            Devuelve el stock de todos los productos en todas las bodegas, ordenado por nombre de producto y bodega.

            **Parámetro (query):**
            - `prodId` — (Opcional) Filtrar por un producto específico
            """)
        .Produces<IEnumerable<InventarioDto>>(200);

        var group = app.MapGroup("/api/productos").WithTags("Inventario");

        group.MapGet("/{id:int}/stock", async (int id, IInventarioService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            var stock = await service.GetStockByProductoAsync(id, ct);
            return Results.Ok(stock);
        })
        .WithName("ObtenerStockProducto")
        .WithSummary("Ver stock del producto en todas las bodegas")
        .WithDescription("""
            Devuelve el stock del producto en cada bodega donde tiene registros.

            **Errores:** `400` si id ≤ 0 · `404` si el producto no existe.
            """)
        .Produces<IEnumerable<InventarioDto>>(200)
        .Produces<object>(400)
        .Produces<object>(404);

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
        .WithSummary("Ajuste directo de stock de un producto en una bodega")
        .WithDescription("""
            Registra un movimiento de ajuste y actualiza el stock del producto en la bodega indicada.

            **Tipos permitidos:**
            - `Entrada` → suma la cantidad al stock
            - `Salida` → resta la cantidad del stock (falla con `400` si stock insuficiente)
            - `Traslado` → **no permitido** en ajuste directo → `400`

            **Cantidades:** enteros positivos sin decimales. Mínimo: 1, Máximo: 999 999.

            **Errores:** `400` validación / stock insuficiente / tipo Traslado · `404` producto o bodega no existe.
            """)
        .Produces<InventarioDto>(200)
        .Produces<object>(400)
        .Produces<object>(404);
    }
}
