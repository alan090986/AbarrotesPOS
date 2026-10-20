using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;
using TiendaAbarrotes.Models;
using TiendaAbarrotes.ViewModels.Modules;
using CommunityToolkit.Mvvm.Messaging;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.Services;

public class VentaService
{
    public Producto? BuscarPorCodigoBarras(string codigo)
    {
        using var db = new TiendaDbContext();
        return db.Productos
                 .Include(p => p.Stock)
                 .FirstOrDefault(p => p.CodigoBarras == codigo && p.Activo);
    }

    public int RegistrarVenta(
        IEnumerable<CartItemViewModel> items,
        string metodoPago,
        int idUsuario,
        int? idCaja)
    {
        var lista = items.ToList();
        if (lista.Count == 0)
            throw new InvalidOperationException("El carrito está vacío.");

        using var db = new TiendaDbContext();
        using var tx = db.Database.BeginTransaction();

        // 1. Cabecera de la venta
        var subtotal = lista.Sum(i => i.Subtotal);
        var venta = new Venta
        {
            Subtotal   = subtotal,
            Impuesto   = 0,
            Total      = subtotal,
            MetodoPago = metodoPago,
            IdUsuario  = idUsuario,
            IdCaja     = idCaja,          // 👈 asocia la venta al turno
            Estado     = "COMPLETADA",
            Fecha      = DateTime.Now
        };
        db.Ventas.Add(venta);
        db.SaveChanges();

        // 2. Detalles y movimientos de stock
        foreach (var item in lista)
        {
            db.DetalleVentas.Add(new DetalleVenta
            {
                IdVenta        = venta.IdVenta,
                IdProducto     = item.IdProducto,
                Cantidad       = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario,
                Descuento      = item.Descuento,
                Subtotal       = item.Subtotal
            });

            // El trigger se encarga de actualizar la tabla Stock
            db.MovimientosStock.Add(new MovimientoStock
            {
                IdProducto     = item.IdProducto,
                TipoMovimiento = "SALIDA",
                Cantidad       = -item.Cantidad,
                Motivo         = "Venta en POS",
                Referencia     = $"VENTA:{venta.IdVenta}",
                IdUsuario      = idUsuario,
                Fecha          = DateTime.Now
            });
        }

        db.SaveChanges();
        tx.Commit();

        // Notificar a todos los módulos interesados
        
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new TiendaAbarrotes.ViewModels.VentaRegistradaMessage());
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new TiendaAbarrotes.ViewModels.StockCambiadoMessage());





        return venta.IdVenta;
    }
}