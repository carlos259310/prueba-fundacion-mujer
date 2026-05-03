using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure.Repositories;

public sealed class ClienteRepository(AppDbContext db) : IClienteRepository
{
    // SELECT * FROM clientes ORDER BY cli_apellido, cli_nombre
    public async Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default) =>
        await db.Clientes.OrderBy(x => x.CliApellido).ThenBy(x => x.CliNombre).ToListAsync(ct);

    // SELECT * FROM clientes WHERE cli_id = @id  (devuelve null si no existe)
    public async Task<Cliente?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Clientes.FindAsync([id], ct);

    // INSERT INTO clientes (...) VALUES (...)
    public async Task<Cliente> CreateAsync(Cliente cliente, CancellationToken ct = default)
    {
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct); // aquí EF ejecuta el SQL y llena el CliId generado
        return cliente;
    }

    // UPDATE clientes SET ... WHERE cli_id = @id
    public async Task<Cliente> UpdateAsync(Cliente cliente, CancellationToken ct = default)
    {
        db.Clientes.Update(cliente);
        await db.SaveChangesAsync(ct);
        return cliente;
    }

    // DELETE FROM clientes WHERE cli_id = @id
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var cliente = await db.Clientes.FindAsync([id], ct);
        if (cliente is not null)
        {
            db.Clientes.Remove(cliente);
            await db.SaveChangesAsync(ct);
        }
    }
}
