using FluentValidation;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;

namespace ProductCatalog.Api.Endpoints;

public static class MovimientoEndpoints
{
    public static void MapMovimientoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/movimientos").WithTags("Movimientos");

        group.MapGet("/", async (
            IMovimientoService service,
            int? prodId = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken ct = default) =>
            Results.Ok(await service.GetAllAsync(prodId, page, pageSize, ct)))
        .WithName("ListarMovimientos")
        .WithSummary("Historial paginado de todos los movimientos")
        .WithDescription("""
            Devuelve todos los movimientos ordenados por fecha descendente.

            **Parámetros (query):**
            - `prodId` — (Opcional) Filtrar por producto
            - `page` — Número de página (default: 1)
            - `pageSize` — Elementos por página (default: 10)
            """)
        .Produces<PagedResult<MovimientoDto>>(200);

        group.MapGet("/reporte", async (
            DateOnly fechaDesde,
            DateOnly fechaHasta,
            IMovimientoService service,
            int? prodId = null,
            CancellationToken ct = default) =>
        {
            if (fechaHasta < fechaDesde)
                return Results.BadRequest(new { error = "fechaHasta debe ser mayor o igual a fechaDesde." });
            var reporte = await service.GenerarReporteAsync(fechaDesde, fechaHasta, prodId, ct);
            return Results.Ok(reporte);
        })
        .WithName("ReporteMovimientos")
        .WithSummary("Reporte agrupado de movimientos por rango de fechas")
        .WithDescription("""
            Genera un reporte diario en el rango `[fechaDesde, fechaHasta]` (ambos inclusive).

            **Parámetros (query):**
            - `fechaDesde` — Fecha inicio, formato **`yyyy-MM-dd`** · Ej: `2026-01-01` · *Requerido*
            - `fechaHasta` — Fecha fin, formato **`yyyy-MM-dd`** · Ej: `2026-04-30` · *Requerido*
            - `prodId` — (Opcional) Filtrar por producto

            **Respuesta 200:** Objeto `ReporteMovimientosDto` con totales globales y detalle por día.

            **Error 400:** `fechaHasta` anterior a `fechaDesde`.
            """)
        .Produces<ReporteMovimientosDto>(200)
        .Produces<object>(400);

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
        .WithSummary("Historial de movimientos de un producto específico (paginado)")
        .WithDescription("""
            Devuelve los movimientos de un producto ordenados por fecha descendente.

            **Parámetros:**
            - `prodId` — ID del producto (ruta)
            - `page` / `pageSize` — Paginación (query, defaults: 1 / 10)

            **Errores:** `400` si prodId ≤ 0 · `404` si el producto no existe.
            """)
        .Produces<PagedResult<MovimientoDto>>(200)
        .Produces<object>(400)
        .Produces<object>(404);

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
        .WithSummary("Registrar un movimiento de inventario (Entrada, Salida o Traslado)")
        .WithDescription("""
            Registra el movimiento y actualiza el stock automáticamente.

            **Reglas de bodegas según tipo:**
            | Tipo | movBodegaInicial | movBodegaFinal | Efecto en stock |
            |---|---|---|---|
            | `Entrada` | — (ignorar) | ✅ Requerida | Suma cantidad en destino |
            | `Salida` | ✅ Requerida | — (ignorar) | Resta cantidad de origen |
            | `Traslado` | ✅ Requerida | ✅ Requerida | Resta de origen + suma a destino |

            **Cantidades:** enteros positivos sin decimales. Mínimo: 1, Máximo: 999 999.

            **Errores:**
            - `400` stock insuficiente para Salida o Traslado
            - `400` bodega especificada no existe
            - `400` validación de campos falla
            - `404` producto no existe
            """)
        .Produces<MovimientoDto>(201)
        .Produces<object>(400)
        .Produces<object>(404);
    }
}
