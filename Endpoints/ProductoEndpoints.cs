using FluentValidation;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;

namespace ProductCatalog.Api.Endpoints;

public static class ProductoEndpoints
{
    public static void MapProductoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/productos").WithTags("Productos");

        group.MapGet("/", async (
            IProductoService service,
            int page = 1,
            int pageSize = 10,
            string? marca = null,
            CancellationToken ct = default) =>
        {
            var result = await service.GetAllAsync(page, pageSize, marca, ct);
            return Results.Ok(result);
        })
        .WithName("ListarProductos")
        .WithSummary("Listar productos con paginación");

        group.MapGet("/{id:int}", async (int id, IProductoService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            var producto = await service.GetByIdAsync(id, ct);
            return Results.Ok(producto);
        })
        .WithName("ObtenerProducto")
        .WithSummary("Obtener producto por ID");

        group.MapPost("/", async (
            CreateProductoDto dto,
            IValidator<CreateProductoDto> validator,
            IProductoService service,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var creado = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/productos/{creado.ProdId}", creado);
        })
        .WithName("CrearProducto")
        .WithSummary("Crear producto");

        group.MapPut("/{id:int}", async (
            int id,
            UpdateProductoDto dto,
            IValidator<UpdateProductoDto> validator,
            IProductoService service,
            CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var actualizado = await service.UpdateAsync(id, dto, ct);
            return Results.Ok(actualizado);
        })
        .WithName("ActualizarProducto")
        .WithSummary("Actualizar producto");

        group.MapDelete("/{id:int}", async (int id, IProductoService service, CancellationToken ct) =>
        {
            if (id <= 0) return Results.BadRequest(new { error = "El ID debe ser un número mayor a cero." });
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("EliminarProducto")
        .WithSummary("Eliminar producto");
    }
}
