using System.Collections.ObjectModel;
using System.Windows;
using TiendaAbarrotes.Models;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.Views;

public partial class AbrirCajaModal : Window
{
    public int MontoInicialCentavos { get; private set; }
    public bool Confirmado { get; private set; }
    public List<CajaDesglose> DesgloseResultado { get; private set; } = new();

    public ObservableCollection<DenominacionItem> Denominaciones { get; } = new();

    public AbrirCajaModal()
    {
        InitializeComponent();
        InicializarDenominaciones();
        DenominacionesList.ItemsSource = Denominaciones;
    }

    private void InicializarDenominaciones()
    {
        decimal[] valores = { 1000, 500, 200, 100, 50, 20, 10, 5, 2, 1, 0.5m };
        foreach (var v in valores)
        {
            var item = new DenominacionItem { Valor = v };
            item.PropertyChanged += (_, _) => RecalcularTotal();
            Denominaciones.Add(item);
        }
    }

    private void RecalcularTotal()
    {
        MontoInicialCentavos = Denominaciones.Sum(d => d.TotalCentavos);
        TotalText.Text = (MontoInicialCentavos / 100m).ToString("C2");
    }

    private void Abrir_Click(object sender, RoutedEventArgs e)
    {
        DesgloseResultado = Denominaciones
            .Where(d => d.Cantidad > 0)
            .Select(d => new CajaDesglose
            {
                Denominacion = d.Valor,
                Cantidad = d.Cantidad
            })
            .ToList();

        Confirmado = true;
        Close();
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Confirmado = false;
        Close();
    }
}