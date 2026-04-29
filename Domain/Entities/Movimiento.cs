namespace ProductCatalog.Api.Domain.Entities;

public sealed class Movimiento
{
    public int MovId { get; set; }
    public int ProdId { get; set; }
    public int? MovBodegaInicial { get; set; }
    public int? MovBodegaFinal { get; set; }
    public int MovCantidad { get; set; }
    public TipoMovimiento MovTipo { get; set; }
    public ConceptoMovimiento MovConcepto { get; set; }
    public DateTime MovFecha { get; set; }

    public Producto Producto { get; set; } = null!;
}
