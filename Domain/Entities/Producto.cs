namespace ProductCatalog.Api.Domain.Entities;

public sealed class Producto
{
    public int ProdId { get; set; }
    public string ProdNombre { get; set; } = string.Empty;
    public string ProdCodigo { get; set; } = string.Empty;
    public string? ProdDescripcion { get; set; }
    public string? ProdMarca { get; set; }
}
