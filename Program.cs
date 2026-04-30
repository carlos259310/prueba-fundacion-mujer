using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Application.Interfaces;
using ProductCatalog.Api.Application.Services;
using ProductCatalog.Api.Endpoints;
using ProductCatalog.Api.Infrastructure;
using ProductCatalog.Api.Infrastructure.Repositories;
using ProductCatalog.Api.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Railway asigna PORT dinámicamente; localmente usa 5080
var port = Environment.GetEnvironmentVariable("PORT") ?? "5080";
builder.WebHost.UseUrls($"http://+:{port}");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ProductCatalog API",
        Version = "v1",
        Description = """
            > 🖥️ **[← Ir a la interfaz visual](/)** &nbsp;·&nbsp; [Bodegas](/bodegas.html) &nbsp;·&nbsp; [Productos](/productos.html) &nbsp;·&nbsp; [Inventario](/inventario.html) &nbsp;·&nbsp; [Movimientos](/movimientos.html)

            ---

            API REST para gestión de catálogo de productos e inventario por bodega.

            ---

            ## Bodegas
            Representan almacenes o puntos físicos de inventario.
            - `GET    /api/bodegas` — listar todas las bodegas
            - `GET    /api/bodegas/{id}` — detalle de una bodega
            - `POST   /api/bodegas` — crear bodega (`bodNombre` requerido, `bodPrincipal` opcional)
            - `PUT    /api/bodegas/{id}` — actualizar bodega
            - `DELETE /api/bodegas/{id}` — eliminar bodega

            ---

            ## Productos
            Catálogo de artículos disponibles para inventario.
            - `GET    /api/productos` — listar productos (paginado: `page`, `pageSize`)
            - `GET    /api/productos/{id}` — detalle de un producto
            - `POST   /api/productos` — crear producto (`prodNombre` y `prodCodigo` requeridos; código único → `409`)
            - `PUT    /api/productos/{id}` — actualizar producto
            - `DELETE /api/productos/{id}` — eliminar producto

            ---

            ## Inventario
            Stock de un producto en una bodega específica. No existe stock global.
            - `GET   /api/productos/{id}/stock` — ver stock del producto en todas las bodegas
            - `PATCH /api/productos/{id}/stock/{bodId}` — ajuste directo de stock

            **Ajuste directo** (`cantidad` siempre positiva, `tipo` define la dirección):
            - `Entrada` → suma al stock
            - `Salida` → resta del stock (falla con `400` si stock insuficiente)
            - `Traslado` → **no permitido** en ajuste directo (`400`)

            ---

            ## Movimientos
            Registro histórico de todas las operaciones de inventario.
            - `POST /api/movimientos` — registrar movimiento (Entrada, Salida o Traslado)
            - `GET  /api/movimientos/{prodId}` — historial paginado por producto
            - `GET  /api/movimientos/reporte` — reporte agrupado por día (`fechaDesde`, `fechaHasta` requeridos; `prodId` opcional)

            | Tipo | `movBodegaInicial` | `movBodegaFinal` | Efecto |
            |---|---|---|---|
            | `Entrada` | — | ✅ requerida | Suma cantidad en bodega destino |
            | `Salida` | ✅ requerida | — | Resta cantidad de bodega origen |
            | `Traslado` | ✅ requerida | ✅ requerida | Resta de origen, suma a destino |

            ---

            ## Reglas de negocio
            - Stock **nunca negativo** → `400 Bad Request`
            - Código de producto **único** → `409 Conflict`
            - Bodega inexistente en movimiento → `400 Bad Request`
            - Producto inexistente → `404 Not Found`
            - `Traslado` en ajuste directo PATCH → `400 Bad Request`

            ---

            ## Enums
            **TipoMovimiento:** `Entrada` · `Salida` · `Traslado`

            **ConceptoMovimiento:** `Compra` · `Venta` · `Ajuste` · `Traslado` · `Devolucion`
            """
    });
    c.UseInlineDefinitionsForEnums();
    c.SchemaFilter<EnumSchemaFilter>();
    c.DocumentFilter<TagDescriptionFilter>();
});

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IBodegaRepository, BodegaRepository>();
builder.Services.AddScoped<IBodegaService, BodegaService>();

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

builder.Services.AddScoped<IInventarioRepository, InventarioRepository>();
builder.Services.AddScoped<IInventarioService, InventarioService>();

builder.Services.AddScoped<IMovimientoRepository, MovimientoRepository>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Comandos CLI: dotnet run -- --seed | --reset | --reset-seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (args.Contains("--reset") || args.Contains("--reset-seed"))
    {
        await DatabaseResetter.ResetAsync(db);
        if (!args.Contains("--reset-seed")) return;
    }

    if (args.Contains("--seed") || args.Contains("--reset-seed"))
    {
        await InitialDataSeeder.SeedAsync(db);
        if (!args.Contains("--reset-seed")) return;
    }

    // Arranque normal: siembra solo si la BD está vacía
    await InitialDataSeeder.SeedAsync(db);
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductCatalog API v1");
    c.DocumentTitle = "ProductCatalog API";
    c.DefaultModelsExpandDepth(-1);
    c.InjectStylesheet("/swagger-custom.css");
    c.InjectJavascript("/swagger-custom.js");
});

app.MapBodegaEndpoints();
app.MapProductoEndpoints();
app.MapInventarioEndpoints();
app.MapMovimientoEndpoints();

app.Run();
