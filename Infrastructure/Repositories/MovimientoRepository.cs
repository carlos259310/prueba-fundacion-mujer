using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure.Repositories;

public sealed class MovimientoRepository(AppDbContext db) : IMovimientoRepository
{
    public async Task<PagedResult<MovimientoDto>> GetAllAsync(int? prodId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Movimientos.Include(x => x.Producto).AsQueryable();
        if (prodId.HasValue) q = q.Where(x => x.ProdId == prodId.Value);
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.MovFecha)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new MovimientoDto(
                x.MovId, x.ProdId, x.Producto.ProdNombre,
                x.MovBodegaInicial, x.MovBodegaFinal,
                x.MovCantidad, x.MovTipo, x.MovConcepto, x.MovFecha))
            .ToListAsync(ct);
        return new PagedResult<MovimientoDto>(items, total, page, pageSize);
    }

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

    public async Task<ReporteMovimientosDto> GenerarReporteAsync(DateTime desde, DateTime hasta, int? prodId, CancellationToken ct = default)
    {
        var desdeUtc = DateTime.SpecifyKind(desde.Date, DateTimeKind.Utc);
        var hastaUtc = DateTime.SpecifyKind(hasta.Date.AddDays(1), DateTimeKind.Utc);

        var query = db.Movimientos
            .Where(x => x.MovFecha >= desdeUtc && x.MovFecha < hastaUtc);

        if (prodId.HasValue)
            query = query.Where(x => x.ProdId == prodId.Value);

        var movimientos = await query
            .Select(x => new { x.MovFecha, x.MovTipo })
            .ToListAsync(ct);

        var detalle = movimientos
            .GroupBy(x => DateOnly.FromDateTime(x.MovFecha))
            .OrderBy(g => g.Key)
            .Select(g => new ReporteItemDto(
                g.Key,
                g.Count(x => x.MovTipo == TipoMovimiento.Entrada),
                g.Count(x => x.MovTipo == TipoMovimiento.Salida),
                g.Count(x => x.MovTipo == TipoMovimiento.Traslado),
                g.Count()))
            .ToList();

        return new ReporteMovimientosDto(
            desde.Date,
            hasta.Date,
            prodId,
            detalle.Sum(d => d.Total),
            detalle.Sum(d => d.Entradas),
            detalle.Sum(d => d.Salidas),
            detalle.Sum(d => d.Traslados),
            detalle);
    }
}
