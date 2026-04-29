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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ProductCatalog API",
        Version = "v1",
        Description = "API de gestión de productos e inventario por bodega."
    });
    c.UseInlineDefinitionsForEnums();
    c.SchemaFilter<EnumSchemaFilter>();
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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductCatalog API v1");
    c.DocumentTitle = "ProductCatalog API";
    c.DefaultModelsExpandDepth(-1);
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapBodegaEndpoints();
app.MapProductoEndpoints();
app.MapInventarioEndpoints();
app.MapMovimientoEndpoints();

app.Run();
