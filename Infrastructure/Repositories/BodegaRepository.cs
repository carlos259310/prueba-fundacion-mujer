using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure.Repositories;

public sealed class BodegaRepository(AppDbContext db) : IBodegaRepository
{
    public async Task<IEnumerable<Bodega>> GetAllAsync(CancellationToken ct = default) =>
        await db.Bodegas.OrderBy(x => x.BodNombre).ToListAsync(ct);

    public async Task<Bodega?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Bodegas.FindAsync([id], ct);

    public async Task<Bodega> CreateAsync(Bodega bodega, CancellationToken ct = default)
    {
        db.Bodegas.Add(bodega);
        await db.SaveChangesAsync(ct);
        return bodega;
    }

    public async Task<Bodega> UpdateAsync(Bodega bodega, CancellationToken ct = default)
    {
        db.Bodegas.Update(bodega);
        await db.SaveChangesAsync(ct);
        return bodega;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var bodega = await db.Bodegas.FindAsync([id], ct);
        if (bodega is not null)
        {
            db.Bodegas.Remove(bodega);
            await db.SaveChangesAsync(ct);
        }
    }
}
