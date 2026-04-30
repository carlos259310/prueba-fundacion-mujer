using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IInventarioService
{
    Task<IEnumerable<InventarioDto>> GetAllStockAsync(int? prodId, CancellationToken ct = default);
    Task<IEnumerable<InventarioDto>> GetStockByProductoAsync(int prodId, CancellationToken ct = default);
    Task<IEnumerable<InventarioDto>> GetStockByBodegaAsync(int bodId, CancellationToken ct = default);
    Task<InventarioDto> AjustarStockAsync(int prodId, int bodId, AjustarStockDto dto, CancellationToken ct = default);
}
