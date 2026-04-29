namespace ProductCatalog.Api.Application.DTOs;

public sealed record ProductoDto(
    int ProdId,
    string ProdNombre,
    string ProdCodigo,
    string? ProdDescripcion,
    string? ProdMarca
);

public sealed record CreateProductoDto(
    string ProdNombre,
    string ProdCodigo,
    string? ProdDescripcion,
    string? ProdMarca
);

public sealed record UpdateProductoDto(
    string ProdNombre,
    string ProdCodigo,
    string? ProdDescripcion,
    string? ProdMarca
);
