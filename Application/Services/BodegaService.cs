using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;
using ProductCatalog.Api.Domain.Exceptions;

namespace ProductCatalog.Api.Application.Services;

public sealed class BodegaService(IBodegaRepository repo) : IBodegaService
{
    public async Task<IEnumerable<BodegaDto>> GetAllAsync(CancellationToken ct = default)
    {
        var bodegas = await repo.GetAllAsync(ct);
        return bodegas.Select(ToDto);
    }

    public async Task<BodegaDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var bodega = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Bodega), id);
        return ToDto(bodega);
    }

    public async Task<BodegaDto> CreateAsync(CreateBodegaDto dto, CancellationToken ct = default)
    {
        var bodega = new Bodega
        {
            BodNombre = dto.BodNombre,
            BodPrincipal = dto.BodPrincipal
        };

        var creada = await repo.CreateAsync(bodega, ct);
        return ToDto(creada);
    }

    public async Task<BodegaDto> UpdateAsync(int id, UpdateBodegaDto dto, CancellationToken ct = default)
    {
        var bodega = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Bodega), id);

        bodega.BodNombre = dto.BodNombre;
        bodega.BodPrincipal = dto.BodPrincipal;

        var actualizada = await repo.UpdateAsync(bodega, ct);
        return ToDto(actualizada);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _ = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Bodega), id);
        await repo.DeleteAsync(id, ct);
    }

    private static BodegaDto ToDto(Bodega b) =>
        new(b.BodId, b.BodNombre, b.BodPrincipal);
}
