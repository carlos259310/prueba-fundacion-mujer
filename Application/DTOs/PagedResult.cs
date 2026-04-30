using System.ComponentModel;

namespace ProductCatalog.Api.Application.DTOs;

public sealed record PagedResult<T>(
    [property: Description("Elementos de la página actual.")]
    IEnumerable<T> Items,
    [property: Description("Total de registros en todas las páginas.")]
    int Total,
    [property: Description("Número de página actual (base 1).")]
    int Page,
    [property: Description("Cantidad de elementos por página.")]
    int PageSize
);
