using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Models;
using TiendaAbarrotes.Utils;

namespace TiendaAbarrotes.Data;

public static class DatabaseInitializer
{
    /// <summary>Crea la BD, triggers e índices si no existen.</summary>
    public static void Initialize(TiendaDbContext db)
    {
        db.Database.EnsureCreated();

        // Trigger: mantener Stock sincronizado con MovimientosStock
        db.Database.ExecuteSqlRaw("""
            CREATE TRIGGER IF NOT EXISTS trg_mov_stock_after_insert
            AFTER INSERT ON MovimientosStock
            BEGIN
                INSERT OR IGNORE INTO Stock (IdProducto, Cantidad, FechaActualizacion)
                VALUES (NEW.IdProducto, 0, datetime('now'));

                UPDATE Stock
                SET Cantidad = Cantidad + NEW.Cantidad,
                    FechaActualizacion = datetime('now')
                WHERE IdProducto = NEW.IdProducto;
            END;
            """);

        // Vista: ventas diarias
        db.Database.ExecuteSqlRaw("""
            CREATE VIEW IF NOT EXISTS v_ventas_diarias AS
            SELECT date(Fecha) AS Dia,
                   COUNT(*)   AS NumVentas,
                   SUM(Total) AS TotalVentas
            FROM Ventas
            WHERE Estado = 'COMPLETADA'
            GROUP BY date(Fecha);
            """);

        // Vista: productos más vendidos
        db.Database.ExecuteSqlRaw("""
            CREATE VIEW IF NOT EXISTS v_productos_mas_vendidos AS
            SELECT p.IdProducto,
                   p.Nombre,
                   SUM(dv.Cantidad) AS TotalVendido
            FROM DetalleVenta dv
            JOIN Productos p ON p.IdProducto = dv.IdProducto
            JOIN Ventas v ON v.IdVenta = dv.IdVenta
            WHERE v.Estado = 'COMPLETADA'
            GROUP BY p.IdProducto, p.Nombre
            ORDER BY TotalVendido DESC;
            """);

        // Vista: stock bajo
        db.Database.ExecuteSqlRaw("""
            CREATE VIEW IF NOT EXISTS v_stock_bajo AS
            SELECT p.IdProducto,
                   p.Nombre,
                   s.Cantidad,
                   p.StockMinimo
            FROM Stock s
            JOIN Productos p ON p.IdProducto = s.IdProducto
            WHERE s.Cantidad <= p.StockMinimo AND p.Activo = 1;
            """);
    }

    /// <summary>Inserta datos de prueba (solo si la BD está vacía).</summary>
    public static void SeedTestData(TiendaDbContext db)
    {
        if (db.Productos.Count() > 1) return; // ya hay datos

        // Usuario admin
        if (!db.Usuarios.Any())
        {
            db.Usuarios.Add(new Usuario
            {
                Nombre       = "Alan Villegas",
                Username     = "admin",
                PasswordHash = PasswordHasher.Hash("admin123"),
                Rol          = "ADMIN"
            });
            db.SaveChanges();
        }

        // Categorías
        if (!db.Categorias.Any())
        {
            db.Categorias.AddRange(
                new Categoria { Nombre = "Bebidas" },
                new Categoria { Nombre = "Abarrotes" },
                new Categoria { Nombre = "Lácteos" },
                new Categoria { Nombre = "Botanas" },
                new Categoria { Nombre = "Limpieza" }
            );
            db.SaveChanges();
        }

        // Productos
        if (db.Productos.Count() <= 1)
        {
            var catBebidas   = db.Categorias.First(c => c.Nombre == "Bebidas").IdCategoria;
            var catAbarrotes = db.Categorias.First(c => c.Nombre == "Abarrotes").IdCategoria;
            var catLacteos   = db.Categorias.First(c => c.Nombre == "Lácteos").IdCategoria;
            var catBotanas   = db.Categorias.First(c => c.Nombre == "Botanas").IdCategoria;
            var catLimpieza  = db.Categorias.First(c => c.Nombre == "Limpieza").IdCategoria;

            var productos = new List<Producto>
            {
                new() { CodigoBarras = "7501055363513", Nombre = "Coca-Cola 600ml",        IdCategoria = catBebidas,   PrecioCompra = 1200, PrecioVenta = 1800, StockMinimo = 10 },
                new() { CodigoBarras = "7501055310807", Nombre = "Pepsi 600ml",            IdCategoria = catBebidas,   PrecioCompra = 1100, PrecioVenta = 1700, StockMinimo = 10 },
                new() { CodigoBarras = "7501011112357", Nombre = "Agua Bonafont 1L",       IdCategoria = catBebidas,   PrecioCompra =  800, PrecioVenta = 1400, StockMinimo = 15 },
                new() { CodigoBarras = "7501000110111", Nombre = "Sabritas Original 45g",  IdCategoria = catBotanas,   PrecioCompra =  900, PrecioVenta = 1600, StockMinimo = 20 },
                new() { CodigoBarras = "7501000121112", Nombre = "Doritos Nacho 45g",      IdCategoria = catBotanas,   PrecioCompra =  900, PrecioVenta = 1600, StockMinimo = 20 },
                new() { CodigoBarras = "7501030490367", Nombre = "Leche Lala 1L",          IdCategoria = catLacteos,   PrecioCompra = 1800, PrecioVenta = 2400, StockMinimo = 10 },
                new() { CodigoBarras = "7501020512345", Nombre = "Yoghurt Danone 900g",    IdCategoria = catLacteos,   PrecioCompra = 2500, PrecioVenta = 3400, StockMinimo =  8 },
                new() { CodigoBarras = "7501035900011", Nombre = "Frijol Isadora 1kg",     IdCategoria = catAbarrotes, PrecioCompra = 2200, PrecioVenta = 3000, StockMinimo = 15 },
                new() { CodigoBarras = "7501035900028", Nombre = "Arroz Verde Valle 1kg",  IdCategoria = catAbarrotes, PrecioCompra = 2000, PrecioVenta = 2800, StockMinimo = 15 },
                new() { CodigoBarras = "7501035900035", Nombre = "Aceite Nutrioli 900ml",  IdCategoria = catAbarrotes, PrecioCompra = 3500, PrecioVenta = 4500, StockMinimo = 10 },
                new() { CodigoBarras = "7501025400011", Nombre = "Jabón Zote 200g",        IdCategoria = catLimpieza,  PrecioCompra =  900, PrecioVenta = 1500, StockMinimo = 20 },
                new() { CodigoBarras = "7501025400028", Nombre = "Cloro Cloralex 1L",      IdCategoria = catLimpieza,  PrecioCompra = 1500, PrecioVenta = 2200, StockMinimo = 10 },
            };

            db.Productos.AddRange(productos);
            db.SaveChanges();

            // Entrada inicial de stock (el trigger actualiza la tabla Stock)
            foreach (var p in productos)
            {
                db.MovimientosStock.Add(new MovimientoStock
                {
                    IdProducto     = p.IdProducto,
                    TipoMovimiento = "ENTRADA",
                    Cantidad       = 50,
                    Motivo         = "Carga inicial",
                    IdUsuario      = 1
                });
            }
            db.SaveChanges();
        }
    }
}