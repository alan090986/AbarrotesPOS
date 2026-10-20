using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TiendaAbarrotes.Services;

namespace TiendaAbarrotes.ViewModels.Modules;

public partial class ReportesViewModel : ObservableObject
{
    private readonly ReporteService _service = new();

    public ObservableCollection<ReporteService.VentaReporteItem> Ventas { get; } = new();

    [ObservableProperty] private DateTime _fechaInicio = DateTime.Today.AddDays(-7); // Últimos 7 días por defecto
    [ObservableProperty] private DateTime _fechaFin = DateTime.Today;
    
    [ObservableProperty] private string _totalVentasTexto = "$0.00";
    [ObservableProperty] private string _numeroVentasTexto = "0 ventas";

    public ReportesViewModel()
    {
        Buscar();
    }

    [RelayCommand]
    private void Buscar()
    {
        // Validar fechas
        if (FechaInicio > FechaFin)
        {
            System.Windows.MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha fin.", "Fechas inválidas", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        Ventas.Clear();
        var resultados = _service.ObtenerVentas(FechaInicio, FechaFin);
        
        foreach (var v in resultados)
            Ventas.Add(v);

        var totalCentavos = _service.TotalVentas(FechaInicio, FechaFin);
        
        TotalVentasTexto = (totalCentavos / 100m).ToString("C2");
        NumeroVentasTexto = $"{resultados.Count} venta{(resultados.Count == 1 ? "" : "s")} en el periodo";
    }
}