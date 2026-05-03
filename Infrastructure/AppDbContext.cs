using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Inventario> Inventarios => Set<Inventario>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Bodega>(e =>
        {
            e.ToTable("bodegas");
            e.HasKey(x => x.BodId);
            e.Property(x => x.BodId).HasColumnName("bod_id").UseIdentityColumn();
            e.Property(x => x.BodNombre).HasColumnName("bod_nombre").HasMaxLength(100).IsRequired();
            e.Property(x => x.BodPrincipal).HasColumnName("bod_principal").HasDefaultValue(false);
        });

        model.Entity<Producto>(e =>
        {
            e.ToTable("productos");
            e.HasKey(x => x.ProdId);
            e.Property(x => x.ProdId).HasColumnName("prod_id").UseIdentityColumn();
            e.Property(x => x.ProdNombre).HasColumnName("prod_nombre").HasMaxLength(200).IsRequired();
            e.Property(x => x.ProdCodigo).HasColumnName("prod_codigo").HasMaxLength(50).IsRequired();
            e.Property(x => x.ProdDescripcion).HasColumnName("prod_descripcion");
            e.Property(x => x.ProdMarca).HasColumnName("prod_marca").HasMaxLength(100);
            e.HasIndex(x => x.ProdCodigo).IsUnique();
        });

        model.Entity<Inventario>(e =>
        {
            e.ToTable("inventario");
            e.HasKey(x => new { x.ProdId, x.BodId });
            e.Property(x => x.ProdId).HasColumnName("prod_id");
            e.Property(x => x.BodId).HasColumnName("bod_id");
            e.Property(x => x.InvStock).HasColumnName("inv_stock").HasDefaultValue(0);
            e.Property(x => x.InvLastUpdate).HasColumnName("inv_last_update");
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProdId);
            e.HasOne(x => x.Bodega).WithMany().HasForeignKey(x => x.BodId);
        });

        model.Entity<Movimiento>(e =>
        {
            e.ToTable("movimientos");
            e.HasKey(x => x.MovId);
            e.Property(x => x.MovId).HasColumnName("mov_id").UseIdentityColumn();
            e.Property(x => x.ProdId).HasColumnName("prod_id");
            e.Property(x => x.MovBodegaInicial).HasColumnName("mov_bodega_inicial");
            e.Property(x => x.MovBodegaFinal).HasColumnName("mov_bodega_final");
            e.Property(x => x.MovCantidad).HasColumnName("mov_cantidad");
            e.Property(x => x.MovTipo).HasColumnName("mov_tipo").HasConversion<string>();
            e.Property(x => x.MovConcepto).HasColumnName("mov_concepto").HasConversion<string>();
            e.Property(x => x.MovFecha).HasColumnName("mov_fecha");
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProdId);
        });

        model.Entity<Cliente>(e =>
        {
            e.ToTable("clientes");
            e.HasKey(x => x.CliId);
            e.Property(x => x.CliId).HasColumnName("cli_id").UseIdentityColumn();
            e.Property(x => x.CliNombre).HasColumnName("cli_nombre").HasMaxLength(100).IsRequired();
            e.Property(x => x.CliApellido).HasColumnName("cli_apellido").HasMaxLength(100).IsRequired();
            e.Property(x => x.CliCorreo).HasColumnName("cli_correo").HasMaxLength(150);
            e.Property(x => x.CliCelular).HasColumnName("cli_celular").HasMaxLength(20);
            e.Property(x => x.CliDireccion).HasColumnName("cli_direccion").HasMaxLength(200);
        });
    }
}
