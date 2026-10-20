using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TiendaAbarrotes.Services;
using TiendaAbarrotes.Views;

namespace TiendaAbarrotes.ViewModels.Modules;

public partial class ProductosViewModel : ObservableObject, IRefreshable, IRecipient<CatalogoCambiadoMessage>
{
    private readonly ProductoService _service = new();

    public ObservableCollection<ProductoItemViewModel> Productos { get; } = new();
    public ObservableCollection<CategoriaItemViewModel> Categorias { get; } = new();

    [ObservableProperty] private string _filtro = "";
    [ObservableProperty] private CategoriaItemViewModel? _categoriaSeleccionada;
    [ObservableProperty] private string _mensaje = "";
    [ObservableProperty] private bool _mensajeEsError;

    public ProductosViewModel()
    {
        CargarCategorias();
        CargarProductos();

        // 🔔 Suscribirse a los mensajes de actualización
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    // Se ejecuta cuando hay cambios en el catálogo
    public void Receive(CatalogoCambiadoMessage message)
    {
        CargarCategorias();
        CargarProductos();
    }

    // Se ejecuta al navegar a la vista
    public void Refresh()
    {
        CargarCategorias();
        CargarProductos();
    }

    partial void OnFiltroChanged(string value) => CargarProductos();
    partial void OnCategoriaSeleccionadaChanged(CategoriaItemViewModel? value) => CargarProductos();

    private void CargarCategorias()
    {
        Categorias.Clear();
        Categorias.Add(new CategoriaItemViewModel { IdCategoria = 0, Nombre = "Todas las categorías" });
        
        foreach (var c in _service.ListarCategorias())
            Categorias.Add(CategoriaItemViewModel.FromModel(c));

        // Mantener la selección si es posible
        var seleccionActual = CategoriaSeleccionada?.IdCategoria ?? 0;
        CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.IdCategoria == seleccionActual) ?? Categorias.FirstOrDefault();
    }

    private void CargarProductos()
    {
        Productos.Clear();
        var idCat = CategoriaSeleccionada?.IdCategoria == 0 ? (int?)null : CategoriaSeleccionada?.IdCategoria;

        foreach (var p in _service.ListarProductos(Filtro, idCat))
            Productos.Add(ProductoItemViewModel.FromModel(p));
    }

    [RelayCommand]
    private void NuevoProducto()
    {
        var modal = new ProductoFormModal
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        modal.ShowDialog();

        if (modal.Confirmado)
        {
            try
            {
                _service.CrearProducto(modal.ProductoResultado);
                Mensaje = $"Producto '{modal.ProductoResultado.Nombre}' creado.";
                MensajeEsError = false;
                // La recarga se hace automáticamente por el mensaje que envía el servicio
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
                MensajeEsError = true;
            }
        }
    }

    [RelayCommand]
    private void EditarProducto(ProductoItemViewModel item)
    {
        var modal = new ProductoFormModal(item.IdProducto)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        modal.ShowDialog();

        if (modal.Confirmado)
        {
            try
            {
                _service.ActualizarProducto(modal.ProductoResultado);
                Mensaje = $"Producto '{modal.ProductoResultado.Nombre}' actualizado.";
                MensajeEsError = false;
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
                MensajeEsError = true;
            }
        }
    }

    [RelayCommand]
    private void EliminarProducto(ProductoItemViewModel item)
    {
        var confirm = System.Windows.MessageBox.Show(
            $"¿Desactivar el producto '{item.Nombre}'?\n\nNo aparecerá en el POS pero se conserva el historial de ventas.",
            "Confirmar", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

        if (confirm != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            _service.EliminarProducto(item.IdProducto);
            Mensaje = $"Producto '{item.Nombre}' desactivado.";
            MensajeEsError = false;
        }
        catch (Exception ex)
        {
            Mensaje = $"Error: {ex.Message}";
            MensajeEsError = true;
        }
    }

    [RelayCommand]
    private void Recargar() => Refresh();
}