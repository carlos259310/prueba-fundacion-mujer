using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IBodegaService
{
    Task<IEnumerable<BodegaDto>> GetAllAsync(CancellationToken ct = default);
    Task<BodegaDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<BodegaDto> CreateAsync(CreateBodegaDto dto, CancellationToken ct = default);
    Task<BodegaDto> UpdateAsync(int id, UpdateBodegaDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
