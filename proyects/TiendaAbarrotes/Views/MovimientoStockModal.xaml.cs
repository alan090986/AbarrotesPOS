using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;
using TiendaAbarrotes.Services;

namespace TiendaAbarrotes.Views;

public partial class MovimientoStockModal : Window
{
    public bool Confirmado { get; private set; }

    private readonly string _tipo;         // ENTRADA o SALIDA
    private readonly int? _idProductoPre;
    private readonly InventarioService _service = new();

    public class ProductoComboItem
    {
        public int IdProducto { get; set; }
        public string? CodigoBarras { get; set; }
        public string Nombre { get; set; } = "";
        public int StockActual { get; set; }
        public string UnidadMedida { get; set; } = "PZA";
        public string Display => string.IsNullOrEmpty(CodigoBarras) 
            ? Nombre 
            : $"{Nombre} ({CodigoBarras})";
    }

    public MovimientoStockModal(string tipo, int? idProducto = null)
    {
        InitializeComponent();
        _tipo = tipo;
        _idProductoPre = idProducto;

        // Ajustar títulos
        if (tipo == "ENTRADA")
        {
            TituloLabel.Text = "ENTRADA DE STOCK";
            SubtituloLabel.Text = "Registrar entrada";
            CantidadLabel.Text = "Cantidad a agregar *";
        }
        else
        {
            TituloLabel.Text = "SALIDA DE STOCK";
            SubtituloLabel.Text = "Registrar salida";
            CantidadLabel.Text = "Cantidad a retirar *";
        }

        CargarProductos();

        if (idProducto.HasValue)
            SeleccionarProducto(idProducto.Value);

        Loaded += (_, _) => CodigoText.Focus();
    }

    private void CargarProductos()
    {
        using var db = new TiendaDbContext();
        var items = db.Productos
            .Include(p => p.Stock)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoComboItem
            {
                IdProducto    = p.IdProducto,
                CodigoBarras  = p.CodigoBarras,
                Nombre        = p.Nombre,
                UnidadMedida  = p.UnidadMedida,
                StockActual   = p.Stock != null ? p.Stock.Cantidad : 0
            })
            .ToList();

        ProductoCombo.ItemsSource = items;

        if (items.Count > 0 && !_idProductoPre.HasValue)
            ProductoCombo.SelectedIndex = 0;
    }

    private void SeleccionarProducto(int idProducto)
    {
        if (ProductoCombo.ItemsSource is not List<ProductoComboItem> items) return;
        var match = items.FirstOrDefault(i => i.IdProducto == idProducto);
        if (match != null)
            ProductoCombo.SelectedItem = match;
    }

    private void ProductoCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ProductoCombo.SelectedItem is ProductoComboItem item)
        {
            StockActualLabel.Text = $"Stock actual: {item.StockActual} {item.UnidadMedida}";
        }
    }

    private void CodigoText_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        var codigo = CodigoText.Text?.Trim();
        if (string.IsNullOrEmpty(codigo)) return;

        if (ProductoCombo.ItemsSource is not List<ProductoComboItem> items) return;
        var match = items.FirstOrDefault(i => i.CodigoBarras == codigo);

        if (match == null)
        {
            MessageBox.Show($"No se encontró producto con código '{codigo}'.",
                            "No encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ProductoCombo.SelectedItem = match;
        CantidadText.Focus();
        CodigoText.Clear();
        e.Handled = true;
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        if (ProductoCombo.SelectedItem is not ProductoComboItem producto)
        {
            MessageBox.Show("Selecciona un producto.");
            return;
        }

        if (!int.TryParse(CantidadText.Text?.Trim(), out var cantidad) || cantidad <= 0)
        {
            MessageBox.Show("Ingresa una cantidad válida mayor a cero.");
            return;
        }

        // Firmar según el tipo
        var cantidadFirmada = _tipo == "ENTRADA" ? cantidad : -cantidad;

        try
        {
            _service.RegistrarMovimiento(
                producto.IdProducto,
                _tipo,
                cantidadFirmada,
                string.IsNullOrWhiteSpace(MotivoText.Text) ? null : MotivoText.Text.Trim(),
                string.IsNullOrWhiteSpace(ReferenciaText.Text) ? null : ReferenciaText.Text.Trim(),
                Session.IdUsuarioActual);

            Confirmado = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Confirmado = false;
        Close();
    }
}