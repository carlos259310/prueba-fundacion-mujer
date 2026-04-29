namespace ProductCatalog.Api.Domain.Entities;

public sealed class Bodega
{
    public int BodId { get; set; }
    public string BodNombre { get; set; } = string.Empty;
    public bool BodPrincipal { get; set; }
}

public sealed class Producto
{
    public int ProdId { get; set; }
    public string ProdNombre { get; set; } = string.Empty;
    public string ProdCodigo { get; set; } = string.Empty;
    public string? ProdDescripcion { get; set; }
    public string? ProdMarca { get; set; }
}

public sealed class Inventario
{
    public int ProdId { get; set; }
    public int BodId { get; set; }
    public int InvStock { get; set; }
    public DateTime InvLastUpdate { get; set; }

    public Producto Producto { get; set; } = null!;
    public Bodega Bodega { get; set; } = null!;
}

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

public enum TipoMovimiento
{
    Entrada,
    Salida,
    Traslado
}

public enum ConceptoMovimiento
{
    Compra,
    Venta,
    Ajuste,
    Traslado,
    Devolucion
}
