using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IProductoRepository
{
    Task<PagedResult<Producto>> GetAllAsync(int page, int pageSize, string? marca, CancellationToken ct = default);
    Task<Producto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<Producto> CreateAsync(Producto producto, CancellationToken ct = default);
    Task<Producto> UpdateAsync(Producto producto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
