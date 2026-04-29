using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;
using ProductCatalog.Api.Domain.Exceptions;

namespace ProductCatalog.Api.Application.Services;

public sealed class InventarioService(
    IInventarioRepository invRepo,
    IProductoRepository prodRepo,
    IBodegaRepository bodRepo) : IInventarioService
{
    public async Task<IEnumerable<InventarioDto>> GetStockByProductoAsync(int prodId, CancellationToken ct = default)
    {
        _ = await prodRepo.GetByIdAsync(prodId, ct)
            ?? throw new NotFoundException(nameof(Producto), prodId);
        return await invRepo.GetStockByProductoAsync(prodId, ct);
    }

    public async Task<IEnumerable<InventarioDto>> GetStockByBodegaAsync(int bodId, CancellationToken ct = default)
    {
        _ = await bodRepo.GetByIdAsync(bodId, ct)
            ?? throw new BodegaNoEncontradaException(bodId);
        return await invRepo.GetStockByBodegaAsync(bodId, ct);
    }

    public async Task<InventarioDto> AjustarStockAsync(int prodId, int bodId, AjustarStockDto dto, CancellationToken ct = default)
    {
        var producto = await prodRepo.GetByIdAsync(prodId, ct)
            ?? throw new NotFoundException(nameof(Producto), prodId);

        _ = await bodRepo.GetByIdAsync(bodId, ct)
            ?? throw new BodegaNoEncontradaException(bodId);

        if (dto.Tipo == TipoMovimiento.Traslado)
            throw new OperacionInvalidaException("Use POST /api/movimientos para registrar un traslado entre bodegas.");

        var inventario = await invRepo.GetAsync(prodId, bodId, ct)
            ?? new Inventario { ProdId = prodId, BodId = bodId, InvStock = 0, InvLastUpdate = DateTime.UtcNow };

        if (dto.Tipo == TipoMovimiento.Entrada)
        {
            inventario.InvStock += dto.Cantidad;
        }
        else
        {
            if (inventario.InvStock < dto.Cantidad)
                throw new StockInsuficienteException(inventario.InvStock, dto.Cantidad);
            inventario.InvStock -= dto.Cantidad;
        }

        inventario.InvLastUpdate = DateTime.UtcNow;
        await invRepo.UpsertAsync(inventario, ct);

        var bodega = await bodRepo.GetByIdAsync(bodId, ct);
        return new InventarioDto(prodId, producto.ProdNombre, bodId, bodega!.BodNombre, inventario.InvStock, inventario.InvLastUpdate);
    }
}
