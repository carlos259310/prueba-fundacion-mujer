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
            API REST para gestión de catálogo de productos e inventario por bodega.

            ## Flujo de uso recomendado
            1. Crear una **Bodega** (`POST /api/bodegas`)
            2. Crear un **Producto** (`POST /api/productos`)
            3. Registrar una **Entrada** de stock (`POST /api/movimientos`)
            4. Consultar el **stock** por bodega (`GET /api/productos/{id}/stock`)
            5. Registrar **Salidas** o **Traslados** según necesidad

            ## Reglas de negocio
            - El stock **nunca puede ser negativo** → `400 Bad Request`
            - El código de producto debe ser **único** → `409 Conflict`
            - Los traslados requieren bodega origen y destino distintas

            ## Enums disponibles
            **TipoMovimiento:** `Entrada` · `Salida` · `Traslado`

            **ConceptoMovimiento:** `Compra` · `Venta` · `Ajuste` · `Traslado` · `Devolucion`
            """
    });
    c.UseInlineDefinitionsForEnums();
    c.SchemaFilter<EnumSchemaFilter>();
    c.TagActionsBy(api => api.GroupName != null ? [api.GroupName] : api.ActionDescriptor.RouteValues.ContainsKey("controller") ? [api.ActionDescriptor.RouteValues["controller"]] : ["General"]);
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
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductCatalog API v1");
    c.DocumentTitle = "ProductCatalog API";
    c.DefaultModelsExpandDepth(-1);
    c.InjectStylesheet("/swagger-custom.css");
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapBodegaEndpoints();
app.MapProductoEndpoints();
app.MapInventarioEndpoints();
app.MapMovimientoEndpoints();

app.Run();
