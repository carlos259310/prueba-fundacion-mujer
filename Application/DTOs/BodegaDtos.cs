using System.ComponentModel;

namespace ProductCatalog.Api.Application.DTOs;

public sealed record BodegaDto(
    [property: Description("ID único de la bodega.")]
    int BodId,
    [property: Description("Nombre de la bodega.")]
    string BodNombre,
    [property: Description("true si es la bodega principal del negocio.")]
    bool BodPrincipal
);

public sealed record CreateBodegaDto(
    [property: Description("Nombre. Requerido, máx. 100 caracteres.")]
    string BodNombre,
    [property: Description("true para marcarla como bodega principal. Por defecto false.")]
    bool BodPrincipal
);

public sealed record UpdateBodegaDto(
    [property: Description("Nuevo nombre. Requerido, máx. 100 caracteres.")]
    string BodNombre,
    [property: Description("true si es la bodega principal.")]
    bool BodPrincipal
);
