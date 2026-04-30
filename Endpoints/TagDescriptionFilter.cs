using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProductCatalog.Api.Endpoints;

public sealed class TagDescriptionFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags =
        [
            new() { Name = "Bodegas",     Description = "Gestión de almacenes y puntos de inventario" },
            new() { Name = "Productos",   Description = "Catálogo de productos disponibles" },
            new() { Name = "Inventario",  Description = "Stock por producto y bodega. Consulta y ajuste directo." },
            new() { Name = "Movimientos", Description = "Entradas, salidas y traslados de mercancía. Historial y reporte." },
        ];
    }
}
