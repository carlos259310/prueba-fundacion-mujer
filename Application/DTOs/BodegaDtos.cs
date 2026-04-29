namespace ProductCatalog.Api.Application.DTOs;

public sealed record BodegaDto(
    int BodId,
    string BodNombre,
    bool BodPrincipal
);

public sealed record CreateBodegaDto(
    string BodNombre,
    bool BodPrincipal
);

public sealed record UpdateBodegaDto(
    string BodNombre,
    bool BodPrincipal
);
