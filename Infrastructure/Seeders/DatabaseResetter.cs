using Microsoft.EntityFrameworkCore;

namespace ProductCatalog.Api.Infrastructure.Seeders;

public static class DatabaseResetter
{
    public static async Task ResetAsync(AppDbContext db)
    {
        Console.WriteLine("[Reset] Limpiando todas las tablas...");
        await db.Database.ExecuteSqlRawAsync(
            "TRUNCATE movimientos, inventario, productos, bodegas RESTART IDENTITY CASCADE;");
        Console.WriteLine("[Reset] Base de datos limpiada.");
    }
}
