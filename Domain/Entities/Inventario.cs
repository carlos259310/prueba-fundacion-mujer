namespace ProductCatalog.Api.Domain.Entities;

public sealed class Inventario
{
    public int ProdId { get; set; }
    public int BodId { get; set; }
    public int InvStock { get; set; }
    public DateTime InvLastUpdate { get; set; }

    public Producto Producto { get; set; } = null!;
    public Bodega Bodega { get; set; } = null!;
}
