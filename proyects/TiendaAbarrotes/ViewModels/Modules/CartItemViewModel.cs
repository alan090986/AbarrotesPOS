using CommunityToolkit.Mvvm.ComponentModel;

namespace TiendaAbarrotes.ViewModels.Modules;

public partial class CartItemViewModel : ObservableObject
{
    public int IdProducto { get; set; }
    public string? CodigoBarras { get; set; }
    public string Nombre { get; set; } = "";
    public int PrecioUnitario { get; set; } // centavos

    [ObservableProperty]
    private int _cantidad = 1;

    [ObservableProperty]
    private int _descuento = 0;

    public int Subtotal => (PrecioUnitario * Cantidad) - Descuento;

    // Propiedades ya formateadas para mostrar en el DataGrid
    public string PrecioUnitarioFormateado => (PrecioUnitario / 100m).ToString("C2");
    public string SubtotalFormateado       => (Subtotal       / 100m).ToString("C2");

    partial void OnCantidadChanged(int value)
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(SubtotalFormateado));
    }

    partial void OnDescuentoChanged(int value)
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(SubtotalFormateado));
    }
}