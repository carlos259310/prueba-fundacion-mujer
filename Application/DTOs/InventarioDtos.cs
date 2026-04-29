using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.DTOs;

public sealed record InventarioDto(
    int ProdId,
    string ProdNombre,
    int BodId,
    string BodNombre,
    int InvStock,
    DateTime InvLastUpdate
);

public sealed record AjustarStockDto(
    int Cantidad,
    TipoMovimiento Tipo,
    ConceptoMovimiento Concepto
);
