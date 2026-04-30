using System.ComponentModel;

namespace ProductCatalog.Api.Application.DTOs;

public sealed record ProductoDto(
    [property: Description("ID único del producto.")]
    int ProdId,
    [property: Description("Nombre del producto.")]
    string ProdNombre,
    [property: Description("Código SKU único del producto.")]
    string ProdCodigo,
    [property: Description("Descripción del producto (puede ser null).")]
    string? ProdDescripcion,
    [property: Description("Marca del producto (puede ser null).")]
    string? ProdMarca
);

public sealed record CreateProductoDto(
    [property: Description("Nombre. Requerido, máx. 200 caracteres.")]
    string ProdNombre,
    [property: Description("Código SKU único. Requerido, máx. 50 caracteres. Duplicado → 409 Conflict.")]
    string ProdCodigo,
    [property: Description("Descripción opcional. Máx. 500 caracteres.")]
    string? ProdDescripcion,
    [property: Description("Marca opcional. Máx. 100 caracteres.")]
    string? ProdMarca
);

public sealed record UpdateProductoDto(
    [property: Description("Nombre. Requerido, máx. 200 caracteres.")]
    string ProdNombre,
    [property: Description("Código SKU único. Requerido, máx. 50 caracteres. Duplicado → 409 Conflict.")]
    string ProdCodigo,
    [property: Description("Descripción opcional. Máx. 500 caracteres.")]
    string? ProdDescripcion,
    [property: Description("Marca opcional. Máx. 100 caracteres.")]
    string? ProdMarca
);
