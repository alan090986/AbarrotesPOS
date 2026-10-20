
using System.IO;
using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Models;

namespace TiendaAbarrotes.Data;

public class TiendaDbContext : DbContext
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<ProductoProveedor> ProductoProveedores => Set<ProductoProveedor>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetalleCompras => Set<DetalleCompra>();
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<ConfiguracionTienda> ConfiguracionTienda => Set<ConfiguracionTienda>();
    public DbSet<CajaDesglose> CajaDesgloses => Set<CajaDesglose>();

    public string DbPath { get; }

    public TiendaDbContext()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TiendaAbarrotes");
        Directory.CreateDirectory(folder);
        DbPath = Path.Combine(folder, "tienda.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---- Categoria ----
        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias");
            e.HasKey(x => x.IdCategoria);
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Nombre).IsUnique();
            e.Property(x => x.Activo).HasConversion<int>();
        });

        // ---- Proveedor ----
        modelBuilder.Entity<Proveedor>(e =>
        {
            e.ToTable("Proveedores");
            e.HasKey(x => x.IdProveedor);
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(150);
            e.Property(x => x.Activo).HasConversion<int>();
        });

        // ---- Usuario ----
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.Username).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Rol).IsRequired().HasMaxLength(20);
            e.Property(x => x.Activo).HasConversion<int>();
        });

        // ---- Producto ----
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("Productos");
            e.HasKey(x => x.IdProducto);
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            e.Property(x => x.CodigoBarras).HasMaxLength(50);
            e.HasIndex(x => x.CodigoBarras).IsUnique();
            e.Property(x => x.UnidadMedida).HasMaxLength(10);
            e.Property(x => x.Activo).HasConversion<int>();

            e.HasOne(x => x.Categoria)
             .WithMany(c => c.Productos)
             .HasForeignKey(x => x.IdCategoria)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProductoProveedor (N:M) ----
        modelBuilder.Entity<ProductoProveedor>(e =>
        {
            e.ToTable("ProductoProveedor");
            e.HasKey(x => new { x.IdProducto, x.IdProveedor });

            e.HasOne(x => x.Producto)
             .WithMany(p => p.Proveedores)
             .HasForeignKey(x => x.IdProducto)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Proveedor)
             .WithMany(p => p.Productos)
             .HasForeignKey(x => x.IdProveedor)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Stock ----
        modelBuilder.Entity<Stock>(e =>
        {
            e.ToTable("Stock");
            e.HasKey(x => x.IdProducto);
            e.HasOne(x => x.Producto)
             .WithOne(p => p.Stock)
             .HasForeignKey<Stock>(x => x.IdProducto)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- MovimientoStock ----
        modelBuilder.Entity<MovimientoStock>(e =>
        {
            e.ToTable("MovimientosStock");
            e.HasKey(x => x.IdMovimiento);
            e.Property(x => x.TipoMovimiento).IsRequired().HasMaxLength(10);

            e.HasOne(x => x.Producto)
             .WithMany(p => p.Movimientos)
             .HasForeignKey(x => x.IdProducto)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Usuario)
             .WithMany(u => u.Movimientos)
             .HasForeignKey(x => x.IdUsuario)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => new { x.IdProducto, x.Fecha });
        });

        // ---- Venta ----
        modelBuilder.Entity<Venta>(e =>
        {
            e.ToTable("Ventas");
            e.HasKey(x => x.IdVenta);
            e.Property(x => x.MetodoPago).IsRequired().HasMaxLength(20);
            e.Property(x => x.Estado).IsRequired().HasMaxLength(20);

            e.HasOne(x => x.Usuario)
             .WithMany(u => u.Ventas)
             .HasForeignKey(x => x.IdUsuario)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.Fecha);
        });

        // ---- DetalleVenta ----
        modelBuilder.Entity<DetalleVenta>(e =>
        {
            e.ToTable("DetalleVenta");
            e.HasKey(x => x.IdDetalleVenta);

            e.HasOne(x => x.Venta)
             .WithMany(v => v.Detalles)
             .HasForeignKey(x => x.IdVenta)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Producto)
             .WithMany(p => p.DetallesVenta)
             .HasForeignKey(x => x.IdProducto)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.IdProducto);
        });

        // ---- Compra ----
        modelBuilder.Entity<Compra>(e =>
        {
            e.ToTable("Compras");
            e.HasKey(x => x.IdCompra);
            e.Property(x => x.Estado).IsRequired().HasMaxLength(20);

            e.HasOne(x => x.Proveedor)
             .WithMany(p => p.Compras)
             .HasForeignKey(x => x.IdProveedor)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Usuario)
             .WithMany(u => u.Compras)
             .HasForeignKey(x => x.IdUsuario)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.Fecha);
        });

        // ---- DetalleCompra ----
        modelBuilder.Entity<DetalleCompra>(e =>
        {
            e.ToTable("DetalleCompra");
            e.HasKey(x => x.IdDetalleCompra);

            e.HasOne(x => x.Compra)
             .WithMany(c => c.Detalles)
             .HasForeignKey(x => x.IdCompra)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Producto)
             .WithMany(p => p.DetallesCompra)
             .HasForeignKey(x => x.IdProducto)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.IdProducto);
        });

        // ---- Caja ----
        modelBuilder.Entity<Caja>(e =>
        {
            e.ToTable("Cajas");
            e.HasKey(x => x.IdCaja);
            e.Property(x => x.Estado).IsRequired().HasMaxLength(20);

            e.HasOne(x => x.UsuarioApertura)
            .WithMany()
            .HasForeignKey(x => x.IdUsuarioApertura)
            .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.UsuarioCierre)
            .WithMany()
            .HasForeignKey(x => x.IdUsuarioCierre)
            .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.Estado);
        });

        // ---- Agregar relación Caja -> Ventas ----
        modelBuilder.Entity<Venta>(e =>
        {
            e.HasOne(x => x.Caja)
            .WithMany(c => c.Ventas)
            .HasForeignKey(x => x.IdCaja)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ConfiguracionTienda>(e =>
{
            e.ToTable("ConfiguracionTienda");
            e.HasKey(x => x.Id);
            e.Property(x => x.NombreTienda).IsRequired().HasMaxLength(150);
        });


        modelBuilder.Entity<CajaDesglose>(e =>
        {
            e.ToTable("CajaDesglose");
            e.HasKey(x => x.IdCajaDesglose);
            e.Property(x => x.Tipo).IsRequired().HasMaxLength(20);
            
            e.HasOne(x => x.Caja)
            .WithMany() // No necesitamos una colección en Caja si no la usamos
            .HasForeignKey(x => x.IdCaja)
            .OnDelete(DeleteBehavior.Cascade);
        });
    }
}