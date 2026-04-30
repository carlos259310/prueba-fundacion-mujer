using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.Interfaces;

public interface IMovimientoRepository
{
    Task<PagedResult<MovimientoDto>> GetByProductoAsync(int prodId, int page, int pageSize, CancellationToken ct = default);
    Task<Movimiento> CreateAsync(Movimiento movimiento, CancellationToken ct = default);
    Task<ReporteMovimientosDto> GenerarReporteAsync(DateTime desde, DateTime hasta, int? prodId, CancellationToken ct = default);
}
