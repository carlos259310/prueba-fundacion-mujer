using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> GetAllAsync(CancellationToken ct = default);
    Task<ClienteDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClienteDto> CreateAsync(CreateClienteDto dto, CancellationToken ct = default);
    Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}