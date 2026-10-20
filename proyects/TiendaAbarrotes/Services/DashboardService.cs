using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;

namespace TiendaAbarrotes.Services;

public class DashboardService
{
    // ==================== KPIs ====================

    /// <summary>Suma de ventas del día actual (completadas).</summary>
    public int VentasDelDia()
    {
        using var db = new TiendaDbContext();
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);

        return db.Ventas
            .Where(v => v.Estado == "COMPLETADA" && v.Fecha >= hoy && v.Fecha < manana)
            .Sum(v => (int?)v.Total) ?? 0;
    }

    /// <summary>Número de ventas del día.</summary>
    public int NumVentasDelDia()
    {
        using var db = new TiendaDbContext();
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);

        return db.Ventas
            .Count(v => v.Estado == "COMPLETADA" && v.Fecha >= hoy && v.Fecha < manana);
    }

    /// <summary>Total de productos activos.</summary>
    public int TotalProductos()
    {
        using var db = new TiendaDbContext();
        return db.Productos.Count(p => p.Activo);
    }

    /// <summary>Cantidad de productos con stock por debajo del mínimo.</summary>
    public int ProductosStockBajo()
    {
        using var db = new TiendaDbContext();
        return db.Stocks
            .Join(db.Productos, s => s.IdProducto, p => p.IdProducto, (s, p) => new { s, p })
            .Count(x => x.p.Activo && x.s.Cantidad <= x.p.StockMinimo);
    }

    /// <summary>Ganancia estimada del mes actual (ventas - costo).</summary>
    public int GananciaDelMes()
    {
        using var db = new TiendaDbContext();
        var inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var fin = inicio.AddMonths(1);

        // Suma de (precio_venta - precio_compra) * cantidad de cada detalle del mes
        var detalles = db.DetalleVentas
            .Join(db.Ventas, d => d.IdVenta, v => v.IdVenta, (d, v) => new { d, v })
            .Join(db.Productos, x => x.d.IdProducto, p => p.IdProducto, (x, p) => new
            {
                x.d.Cantidad,
                x.d.PrecioUnitario,
                x.d.Descuento,
                p.PrecioCompra,
                x.v.Fecha,
                x.v.Estado
            })
            .Where(x => x.Estado == "COMPLETADA" && x.Fecha >= inicio && x.Fecha < fin)
            .ToList();

        return detalles.Sum(x =>
            ((x.PrecioUnitario - x.PrecioCompra) * x.Cantidad) - x.Descuento);
    }

    /// <summary>Total de ventas del mes.</summary>
    public int VentasDelMes()
    {
        using var db = new TiendaDbContext();
        var inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var fin = inicio.AddMonths(1);

        return db.Ventas
            .Where(v => v.Estado == "COMPLETADA" && v.Fecha >= inicio && v.Fecha < fin)
            .Sum(v => (int?)v.Total) ?? 0;
    }

    // ==================== Gráfico ====================

    /// <summary>Ventas agrupadas por día de los últimos 30 días.</summary>
    public List<(DateTime Dia, int Total)> VentasUltimos30Dias()
    {
        using var db = new TiendaDbContext();
        var desde = DateTime.Today.AddDays(-29);
        var hasta = DateTime.Today.AddDays(1);

        var datos = db.Ventas
            .Where(v => v.Estado == "COMPLETADA" && v.Fecha >= desde && v.Fecha < hasta)
            .GroupBy(v => v.Fecha.Date)
            .Select(g => new { Dia = g.Key, Total = g.Sum(v => v.Total) })
            .OrderBy(x => x.Dia)
            .ToList();

        // Rellenar días sin ventas con 0
        var resultado = new List<(DateTime, int)>();
        for (int i = 0; i < 30; i++)
        {
            var dia = desde.AddDays(i);
            var match = datos.FirstOrDefault(d => d.Dia == dia);
            resultado.Add((dia, match?.Total ?? 0));
        }
        return resultado;
    }

    /// <summary>Ventas agrupadas por mes de los últimos 12 meses.</summary>
    public List<(DateTime Mes, int Total)> VentasUltimos12Meses()
    {
        using var db = new TiendaDbContext();
        var primerMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11);

        var datos = db.Ventas
            .Where(v => v.Estado == "COMPLETADA" && v.Fecha >= primerMes)
            .GroupBy(v => new { v.Fecha.Year, v.Fecha.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(v => v.Total)
            })
            .ToList();

        var resultado = new List<(DateTime, int)>();
        for (int i = 0; i < 12; i++)
        {
            var mes = primerMes.AddMonths(i);
            var match = datos.FirstOrDefault(d => d.Year == mes.Year && d.Month == mes.Month);
            resultado.Add((mes, match?.Total ?? 0));
        }
        return resultado;
    }

    // ==================== Top productos ====================

    public class TopProducto
    {
        public string Nombre { get; set; } = "";
        public int CantidadVendida { get; set; }
        public int TotalVendido { get; set; }
    }

    public List<TopProducto> Top5Productos(int dias = 30)
    {
        using var db = new TiendaDbContext();
        var desde = DateTime.Today.AddDays(-dias);

        return db.DetalleVentas
            .Join(db.Ventas, d => d.IdVenta, v => v.IdVenta, (d, v) => new { d, v })
            .Join(db.Productos, x => x.d.IdProducto, p => p.IdProducto, (x, p) => new
            {
                x.d.Cantidad,
                x.d.Subtotal,
                x.v.Fecha,
                x.v.Estado,
                p.Nombre
            })
            .Where(x => x.Estado == "COMPLETADA" && x.Fecha >= desde)
            .GroupBy(x => x.Nombre)
            .Select(g => new TopProducto
            {
                Nombre = g.Key,
                CantidadVendida = g.Sum(x => x.Cantidad),
                TotalVendido = g.Sum(x => x.Subtotal)
            })
            .OrderByDescending(x => x.CantidadVendida)
            .Take(5)
            .ToList();
    }
}