using System.Windows.Controls;
using System.Windows.Input;
using TiendaAbarrotes.ViewModels.Modules;

namespace TiendaAbarrotes.Views;

public partial class VentasView : UserControl
{
    public VentasView()
    {
        InitializeComponent();
        Loaded += (_, _) => BarcodeInput.Focus();
    }

    private void BarcodeInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is not VentasViewModel vm) return;

        vm.ProcesarCodigo();

        BarcodeInput.Clear();
        BarcodeInput.Focus();
        e.Handled = true;
    }
}
