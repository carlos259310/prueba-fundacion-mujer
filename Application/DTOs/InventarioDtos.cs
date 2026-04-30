using System.ComponentModel;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.DTOs;

public sealed record InventarioDto(
    [property: Description("ID del producto.")]
    int ProdId,
    [property: Description("Nombre del producto.")]
    string ProdNombre,
    [property: Description("ID de la bodega.")]
    int BodId,
    [property: Description("Nombre de la bodega.")]
    string BodNombre,
    [property: Description("Cantidad disponible en stock.")]
    int InvStock,
    [property: Description("Fecha y hora UTC de la última actualización.")]
    DateTime InvLastUpdate
);

public sealed record AjustarStockDto(
    [property: Description("Cantidad. Entero positivo sin decimales. Mínimo: 1, Máximo: 999 999.")]
    int Cantidad,
    [property: Description("Tipo de ajuste: Entrada (suma al stock) | Salida (resta del stock). Traslado no permitido aquí → 400.")]
    TipoMovimiento Tipo,
    [property: Description("Concepto: Compra | Venta | Ajuste | Devolucion.")]
    ConceptoMovimiento Concepto
);
