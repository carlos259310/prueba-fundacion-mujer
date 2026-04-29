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
