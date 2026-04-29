namespace ProductCatalog.Api.Application.DTOs;

public sealed record PagedResult<T>(
    IEnumerable<T> Items,
    int Total,
    int Page,
    int PageSize
);
