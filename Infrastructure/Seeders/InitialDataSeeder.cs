using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure.Seeders;

public static class InitialDataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Bodegas.Any() || db.Productos.Any())
        {
            Console.WriteLine("[Seeder] La base de datos ya tiene datos. No se sembraron registros.");
            return;
        }

        db.Bodegas.Add(new Bodega
        {
            BodNombre = "Bodega Principal",
            BodPrincipal = true
        });

        db.Productos.AddRange(
            new Producto
            {
                ProdNombre = "Laptop Dell XPS 15",
                ProdCodigo = "LAP-001",
                ProdDescripcion = "Laptop 15 pulgadas Intel i7 16GB RAM",
                ProdMarca = "Dell"
            },
            new Producto
            {
                ProdNombre = "Mouse Logitech MX Master 3",
                ProdCodigo = "MOU-001",
                ProdDescripcion = "Mouse inalámbrico ergonómico 7 botones",
                ProdMarca = "Logitech"
            },
            new Producto
            {
                ProdNombre = "Teclado Mecánico Keychron K2",
                ProdCodigo = "TEC-001",
                ProdDescripcion = "Teclado mecánico compacto switches Brown",
                ProdMarca = "Keychron"
            },
            new Producto
            {
                ProdNombre = "Monitor LG UltraWide 34\"",
                ProdCodigo = "MON-001",
                ProdDescripcion = "Monitor curvo 34 pulgadas 3440x1440",
                ProdMarca = "LG"
            },
            new Producto
            {
                ProdNombre = "Audífonos Sony WH-1000XM5",
                ProdCodigo = "AUD-001",
                ProdDescripcion = "Audífonos inalámbricos noise cancelling",
                ProdMarca = "Sony"
            }
        );

        await db.SaveChangesAsync();
        Console.WriteLine("[Seeder] Bodega principal y 5 productos creados exitosamente.");
    }
}
