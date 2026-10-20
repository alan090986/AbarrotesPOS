using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;
using TiendaAbarrotes.Models;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.Views;

public partial class CerrarCajaModal : Window
{
    public int IdCaja { get; }
    public int MontoRealCentavos { get; private set; }
    public string? Observaciones { get; private set; }
    public bool Confirmado { get; private set; }
    public List<CajaDesglose> DesgloseResultado { get; private set; } = new();

    public ObservableCollection<DenominacionItem> Denominaciones { get; } = new();

    private int _montoEsperado;

    public CerrarCajaModal(int idCaja)
    {
        InitializeComponent();
        IdCaja = idCaja;
        InicializarDenominaciones();
        DenominacionesList.ItemsSource = Denominaciones;
        
        try
        {
            CargarResumen();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar datos de caja: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void InicializarDenominaciones()
    {
        decimal[] valores = { 1000, 500, 200, 100, 50, 20, 10, 5, 2, 1, 0.5m };
        foreach (var v in valores)
        {
            var item = new DenominacionItem { Valor = v };
            item.PropertyChanged += (_, _) => RecalcularContado();
            Denominaciones.Add(item);
        }
    }

    private void CargarResumen()
    {
        using var db = new TiendaDbContext();
        var caja = db.Cajas.Include(c => c.Ventas).FirstOrDefault(c => c.IdCaja == IdCaja);
        if (caja == null) return;

        var ventas = caja.Ventas.Where(v => v.Estado == "COMPLETADA").ToList();
        var efectivo = ventas.Where(v => v.MetodoPago == "EFECTIVO").Sum(v => v.Total);
        var tarjeta  = ventas.Where(v => v.MetodoPago == "TARJETA").Sum(v => v.Total);
        var transf   = ventas.Where(v => v.MetodoPago == "TRANSFERENCIA").Sum(v => v.Total);

        _montoEsperado = caja.MontoInicial + efectivo;

        MontoInicialText.Text   = (caja.MontoInicial / 100m).ToString("C2");
        VentasEfectivoText.Text = (efectivo / 100m).ToString("C2");
        VentasTarjetaText.Text  = (tarjeta / 100m).ToString("C2");
        VentasTransfText.Text   = (transf / 100m).ToString("C2");
        MontoEsperadoText.Text  = (_montoEsperado / 100m).ToString("C2");

        var desgloseApertura = db.CajaDesgloses
        .Where(d => d.IdCaja == IdCaja && d.Tipo == "APERTURA")
        .ToList() // <-- Trae los datos a memoria
        .OrderByDescending(d => d.Denominacion) // <-- Ahora sí, ordena en C#
        .ToList();

        if (desgloseApertura.Any())
        {
            var texto = string.Join("  |  ", desgloseApertura.Select(d => $"{d.Cantidad} x {d.Denominacion:C0}"));
            DesgloseAperturaText.Text = texto;
        }

        RecalcularContado(); 
    }

    private void RecalcularContado()
    {
        MontoRealCentavos = Denominaciones.Sum(d => d.TotalCentavos);
        TotalContadoText.Text = (MontoRealCentavos / 100m).ToString("C2");

        var dif = MontoRealCentavos - _montoEsperado;
        DiferenciaText.Text = (dif / 100m).ToString("C2");

        if (dif == 0)
        {
            DiferenciaText.Foreground = (System.Windows.Media.Brush)FindResource("BrushExito");
            DiferenciaLabel.Text = "Cuadre perfecto";
        }
        else if (dif > 0)
        {
            DiferenciaText.Foreground = (System.Windows.Media.Brush)FindResource("BrushAcento");
            DiferenciaLabel.Text = "Sobrante en caja";
        }
        else
        {
            DiferenciaText.Foreground = (System.Windows.Media.Brush)FindResource("BrushPeligro");
            DiferenciaLabel.Text = "Faltante en caja";
        }
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        DesgloseResultado = Denominaciones
            .Where(d => d.Cantidad > 0)
            .Select(d => new CajaDesglose
            {
                Denominacion = d.Valor,
                Cantidad = d.Cantidad
            })
            .ToList();

        Observaciones = string.IsNullOrWhiteSpace(ObservacionesText.Text) ? null : ObservacionesText.Text.Trim();
        Confirmado = true;
        Close();
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Confirmado = false;
        Close();
    }
}