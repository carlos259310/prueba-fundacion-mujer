using System.ComponentModel;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.DTOs;

public sealed record MovimientoDto(
    [property: Description("ID único del movimiento.")]
    int MovId,
    [property: Description("ID del producto.")]
    int ProdId,
    [property: Description("Nombre del producto.")]
    string ProdNombre,
    [property: Description("ID de la bodega origen (null para Entrada).")]
    int? MovBodegaInicial,
    [property: Description("ID de la bodega destino (null para Salida).")]
    int? MovBodegaFinal,
    [property: Description("Cantidad movida. Entero positivo (1–999 999), sin decimales.")]
    int MovCantidad,
    [property: Description("Tipo: Entrada | Salida | Traslado.")]
    TipoMovimiento MovTipo,
    [property: Description("Concepto: Compra | Venta | Ajuste | Traslado | Devolucion.")]
    ConceptoMovimiento MovConcepto,
    [property: Description("Fecha y hora UTC del movimiento.")]
    DateTime MovFecha
);

public sealed record CreateMovimientoDto(
    [property: Description("ID del producto. Requerido, mayor a 0.")]
    int ProdId,
    [property: Description("ID bodega origen. Requerido para Salida y Traslado; null en Entrada.")]
    int? MovBodegaInicial,
    [property: Description("ID bodega destino. Requerido para Entrada y Traslado; null en Salida.")]
    int? MovBodegaFinal,
    [property: Description("Cantidad. Entero positivo sin decimales. Mínimo: 1, Máximo: 999 999.")]
    int MovCantidad,
    [property: Description("Tipo de movimiento: Entrada | Salida | Traslado.")]
    TipoMovimiento MovTipo,
    [property: Description("Concepto: Compra | Venta | Ajuste | Traslado | Devolucion.")]
    ConceptoMovimiento MovConcepto
);

public sealed record ReporteMovimientosDto(
    [property: Description("Fecha inicio del reporte (yyyy-MM-dd).")]
    DateOnly FechaDesde,
    [property: Description("Fecha fin del reporte (yyyy-MM-dd).")]
    DateOnly FechaHasta,
    [property: Description("ID del producto filtrado (null = todos los productos).")]
    int? ProdId,
    [property: Description("Total de movimientos en el período.")]
    int TotalMovimientos,
    [property: Description("Total de entradas en el período.")]
    int TotalEntradas,
    [property: Description("Total de salidas en el período.")]
    int TotalSalidas,
    [property: Description("Total de traslados en el período.")]
    int TotalTrasladados,
    [property: Description("Detalle diario de movimientos.")]
    IReadOnlyList<ReporteItemDto> Detalle
);

public sealed record ReporteItemDto(
    [property: Description("Fecha del día (yyyy-MM-dd).")]
    DateOnly Fecha,
    [property: Description("Entradas en el día.")]
    int Entradas,
    [property: Description("Salidas en el día.")]
    int Salidas,
    [property: Description("Traslados en el día.")]
    int Traslados,
    [property: Description("Total de movimientos en el día.")]
    int Total
);
