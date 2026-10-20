using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.Views;

public partial class PagoModal : Window
{
    public int TotalCentavos { get; }
    public string MetodoPago { get; private set; } = "EFECTIVO";
    public int PagoConCentavos { get; private set; }
    public int CambioCentavos { get; private set; }
    public bool Confirmado { get; private set; }

    public ObservableCollection<DenominacionItem> Denominaciones { get; } = new();

    public PagoModal(int totalCentavos)
    {
        InitializeComponent();
        TotalCentavos = totalCentavos;
        TotalText.Text = (totalCentavos / 100m).ToString("C2");

        InicializarDenominaciones();
        DenominacionesList.ItemsSource = Denominaciones;

        MetodoPagoCombo.SelectedIndex = 0; // Efectivo por defecto
    }

    private void InicializarDenominaciones()
    {
        decimal[] valores = { 1000, 500, 200, 100, 50, 20, 10, 5, 2, 1, 0.5m };
        foreach (var v in valores)
        {
            var item = new DenominacionItem { Valor = v };
            item.PropertyChanged += (_, _) => RecalcularCambio();
            Denominaciones.Add(item);
        }
    }

    private void MetodoPagoCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MetodoPagoCombo.SelectedItem is not ComboBoxItem item) return;
        var metodo = item.Tag?.ToString() ?? "EFECTIVO";

        if (metodo == "EFECTIVO")
        {
            DenominacionesPanel.Visibility = Visibility.Visible;
            PagoElectronicoPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            DenominacionesPanel.Visibility = Visibility.Collapsed;
            PagoElectronicoPanel.Visibility = Visibility.Visible;

            // Limpia cantidades y calcula "pago exacto"
            foreach (var d in Denominaciones) d.CantidadTexto = "0";
            PagoConCentavos = TotalCentavos;
            CambioCentavos = 0;
            TotalPagadoText.Text = (TotalCentavos / 100m).ToString("C2");
            CambioText.Text = (0 / 100m).ToString("C2");
        }
    }

    private void RecalcularCambio()
    {
        PagoConCentavos = Denominaciones.Sum(d => d.TotalCentavos);
        TotalPagadoText.Text = (PagoConCentavos / 100m).ToString("C2");

        var cambio = PagoConCentavos - TotalCentavos;
        CambioCentavos = cambio > 0 ? cambio : 0;
        CambioText.Text = (CambioCentavos / 100m).ToString("C2");
    }

    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        if (MetodoPagoCombo.SelectedItem is not ComboBoxItem item) return;
        MetodoPago = item.Tag?.ToString() ?? "EFECTIVO";

        if (MetodoPago == "EFECTIVO" && PagoConCentavos < TotalCentavos)
        {
            MessageBox.Show("El monto pagado es menor al total. Revisa las denominaciones.",
                            "Monto insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MetodoPago != "EFECTIVO")
        {
            PagoConCentavos = TotalCentavos;
            CambioCentavos = 0;
        }

        Confirmado = true;
        Close();
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Confirmado = false;
        Close();
    }
}