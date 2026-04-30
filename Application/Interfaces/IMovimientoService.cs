using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IMovimientoService
{
    Task<PagedResult<MovimientoDto>> GetAllAsync(int? prodId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<MovimientoDto>> GetByProductoAsync(int prodId, int page, int pageSize, CancellationToken ct = default);
    Task<MovimientoDto> RegistrarAsync(CreateMovimientoDto dto, CancellationToken ct = default);
    Task<ReporteMovimientosDto> GenerarReporteAsync(DateOnly desde, DateOnly hasta, int? prodId, CancellationToken ct = default);
}
