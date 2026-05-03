using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;
using ProductCatalog.Api.Domain.Exceptions;

namespace ProductCatalog.Api.Application.Services;

public sealed class ClienteService(IClienteRepository repo) : IClienteService
{
    public async Task<IEnumerable<ClienteDto>> GetAllAsync(CancellationToken ct = default)
    {
        var clientes = await repo.GetAllAsync(ct);
        return clientes.Select(ToDto); // convierte cada entidad a DTO
    }

    public async Task<ClienteDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var cliente = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cliente), id); // lanza 404 si no existe
        return ToDto(cliente);
    }

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto, CancellationToken ct = default)
    {
        var cliente = new Cliente // construye la entidad desde el DTO
        {
            CliNombre = dto.CliNombre,
            CliApellido = dto.CliApellido,
            CliCorreo = dto.CliCorreo,
            CliCelular = dto.CliCelular,
            CliDireccion = dto.CliDireccion
        };
        var creado = await repo.CreateAsync(cliente, ct);
        return ToDto(creado); // devuelve el DTO con el ID que asignó la BD
    }

    public async Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto, CancellationToken ct = default)
    {
        var cliente = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cliente), id);

        // actualiza los campos sobre la entidad existente
        cliente.CliNombre = dto.CliNombre;
        cliente.CliApellido = dto.CliApellido;
        cliente.CliCorreo = dto.CliCorreo;
        cliente.CliCelular = dto.CliCelular;
        cliente.CliDireccion = dto.CliDireccion;

        var actualizado = await repo.UpdateAsync(cliente, ct);
        return ToDto(actualizado);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _ = await repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cliente), id); // verifica que existe antes de borrar
        await repo.DeleteAsync(id, ct);
    }

    // método privado de conversión: entidad → DTO
    private static ClienteDto ToDto(Cliente c) =>
        new(c.CliId, c.CliNombre, c.CliApellido, c.CliCorreo, c.CliCelular, c.CliDireccion);
}