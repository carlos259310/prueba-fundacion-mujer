using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IBodegaRepository
{
    Task<IEnumerable<Bodega>> GetAllAsync(CancellationToken ct = default);
    Task<Bodega?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Bodega> CreateAsync(Bodega bodega, CancellationToken ct = default);
    Task<Bodega> UpdateAsync(Bodega bodega, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
