using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure.Repositories;

public sealed class ProductoRepository(AppDbContext db) : IProductoRepository
{
    public async Task<PagedResult<Producto>> GetAllAsync(int page, int pageSize, string? marca, CancellationToken ct = default)
    {
        var query = db.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(marca))
            query = query.Where(x => x.ProdMarca != null && x.ProdMarca.ToLower().Contains(marca.ToLower()));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.ProdNombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Producto>(items, total, page, pageSize);
    }

    public async Task<Producto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Productos.FindAsync([id], ct);

    public async Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken ct = default) =>
        await db.Productos.FirstOrDefaultAsync(x => x.ProdCodigo == codigo, ct);

    public async Task<Producto> CreateAsync(Producto producto, CancellationToken ct = default)
    {
        db.Productos.Add(producto);
        await db.SaveChangesAsync(ct);
        return producto;
    }

    public async Task<Producto> UpdateAsync(Producto producto, CancellationToken ct = default)
    {
        db.Productos.Update(producto);
        await db.SaveChangesAsync(ct);
        return producto;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var producto = await db.Productos.FindAsync([id], ct);
        if (producto is not null)
        {
            db.Productos.Remove(producto);
            await db.SaveChangesAsync(ct);
        }
    }
}
