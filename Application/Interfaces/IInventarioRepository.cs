using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IInventarioRepository
{
    Task<IEnumerable<InventarioDto>> GetAllStockAsync(int? prodId, CancellationToken ct = default);
    Task<IEnumerable<InventarioDto>> GetStockByProductoAsync(int prodId, CancellationToken ct = default);
    Task<IEnumerable<InventarioDto>> GetStockByBodegaAsync(int bodId, CancellationToken ct = default);
    Task<Inventario?> GetAsync(int prodId, int bodId, CancellationToken ct = default);
    Task<Inventario> UpsertAsync(Inventario inventario, CancellationToken ct = default);
}
