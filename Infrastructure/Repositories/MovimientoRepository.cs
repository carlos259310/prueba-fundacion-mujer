using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure.Repositories;

public sealed class MovimientoRepository(AppDbContext db) : IMovimientoRepository
{
    public async Task<PagedResult<MovimientoDto>> GetByProductoAsync(int prodId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Movimientos
            .Include(x => x.Producto)
            .Where(x => x.ProdId == prodId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.MovFecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MovimientoDto(
                x.MovId, x.ProdId, x.Producto.ProdNombre,
                x.MovBodegaInicial, x.MovBodegaFinal,
                x.MovCantidad, x.MovTipo, x.MovConcepto, x.MovFecha))
            .ToListAsync(ct);

        return new PagedResult<MovimientoDto>(items, total, page, pageSize);
    }

    public async Task<Movimiento> CreateAsync(Movimiento movimiento, CancellationToken ct = default)
    {
        db.Movimientos.Add(movimiento);
        await db.SaveChangesAsync(ct);
        return movimiento;
    }
}
