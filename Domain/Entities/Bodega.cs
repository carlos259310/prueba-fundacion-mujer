namespace ProductCatalog.Api.Domain.Entities;

public sealed class Bodega
{
    public int BodId { get; set; }
    public string BodNombre { get; set; } = string.Empty;
    public bool BodPrincipal { get; set; }
}
