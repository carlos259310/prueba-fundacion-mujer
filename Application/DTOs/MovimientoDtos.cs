using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.DTOs;

public sealed record MovimientoDto(
    int MovId,
    int ProdId,
    string ProdNombre,
    int? MovBodegaInicial,
    int? MovBodegaFinal,
    int MovCantidad,
    TipoMovimiento MovTipo,
    ConceptoMovimiento MovConcepto,
    DateTime MovFecha
);

public sealed record CreateMovimientoDto(
    int ProdId,
    int? MovBodegaInicial,
    int? MovBodegaFinal,
    int MovCantidad,
    TipoMovimiento MovTipo,
    ConceptoMovimiento MovConcepto
);

public sealed record ReporteMovimientosDto(
    DateTime FechaDesde,
    DateTime FechaHasta,
    int? ProdId,
    int TotalMovimientos,
    int TotalEntradas,
    int TotalSalidas,
    int TotalTrasladados,
    IReadOnlyList<ReporteItemDto> Detalle
);

public sealed record ReporteItemDto(
    DateOnly Fecha,
    int Entradas,
    int Salidas,
    int Traslados,
    int Total
);
