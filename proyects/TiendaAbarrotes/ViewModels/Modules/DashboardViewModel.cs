using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TiendaAbarrotes.Services;
using CommunityToolkit.Mvvm.Messaging;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.ViewModels.Modules;

public partial class DashboardViewModel : ObservableObject, IRefreshable, 
    IRecipient<VentaRegistradaMessage>, 
    IRecipient<StockCambiadoMessage>
{
    private readonly DashboardService _service = new();

    // ============ KPIs ============
    [ObservableProperty] private string _ventasDelDia = "$0.00";
    [ObservableProperty] private string _numVentasDelDia = "0 ventas";
    [ObservableProperty] private string _totalProductos = "0";
    [ObservableProperty] private string _stockBajo = "0";
    [ObservableProperty] private string _gananciaDelMes = "$0.00";
    [ObservableProperty] private string _ventasDelMes = "$0.00";
    [ObservableProperty] private string _fechaActual = "";

    // ============ Gráfico ============
    [ObservableProperty] private ISeries[] _seriesVentas = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _ejeX = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _ejeY = Array.Empty<Axis>();

    // ============ Top productos ============
    public ObservableCollection<TopProductoItem> TopProductos { get; } = new();

   public DashboardViewModel()
    {
        FechaActual = DateTime.Today.ToString("dddd, dd 'de' MMMM 'de' yyyy");
        CargarTodo();

        // Registrar este ViewModel para escuchar los mensajes
        WeakReferenceMessenger.Default.RegisterAll(this);

    }

    public void Receive(VentaRegistradaMessage message) => CargarTodo();
    public void Receive(StockCambiadoMessage message) => CargarTodo();

    public void Refresh() => CargarTodo();
    
    [RelayCommand]
    private void Actualizar() => CargarTodo();

    private void CargarTodo()
    {
        // KPIs
        VentasDelDia = (_service.VentasDelDia() / 100m).ToString("C2");
        var num = _service.NumVentasDelDia();
        NumVentasDelDia = $"{num} venta{(num == 1 ? "" : "s")} hoy";

        TotalProductos = _service.TotalProductos().ToString("N0");
        StockBajo = _service.ProductosStockBajo().ToString("N0");
        GananciaDelMes = (_service.GananciaDelMes() / 100m).ToString("C2");
        VentasDelMes = (_service.VentasDelMes() / 100m).ToString("C2");

        // Gráfico (últimos 30 días)
        var datos = _service.VentasUltimos30Dias();

        SeriesVentas = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = datos.Select(d => d.Total / 100.0).ToArray(),
                GeometrySize = 8,
                LineSmoothness = 0.5,
                Stroke = new SolidColorPaint(SKColor.Parse("#5E5CE6"), 3),
                Fill = new SolidColorPaint(SKColor.Parse("#5E5CE6").WithAlpha(40)),
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#5E5CE6"), 2),
                GeometryFill = new SolidColorPaint(SKColors.White)
            }
        };

        EjeX = new Axis[]
        {
            new Axis
            {
                Labels = datos.Select(d => d.Dia.ToString("dd/MM")).ToArray(),
                LabelsRotation = 45,
                TextSize = 11,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB"))
            }
        };

        EjeY = new Axis[]
        {
            new Axis
            {
                TextSize = 11,
                Labeler = v => $"${v:N0}",
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB"))
            }
        };

        // Top productos
        TopProductos.Clear();
        foreach (var p in _service.Top5Productos())
        {
            TopProductos.Add(new TopProductoItem
            {
                Nombre = p.Nombre,
                CantidadVendida = p.CantidadVendida,
                TotalVendido = (p.TotalVendido / 100m).ToString("C2")
            });
        }
    }
}

public class TopProductoItem
{
    public string Nombre { get; set; } = "";
    public int CantidadVendida { get; set; }
    public string TotalVendido { get; set; } = "";
}