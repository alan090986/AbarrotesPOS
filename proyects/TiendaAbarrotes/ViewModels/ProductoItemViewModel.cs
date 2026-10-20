using TiendaAbarrotes.Models;

namespace TiendaAbarrotes.ViewModels;

public class ProductoItemViewModel
{
    public int IdProducto { get; set; }
    public string? CodigoBarras { get; set; }
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public int PrecioCompra { get; set; }
    public int PrecioVenta { get; set; }
    public int StockMinimo { get; set; }
    public int StockActual { get; set; }
    public string UnidadMedida { get; set; } = "PZA";

    public string PrecioVentaTexto => (PrecioVenta / 100m).ToString("C2");
    public string PrecioCompraTexto => (PrecioCompra / 100m).ToString("C2");

    public string StockTexto => $"{StockActual} {UnidadMedida}";
    public string StockMinimoTexto => $"{StockMinimo} {UnidadMedida}";

    public bool StockBajo => StockActual <= StockMinimo;

    public static ProductoItemViewModel FromModel(Producto p) => new()
    {
        IdProducto      = p.IdProducto,
        CodigoBarras    = p.CodigoBarras,
        Nombre          = p.Nombre,
        Categoria       = p.Categoria?.Nombre ?? "",
        PrecioCompra    = p.PrecioCompra,
        PrecioVenta     = p.PrecioVenta,
        StockMinimo     = p.StockMinimo,
        UnidadMedida    = p.UnidadMedida,
        StockActual     = p.Stock?.Cantidad ?? 0
    };
}