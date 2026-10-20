using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TiendaAbarrotes.Services;
using TiendaAbarrotes.Views;
using CommunityToolkit.Mvvm.Messaging;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.ViewModels.Modules;

public partial class InventarioViewModel : ObservableObject, IRefreshable, IRecipient<StockCambiadoMessage>
{
    private readonly InventarioService _service = new();

    public ObservableCollection<InventarioService.StockItem> StockItems { get; } = new();
    public ObservableCollection<InventarioService.MovimientoItem> Movimientos { get; } = new();

    [ObservableProperty] private string _filtroStock = "";
    [ObservableProperty] private string _filtroHistorial = "";
    [ObservableProperty] private string _tipoSeleccionado = "TODOS";
    [ObservableProperty] private string _mensaje = "";
    [ObservableProperty] private bool _mensajeEsError;
    [ObservableProperty] private int _tabSeleccionado; // 0 = stock, 1 = historial

    // KPIs
    [ObservableProperty] private string _kpiSKUs = "0";
    [ObservableProperty] private string _kpiValorInventario = "$0.00";
    [ObservableProperty] private string _kpiStockBajo = "0";
    [ObservableProperty] private string _kpiMovsHoy = "0";

    public InventarioViewModel()
    {
        RecargarTodo();

           WeakReferenceMessenger.Default.RegisterAll(this);
    }
    public void Refresh() => RecargarTodo();

    public void Receive(StockCambiadoMessage message) => RecargarTodo();

    

    partial void OnFiltroStockChanged(string value) => CargarStock();
    partial void OnFiltroHistorialChanged(string value) => CargarMovimientos();
    partial void OnTipoSeleccionadoChanged(string value) => CargarMovimientos();

    private void RecargarTodo()
    {
        CargarKPIs();
        CargarStock();
        CargarMovimientos();
    }

    private void CargarKPIs()
    {
        KpiSKUs             = _service.TotalSKUs().ToString("N0");
        KpiValorInventario  = (_service.ValorInventarioCentavos() / 100m).ToString("C2");
        KpiStockBajo        = _service.ProductosStockBajo().ToString("N0");
        KpiMovsHoy          = _service.MovimientosHoy().ToString("N0");
    }

    private void CargarStock()
    {
        StockItems.Clear();
        foreach (var s in _service.ListarStock(FiltroStock))
            StockItems.Add(s);
    }

    private void CargarMovimientos()
    {
        Movimientos.Clear();
        var tipo = TipoSeleccionado == "TODOS" ? null : TipoSeleccionado;
        foreach (var m in _service.ListarMovimientos(FiltroHistorial, tipo))
            Movimientos.Add(m);
    }

    // ==================== COMANDOS ====================

    [RelayCommand]
    private void NuevaEntrada()
    {
        var modal = new MovimientoStockModal("ENTRADA")
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        modal.ShowDialog();
        if (modal.Confirmado) { Mensaje = "Entrada registrada."; MensajeEsError = false; RecargarTodo(); }
    }

    [RelayCommand]
    private void NuevaSalida()
    {
        var modal = new MovimientoStockModal("SALIDA")
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        modal.ShowDialog();
        if (modal.Confirmado) { Mensaje = "Salida registrada."; MensajeEsError = false; RecargarTodo(); }
    }

    [RelayCommand]
    private void AgregarStock(InventarioService.StockItem item)
    {
        var modal = new MovimientoStockModal("ENTRADA", item.IdProducto)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        modal.ShowDialog();
        if (modal.Confirmado)
        {
            Mensaje = $"Stock actualizado para '{item.Nombre}'.";
            MensajeEsError = false;
            RecargarTodo();
        }
    }

    [RelayCommand]
    private void Recargar() => RecargarTodo();
}