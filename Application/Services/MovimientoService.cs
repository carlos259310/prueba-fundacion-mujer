using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;
using ProductCatalog.Api.Domain.Exceptions;

namespace ProductCatalog.Api.Application.Services;

public sealed class MovimientoService(
    IMovimientoRepository movRepo,
    IInventarioRepository invRepo,
    IProductoRepository prodRepo,
    IBodegaRepository bodRepo) : IMovimientoService
{
    public async Task<PagedResult<MovimientoDto>> GetByProductoAsync(int prodId, int page, int pageSize, CancellationToken ct = default)
    {
        _ = await prodRepo.GetByIdAsync(prodId, ct)
            ?? throw new NotFoundException(nameof(Producto), prodId);
        return await movRepo.GetByProductoAsync(prodId, page, pageSize, ct);
    }

    public async Task<MovimientoDto> RegistrarAsync(CreateMovimientoDto dto, CancellationToken ct = default)
    {
        var producto = await prodRepo.GetByIdAsync(dto.ProdId, ct)
            ?? throw new NotFoundException(nameof(Producto), dto.ProdId);

        if (dto.MovTipo == TipoMovimiento.Entrada || dto.MovTipo == TipoMovimiento.Traslado)
        {
            _ = await bodRepo.GetByIdAsync(dto.MovBodegaFinal!.Value, ct)
                ?? throw new BodegaNoEncontradaException(dto.MovBodegaFinal.Value);
        }

        if (dto.MovTipo == TipoMovimiento.Salida || dto.MovTipo == TipoMovimiento.Traslado)
        {
            _ = await bodRepo.GetByIdAsync(dto.MovBodegaInicial!.Value, ct)
                ?? throw new BodegaNoEncontradaException(dto.MovBodegaInicial.Value);
        }

        await AplicarInventario(dto, ct);

        var movimiento = new Movimiento
        {
            ProdId = dto.ProdId,
            MovBodegaInicial = dto.MovBodegaInicial,
            MovBodegaFinal = dto.MovBodegaFinal,
            MovCantidad = dto.MovCantidad,
            MovTipo = dto.MovTipo,
            MovConcepto = dto.MovConcepto,
            MovFecha = DateTime.UtcNow
        };

        var creado = await movRepo.CreateAsync(movimiento, ct);

        return new MovimientoDto(
            creado.MovId, creado.ProdId, producto.ProdNombre,
            creado.MovBodegaInicial, creado.MovBodegaFinal,
            creado.MovCantidad, creado.MovTipo, creado.MovConcepto, creado.MovFecha);
    }

    private async Task AplicarInventario(CreateMovimientoDto dto, CancellationToken ct)
    {
        if (dto.MovTipo == TipoMovimiento.Entrada)
        {
            var inv = await ObtenerOCrear(dto.ProdId, dto.MovBodegaFinal!.Value, ct);
            inv.InvStock += dto.MovCantidad;
            inv.InvLastUpdate = DateTime.UtcNow;
            await invRepo.UpsertAsync(inv, ct);
        }
        else if (dto.MovTipo == TipoMovimiento.Salida)
        {
            var inv = await ObtenerOCrear(dto.ProdId, dto.MovBodegaInicial!.Value, ct);
            if (inv.InvStock < dto.MovCantidad)
                throw new StockInsuficienteException(inv.InvStock, dto.MovCantidad);
            inv.InvStock -= dto.MovCantidad;
            inv.InvLastUpdate = DateTime.UtcNow;
            await invRepo.UpsertAsync(inv, ct);
        }
        else
        {
            var origen = await ObtenerOCrear(dto.ProdId, dto.MovBodegaInicial!.Value, ct);
            if (origen.InvStock < dto.MovCantidad)
                throw new StockInsuficienteException(origen.InvStock, dto.MovCantidad);
            origen.InvStock -= dto.MovCantidad;
            origen.InvLastUpdate = DateTime.UtcNow;
            await invRepo.UpsertAsync(origen, ct);

            var destino = await ObtenerOCrear(dto.ProdId, dto.MovBodegaFinal!.Value, ct);
            destino.InvStock += dto.MovCantidad;
            destino.InvLastUpdate = DateTime.UtcNow;
            await invRepo.UpsertAsync(destino, ct);
        }
    }

    public Task<ReporteMovimientosDto> GenerarReporteAsync(DateTime desde, DateTime hasta, int? prodId, CancellationToken ct = default)
        => movRepo.GenerarReporteAsync(desde, hasta, prodId, ct);

    private async Task<Inventario> ObtenerOCrear(int prodId, int bodId, CancellationToken ct) =>
        await invRepo.GetAsync(prodId, bodId, ct)
            ?? new Inventario { ProdId = prodId, BodId = bodId, InvStock = 0, InvLastUpdate = DateTime.UtcNow };
}
