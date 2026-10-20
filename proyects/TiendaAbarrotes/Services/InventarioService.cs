using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;
using TiendaAbarrotes.Models;
using CommunityToolkit.Mvvm.Messaging;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.Services;

public class InventarioService
{
    // ==================== CONSULTAS ====================

    public class StockItem
    {
        public int IdProducto { get; set; }
        public string? CodigoBarras { get; set; }
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string UnidadMedida { get; set; } = "PZA";
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int PrecioCompra { get; set; }
        public int PrecioVenta { get; set; }

        public string StockTexto => $"{StockActual} {UnidadMedida}";
        public string StockMinimoTexto => $"{StockMinimo} {UnidadMedida}";
        public bool StockBajo => StockActual <= StockMinimo;
        public int ValorInventarioCentavos => StockActual * PrecioCompra;
        public string ValorInventarioTexto => (ValorInventarioCentavos / 100m).ToString("C2");
    }

    public class MovimientoItem
    {
        public int IdMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreProducto { get; set; } = "";
        public string TipoMovimiento { get; set; } = "";
        public int Cantidad { get; set; }
        public string? Motivo { get; set; }
        public string? Referencia { get; set; }
        public string? Usuario { get; set; }

        public string FechaTexto => Fecha.ToString("dd/MM/yyyy HH:mm");
        public string CantidadTexto => Cantidad > 0 ? $"+{Cantidad}" : Cantidad.ToString();
        public string TipoTexto => TipoMovimiento switch
        {
            "ENTRADA" => "Entrada",
            "SALIDA"  => "Salida",
            "AJUSTE"  => "Ajuste",
            _         => TipoMovimiento
        };
    }

    public List<StockItem> ListarStock(string? filtro = null)
    {
        using var db = new TiendaDbContext();

        var q = db.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Stock)
            .Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var f = filtro.ToLower();
            q = q.Where(p =>
                p.Nombre.ToLower().Contains(f) ||
                (p.CodigoBarras != null && p.CodigoBarras.Contains(f)));
        }

        return q.OrderBy(p => p.Nombre)
            .Select(p => new StockItem
            {
                IdProducto      = p.IdProducto,
                CodigoBarras    = p.CodigoBarras,
                Nombre          = p.Nombre,
                Categoria       = p.Categoria.Nombre,
                UnidadMedida    = p.UnidadMedida,
                PrecioCompra    = p.PrecioCompra,
                PrecioVenta     = p.PrecioVenta,
                StockMinimo     = p.StockMinimo,
                StockActual     = p.Stock != null ? p.Stock.Cantidad : 0
            })
            .ToList();
    }

    public List<MovimientoItem> ListarMovimientos(string? filtro = null, string? tipo = null, int top = 200)
    {
        using var db = new TiendaDbContext();

        var q = db.MovimientosStock
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var f = filtro.ToLower();
            q = q.Where(m => m.Producto.Nombre.ToLower().Contains(f) ||
                             (m.Producto.CodigoBarras != null && m.Producto.CodigoBarras.Contains(f)));
        }

        if (!string.IsNullOrWhiteSpace(tipo) && tipo != "TODOS")
            q = q.Where(m => m.TipoMovimiento == tipo);

        return q.OrderByDescending(m => m.Fecha)
            .Take(top)
            .Select(m => new MovimientoItem
            {
                IdMovimiento    = m.IdMovimiento,
                Fecha           = m.Fecha,
                NombreProducto  = m.Producto.Nombre,
                TipoMovimiento  = m.TipoMovimiento,
                Cantidad        = m.Cantidad,
                Motivo          = m.Motivo,
                Referencia      = m.Referencia,
                Usuario         = m.Usuario != null ? m.Usuario.Nombre : null
            })
            .ToList();
    }

    // ==================== KPIs ====================

    public int TotalSKUs()
    {
        using var db = new TiendaDbContext();
        return db.Productos.Count(p => p.Activo);
    }

    public int ValorInventarioCentavos()
    {
        using var db = new TiendaDbContext();
        return db.Productos
            .Include(p => p.Stock)
            .Where(p => p.Activo)
            .Sum(p => (int?) ((p.Stock != null ? p.Stock.Cantidad : 0) * p.PrecioCompra)) ?? 0;
    }

    public int ProductosStockBajo()
    {
        using var db = new TiendaDbContext();
        return db.Productos
            .Include(p => p.Stock)
            .Count(p => p.Activo && (p.Stock == null || p.Stock.Cantidad <= p.StockMinimo));
    }

    public int MovimientosHoy()
    {
        using var db = new TiendaDbContext();
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);
        return db.MovimientosStock.Count(m => m.Fecha >= hoy && m.Fecha < manana);
    }

    // ==================== REGISTRAR MOVIMIENTO ====================

    /// <summary>Registra un movimiento. Cantidad positiva = entrada, negativa = salida.</summary>
    public void RegistrarMovimiento(
        int idProducto,
        string tipo,
        int cantidad,
        string? motivo,
        string? referencia,
        int idUsuario)
    {
        if (cantidad == 0)
            throw new InvalidOperationException("La cantidad no puede ser cero.");

        using var db = new TiendaDbContext();

        var producto = db.Productos
            .Include(p => p.Stock)
            .FirstOrDefault(p => p.IdProducto == idProducto)
            ?? throw new InvalidOperationException("Producto no encontrado.");

        // Si es SALIDA y no hay suficiente stock, advertir
        if (tipo == "SALIDA")
        {
            var actual = producto.Stock?.Cantidad ?? 0;
            if (Math.Abs(cantidad) > actual)
                throw new InvalidOperationException(
                    $"No hay suficiente stock. Disponible: {actual}, solicitado: {Math.Abs(cantidad)}.");
        }

        // El trigger se encarga de actualizar la tabla Stock
        db.MovimientosStock.Add(new MovimientoStock
        {
            IdProducto     = idProducto,
            TipoMovimiento = tipo,
            Cantidad       = cantidad,       // Positivo entrada, negativo salida
            Motivo         = motivo,
            Referencia     = referencia,
            IdUsuario      = idUsuario,
            Fecha          = DateTime.Now
        });

        db.SaveChanges();

        // En InventarioService.cs, justo después del db.SaveChanges();
    WeakReferenceMessenger.Default.Send<StockCambiadoMessage>(new StockCambiadoMessage());
    }
}