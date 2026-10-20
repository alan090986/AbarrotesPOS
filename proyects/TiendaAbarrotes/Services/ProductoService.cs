using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;
using TiendaAbarrotes.Models;
using CommunityToolkit.Mvvm.Messaging;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.Services;

public class ProductoService
{
    // ==================== CATEGORÍAS ====================

    public List<Categoria> ListarCategorias()
    {
        using var db = new TiendaDbContext();
        return db.Categorias.Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();
    }

    public Categoria CrearCategoria(string nombre, string? descripcion = null)
    {
        using var db = new TiendaDbContext();

        if (db.Categorias.Any(c => c.Nombre == nombre))
            throw new InvalidOperationException($"La categoría '{nombre}' ya existe.");

        var cat = new Categoria { Nombre = nombre, Descripcion = descripcion };
        db.Categorias.Add(cat);
        db.SaveChanges();
        WeakReferenceMessenger.Default.Send<CatalogoCambiadoMessage>(new CatalogoCambiadoMessage());
        return cat;
    }

    // ==================== PRODUCTOS ====================

    public List<Producto> ListarProductos(string? filtro = null, int? idCategoria = null)
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

        if (idCategoria.HasValue)
            q = q.Where(p => p.IdCategoria == idCategoria.Value);

        return q.OrderBy(p => p.Nombre).ToList();
    }

    public Producto? ObtenerPorId(int id)
    {
        using var db = new TiendaDbContext();
        return db.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Stock)
            .FirstOrDefault(p => p.IdProducto == id);
    }

    public bool ExisteCodigoBarras(string codigo, int? excluirId = null)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return false;
        using var db = new TiendaDbContext();
        return db.Productos.Any(p =>
            p.CodigoBarras == codigo &&
            p.Activo &&
            (!excluirId.HasValue || p.IdProducto != excluirId.Value));
    }

    public Producto CrearProducto(Producto producto)
    {
        using var db = new TiendaDbContext();

        producto.FechaCreacion = DateTime.Now;
        producto.FechaActualizacion = DateTime.Now;
        producto.Activo = true;

        db.Productos.Add(producto);
        db.SaveChanges();

        // Crear fila de Stock en 0 si no existe
        if (!db.Stocks.Any(s => s.IdProducto == producto.IdProducto))
        {
            db.Stocks.Add(new Stock
            {
                IdProducto = producto.IdProducto,
                Cantidad = 0,
                FechaActualizacion = DateTime.Now
            });
            db.SaveChanges();

        WeakReferenceMessenger.Default.Send<CatalogoCambiadoMessage>(new CatalogoCambiadoMessage());        }

        return producto;
    }

    public void ActualizarProducto(Producto producto)
    {
        using var db = new TiendaDbContext();

        var existing = db.Productos.Find(producto.IdProducto)
            ?? throw new InvalidOperationException("Producto no encontrado.");

        existing.CodigoBarras   = producto.CodigoBarras;
        existing.Nombre         = producto.Nombre;
        existing.Descripcion    = producto.Descripcion;
        existing.IdCategoria    = producto.IdCategoria;
        existing.UnidadMedida   = producto.UnidadMedida;
        existing.PrecioCompra   = producto.PrecioCompra;
        existing.PrecioVenta    = producto.PrecioVenta;
        existing.StockMinimo    = producto.StockMinimo;
        existing.FechaActualizacion = DateTime.Now;

        db.SaveChanges();

        WeakReferenceMessenger.Default.Send<CatalogoCambiadoMessage>(new CatalogoCambiadoMessage());
    }

    public void EliminarProducto(int id)
    {
        using var db = new TiendaDbContext();
        var p = db.Productos.Find(id);
        if (p == null) return;

        // Borrado lógico: solo se desactiva para conservar historial de ventas
        p.Activo = false;
        p.FechaActualizacion = DateTime.Now;
        db.SaveChanges();

        WeakReferenceMessenger.Default.Send<CatalogoCambiadoMessage>(new CatalogoCambiadoMessage());
    }
    
}