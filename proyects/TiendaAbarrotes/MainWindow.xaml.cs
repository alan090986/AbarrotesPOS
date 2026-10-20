using System.Windows;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
