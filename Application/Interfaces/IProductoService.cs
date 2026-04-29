using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IProductoService
{
    Task<PagedResult<ProductoDto>> GetAllAsync(int page, int pageSize, string? marca, CancellationToken ct = default);
    Task<ProductoDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductoDto> CreateAsync(CreateProductoDto dto, CancellationToken ct = default);
    Task<ProductoDto> UpdateAsync(int id, UpdateProductoDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
