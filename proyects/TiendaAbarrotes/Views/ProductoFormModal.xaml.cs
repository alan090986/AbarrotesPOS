using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaAbarrotes.Models;
using TiendaAbarrotes.Services;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.Views;

public partial class ProductoFormModal : Window
{
    public Producto ProductoResultado { get; private set; } = new();
    public bool Confirmado { get; private set; }

    private readonly int? _idProducto;
    private readonly ProductoService _service = new();

    /// <summary>Constructor para ALTA de producto.</summary>
    public ProductoFormModal()
    {
        InitializeComponent();
        _idProducto = null;
        CargarCategorias();
        SeleccionarUnidadPorDefecto();
        Loaded += (_, _) => CodigoText.Focus();
    }

    /// <summary>Constructor para EDICIÓN.</summary>
    public ProductoFormModal(int idProducto)
    {
        InitializeComponent();
        _idProducto = idProducto;

        TituloLabel.Text = "EDITAR PRODUCTO";
        SubtituloLabel.Text = "Editar producto";

        CargarCategorias();
        CargarDatos();
    }

    private void CargarCategorias()
    {
        CategoriaCombo.Items.Clear();
        foreach (var c in _service.ListarCategorias())
        {
            CategoriaCombo.Items.Add(new ComboBoxItem
            {
                Content = c.Nombre,
                Tag = c.IdCategoria
            });
        }
        if (CategoriaCombo.Items.Count > 0)
            CategoriaCombo.SelectedIndex = 0;
    }

    private void SeleccionarUnidadPorDefecto()
    {
        foreach (ComboBoxItem item in UnidadCombo.Items)
        {
            if (item.Tag?.ToString() == "PZA")
            {
                UnidadCombo.SelectedItem = item;
                break;
            }
        }
    }

    private void CargarDatos()
    {
        var p = _service.ObtenerPorId(_idProducto!.Value);
        if (p == null) return;

        CodigoText.Text        = p.CodigoBarras ?? "";
        NombreText.Text        = p.Nombre;
        DescripcionText.Text   = p.Descripcion ?? "";
        PrecioCompraText.Text  = (p.PrecioCompra / 100m).ToString("0.00");
        PrecioVentaText.Text   = (p.PrecioVenta / 100m).ToString("0.00");
        StockMinimoText.Text   = p.StockMinimo.ToString();

        // Categoría
        foreach (ComboBoxItem item in CategoriaCombo.Items)
        {
            if ((int?)item.Tag == p.IdCategoria)
            {
                CategoriaCombo.SelectedItem = item;
                break;
            }
        }

        // Unidad
        foreach (ComboBoxItem item in UnidadCombo.Items)
        {
            if (item.Tag?.ToString() == p.UnidadMedida)
            {
                UnidadCombo.SelectedItem = item;
                break;
            }
        }
    }

    private void CodigoText_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        var codigo = CodigoText.Text?.Trim();
        if (string.IsNullOrEmpty(codigo)) return;

        if (_service.ExisteCodigoBarras(codigo, _idProducto))
        {
            MessageBox.Show($"El código '{codigo}' ya está asignado a otro producto.",
                            "Código duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
            CodigoText.SelectAll();
            return;
        }

        // Si pasa la validación, saltar al siguiente campo
        NombreText.Focus();
        e.Handled = true;
    }

    private void PrecioText_TextChanged(object sender, TextChangedEventArgs e)
        => RecalcularMargen();

    private void RecalcularMargen()
    {
        if (MargenText == null) return;

        var compra = ParsearPesos(PrecioCompraText.Text);
        var venta  = ParsearPesos(PrecioVentaText.Text);

        if (compra <= 0 || venta <= 0)
        {
            MargenText.Text = "0%";
            MargenText.Foreground = (System.Windows.Media.Brush)FindResource("BrushTextoSecundario");
            return;
        }

        var margen = ((venta - compra) / (decimal)compra) * 100;

        MargenText.Text = $"{margen:N1}%";

        if (margen < 0)
            MargenText.Foreground = (System.Windows.Media.Brush)FindResource("BrushPeligro");
        else if (margen < 10)
            MargenText.Foreground = (System.Windows.Media.Brush)FindResource("BrushTextoSecundario");
        else
            MargenText.Foreground = (System.Windows.Media.Brush)FindResource("BrushExito");
    }

    private int ParsearPesos(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return 0;
        var limpio = texto.Replace("$", "").Replace(",", "").Trim();
        if (!decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            return 0;
        return (int)Math.Round(d * 100);
    }

    private void NuevaCategoria_Click(object sender, RoutedEventArgs e)
    {
        var modal = new CategoriaModal { Owner = this };
        modal.ShowDialog();

        if (modal.Confirmado)
        {
            CargarCategorias();
            // Seleccionar la categoría recién creada
            foreach (ComboBoxItem item in CategoriaCombo.Items)
            {
                if ((int?)item.Tag == modal.IdCategoriaCreada)
                {
                    CategoriaCombo.SelectedItem = item;
                    break;
                }
            }
        }
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(NombreText.Text))
        {
            MessageBox.Show("El nombre es obligatorio.");
            return;
        }
        if (CategoriaCombo.SelectedItem is not ComboBoxItem catItem)
        {
            MessageBox.Show("Selecciona una categoría.");
            return;
        }
        if (UnidadCombo.SelectedItem is not ComboBoxItem unidadItem)
        {
            MessageBox.Show("Selecciona una unidad de medida.");
            return;
        }

        var precioCompra = ParsearPesos(PrecioCompraText.Text);
        var precioVenta  = ParsearPesos(PrecioVentaText.Text);

        if (precioVenta <= 0)
        {
            MessageBox.Show("El precio de venta debe ser mayor a 0.");
            return;
        }

        var codigoBarras = string.IsNullOrWhiteSpace(CodigoText.Text) ? null : CodigoText.Text.Trim();

        // Validar código duplicado al guardar
        if (codigoBarras != null && _service.ExisteCodigoBarras(codigoBarras, _idProducto))
        {
            MessageBox.Show($"El código '{codigoBarras}' ya está asignado a otro producto.");
            return;
        }

        int stockMin = 0;
        int.TryParse(StockMinimoText.Text?.Trim(), out stockMin);

        ProductoResultado = new Producto
        {
            IdProducto    = _idProducto ?? 0,
            CodigoBarras  = codigoBarras,
            Nombre        = NombreText.Text.Trim(),
            Descripcion   = string.IsNullOrWhiteSpace(DescripcionText.Text) ? null : DescripcionText.Text.Trim(),
            IdCategoria   = (int)(catItem.Tag ?? 0),
            UnidadMedida  = unidadItem.Tag?.ToString() ?? "PZA",
            PrecioCompra  = precioCompra,
            PrecioVenta   = precioVenta,
            StockMinimo   = stockMin
        };

        Confirmado = true;
        Close();
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Confirmado = false;
        Close();
    }
}