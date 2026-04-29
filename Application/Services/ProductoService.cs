using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;
using ProductCatalog.Api.Domain.Exceptions;

namespace ProductCatalog.Api.Application.Services;

public sealed class ProductoService(IProductoRepository repo) : IProductoService
{
    public async Task<PagedResult<ProductoDto>> GetAllAsync(int page, int pageSize, string? marca, CancellationToken ct = default)
    {
        var result = await repo.GetAllAsync(page, pageSize, marca, ct);
        return new PagedResult<ProductoDto>(result.Items.Select(ToDto), result.Total, result.Page, result.PageSize);
    }

    public async Task<ProductoDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var producto = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Producto), id);
        return ToDto(producto);
    }

    public async Task<ProductoDto> CreateAsync(CreateProductoDto dto, CancellationToken ct = default)
    {
        var existe = await repo.GetByCodigoAsync(dto.ProdCodigo, ct);
        if (existe is not null)
            throw new CodigoYaExisteException(dto.ProdCodigo);

        var producto = new Producto
        {
            ProdNombre = dto.ProdNombre,
            ProdCodigo = dto.ProdCodigo,
            ProdDescripcion = dto.ProdDescripcion,
            ProdMarca = dto.ProdMarca
        };

        var creado = await repo.CreateAsync(producto, ct);
        return ToDto(creado);
    }

    public async Task<ProductoDto> UpdateAsync(int id, UpdateProductoDto dto, CancellationToken ct = default)
    {
        var producto = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Producto), id);

        var codigoEnUso = await repo.GetByCodigoAsync(dto.ProdCodigo, ct);
        if (codigoEnUso is not null && codigoEnUso.ProdId != id)
            throw new CodigoYaExisteException(dto.ProdCodigo);

        producto.ProdNombre = dto.ProdNombre;
        producto.ProdCodigo = dto.ProdCodigo;
        producto.ProdDescripcion = dto.ProdDescripcion;
        producto.ProdMarca = dto.ProdMarca;

        var actualizado = await repo.UpdateAsync(producto, ct);
        return ToDto(actualizado);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var producto = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Producto), id);
        await repo.DeleteAsync(producto.ProdId, ct);
    }

    private static ProductoDto ToDto(Producto p) =>
        new(p.ProdId, p.ProdNombre, p.ProdCodigo, p.ProdDescripcion, p.ProdMarca);
}
