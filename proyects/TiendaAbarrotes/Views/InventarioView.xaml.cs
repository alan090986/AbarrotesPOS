using System.Windows;
using System.Windows.Controls;
using TiendaAbarrotes.ViewModels.Modules;

namespace TiendaAbarrotes.Views;

public partial class InventarioView : UserControl
{
    public InventarioView() => InitializeComponent();

    private void Tab_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string tag && int.TryParse(tag, out var idx))
        {
            if (DataContext is InventarioViewModel vm)
                vm.TabSeleccionado = idx;
        }
    }
}