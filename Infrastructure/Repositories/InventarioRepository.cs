using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure.Repositories;

public sealed class InventarioRepository(AppDbContext db) : IInventarioRepository
{
    public async Task<IEnumerable<InventarioDto>> GetAllStockAsync(int? prodId, CancellationToken ct = default)
    {
        var q = db.Inventarios
            .Include(x => x.Producto)
            .Include(x => x.Bodega)
            .AsQueryable();
        if (prodId.HasValue) q = q.Where(x => x.ProdId == prodId.Value);
        return await q
            .OrderBy(x => x.Producto.ProdNombre).ThenBy(x => x.Bodega.BodNombre)
            .Select(x => new InventarioDto(x.ProdId, x.Producto.ProdNombre, x.BodId, x.Bodega.BodNombre, x.InvStock, x.InvLastUpdate))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<InventarioDto>> GetStockByProductoAsync(int prodId, CancellationToken ct = default) =>
        await db.Inventarios
            .Include(x => x.Producto)
            .Include(x => x.Bodega)
            .Where(x => x.ProdId == prodId)
            .Select(x => new InventarioDto(x.ProdId, x.Producto.ProdNombre, x.BodId, x.Bodega.BodNombre, x.InvStock, x.InvLastUpdate))
            .ToListAsync(ct);

    public async Task<IEnumerable<InventarioDto>> GetStockByBodegaAsync(int bodId, CancellationToken ct = default) =>
        await db.Inventarios
            .Include(x => x.Producto)
            .Include(x => x.Bodega)
            .Where(x => x.BodId == bodId)
            .Select(x => new InventarioDto(x.ProdId, x.Producto.ProdNombre, x.BodId, x.Bodega.BodNombre, x.InvStock, x.InvLastUpdate))
            .ToListAsync(ct);

    public async Task<Inventario?> GetAsync(int prodId, int bodId, CancellationToken ct = default) =>
        await db.Inventarios.FindAsync([prodId, bodId], ct);

    public async Task<Inventario> UpsertAsync(Inventario inventario, CancellationToken ct = default)
    {
        if (db.Entry(inventario).State == EntityState.Detached)
            db.Inventarios.Add(inventario);

        await db.SaveChangesAsync(ct);
        return inventario;
    }
}
