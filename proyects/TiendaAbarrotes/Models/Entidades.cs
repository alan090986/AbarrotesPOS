namespace TiendaAbarrotes.Models;

public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

public class Proveedor
{
    public int IdProveedor { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Rfc { get; set; }
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<ProductoProveedor> Productos { get; set; } = new List<ProductoProveedor>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Rol { get; set; } = "CAJERO";
    public bool Activo { get; set; } = true;
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<MovimientoStock> Movimientos { get; set; } = new List<MovimientoStock>();
    public DateTime? UltimoAcceso { get; set; }
}

public class Producto
{
    public int IdProducto { get; set; }
    public string? CodigoBarras { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int IdCategoria { get; set; }
    public string UnidadMedida { get; set; } = "PZA";
    public int PrecioCompra { get; set; }
    public int PrecioVenta { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime FechaActualizacion { get; set; } = DateTime.Now;

    public Categoria Categoria { get; set; } = null!;
    public Stock? Stock { get; set; }
    public ICollection<ProductoProveedor> Proveedores { get; set; } = new List<ProductoProveedor>();
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
    public ICollection<MovimientoStock> Movimientos { get; set; } = new List<MovimientoStock>();
}

public class ProductoProveedor
{
    public int IdProducto { get; set; }
    public int IdProveedor { get; set; }
    public string? CodigoProveedor { get; set; }
    public int? PrecioCompra { get; set; }
    public Producto Producto { get; set; } = null!;
    public Proveedor Proveedor { get; set; } = null!;
}

public class Stock
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    public Producto Producto { get; set; } = null!;
}

public class MovimientoStock
{
    public int IdMovimiento { get; set; }
    public int IdProducto { get; set; }
    public string TipoMovimiento { get; set; } = null!;
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public string? Referencia { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int? IdUsuario { get; set; }
    public Producto Producto { get; set; } = null!;
    public Usuario? Usuario { get; set; }
}

public class Venta
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int Subtotal { get; set; }
    public int Impuesto { get; set; }
    public int Total { get; set; }
    public string MetodoPago { get; set; } = "EFECTIVO";
    public int IdUsuario { get; set; }
    public string Estado { get; set; } = "COMPLETADA";
    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    public int? IdCaja { get; set; }         
    public Caja? Caja { get; set; }
}

public class DetalleVenta
{
    public int IdDetalleVenta { get; set; }
    public int IdVenta { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public int PrecioUnitario { get; set; }
    public int Descuento { get; set; }
    public int Subtotal { get; set; }
    public Venta Venta { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}

public class Compra
{
    public int IdCompra { get; set; }
    public int IdProveedor { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int Total { get; set; }
    public int IdUsuario { get; set; }
    public string Estado { get; set; } = "RECIBIDA";
    public Proveedor Proveedor { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}

public class DetalleCompra
{
    public int IdDetalleCompra { get; set; }
    public int IdCompra { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public int PrecioUnitario { get; set; }
    public int Subtotal { get; set; }
    public Compra Compra { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}

public class Caja
{
    public int IdCaja { get; set; }
    public DateTime FechaApertura { get; set; } = DateTime.Now;
    public DateTime? FechaCierre { get; set; }
    public int MontoInicial { get; set; }
    public int TotalVentasEfectivo { get; set; }
    public int TotalVentasTarjeta { get; set; }
    public int TotalVentasTransferencia { get; set; }
    public int MontoEsperado { get; set; }
    public int? MontoReal { get; set; }
    public int? Diferencia { get; set; }
    public int IdUsuarioApertura { get; set; }
    public int? IdUsuarioCierre { get; set; }
    public string Estado { get; set; } = "ABIERTA";
    public string? Observaciones { get; set; }

    public Usuario UsuarioApertura { get; set; } = null!;
    public Usuario? UsuarioCierre { get; set; }
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

public class ConfiguracionTienda
{
    public int Id { get; set; } = 1;   // siempre será 1 (fila única)
    public string NombreTienda { get; set; } = "Mi Tienda";
    public string? Rfc { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateTime FechaInstalacion { get; set; } = DateTime.Now;
}

public class CajaDesglose
{
    public int IdCajaDesglose { get; set; }
    public int IdCaja { get; set; }
    public string Tipo { get; set; } = ""; // "APERTURA" o "CIERRE"
    public decimal Denominacion { get; set; } // 1000, 500, 0.5
    public int Cantidad { get; set; }

    public Caja Caja { get; set; } = null!;
}