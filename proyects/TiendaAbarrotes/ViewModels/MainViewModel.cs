using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using TiendaAbarrotes.ViewModels.Modules;
using TiendaAbarrotes.Views;
using TiendaAbarrotes.Services;


namespace TiendaAbarrotes.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Diccionario de Vistas → ViewModels (registro de módulos)
    private readonly Dictionary<string, UserControl> _views = new();

    public ObservableCollection<SidebarItemViewModel> SidebarItems { get; } = new();

    [ObservableProperty] private object? _currentView;
    [ObservableProperty] private string _paginaActual = "Dashboard";
    [ObservableProperty] private UsuarioInfo _usuarioActual = new();

    

           public MainViewModel()
    {
        // 1. Registro de vistas
        _views["Dashboard"] = new DashboardView { DataContext = new DashboardViewModel() };
        _views["Ventas"] = new VentasView { DataContext = new VentasViewModel() };
        _views["Productos"] = new ProductosView { DataContext = new Modules.ProductosViewModel() };
        _views["Inventario"] = new InventarioView { DataContext = new Modules.InventarioViewModel() };
        _views["Reportes"] = new ReportesView { DataContext = new Modules.ReportesViewModel() };
        _views["Usuarios"] = new UsuariosView { DataContext = new Modules.UsuariosViewModel() };

        // 2. Construir el menú según el Rol
        if (Session.EsAdmin)
        {
            // El Admin ve todo, empezando por el Dashboard
            SidebarItems.Add(new() { Titulo = "Inicio",         Icon = PackIconKind.HomeOutline,     ViewKey = "Dashboard"  });
            SidebarItems.Add(new() { Titulo = "Punto de Venta", Icon = PackIconKind.CartOutline,     ViewKey = "Ventas"     });
            SidebarItems.Add(new() { Titulo = "Productos",      Icon = PackIconKind.PackageVariant,  ViewKey = "Productos"  });
            SidebarItems.Add(new() { Titulo = "Inventario",     Icon = PackIconKind.Warehouse,       ViewKey = "Inventario" });
            SidebarItems.Add(new() { Titulo = "Reportes",       Icon = PackIconKind.ChartBox,        ViewKey = "Reportes"   });
            SidebarItems.Add(new() { Titulo = "Usuarios",       Icon = PackIconKind.AccountGroup,    ViewKey = "Usuarios"   });
        }
        else
        {
            // El Cajero SOLO ve el Punto de Venta
            SidebarItems.Add(new() { Titulo = "Punto de Venta", Icon = PackIconKind.CartOutline, ViewKey = "Ventas" });
        }

        // 3. Cargar usuario real desde Session
        UsuarioActual = new UsuarioInfo
        {
            Nombre = Session.NombreUsuario,
            Rol    = Session.UsuarioActual?.Rol ?? "CAJERO"
        };

        // 4. Navegar a la primera vista
        // Si es Admin, entrará a "Inicio" (Dashboard). 
        // Si es Cajero, entrará a "Punto de Venta".
        Navigate(SidebarItems[0]);
    }

   [RelayCommand]
    private void Navigate(SidebarItemViewModel item)
    {
        foreach (var s in SidebarItems) s.IsActive = false;
        item.IsActive = true;

        var view = _views[item.ViewKey];
        CurrentView = view;
        PaginaActual = item.Titulo;

        // ACTUALIZAR AL CAMBIAR DE PESTAÑA
        if (view is System.Windows.Controls.UserControl uc &&
            uc.DataContext is IRefreshable refreshable)
        {
            refreshable.Refresh();
        }
    }



    [RelayCommand]



private void CrearRespaldoManual()
{
    try
    {
        var backup = new BackupService();
        backup.CrearRespaldo();
        
        System.Windows.MessageBox.Show(
            $"Respaldo creado correctamente.\n\nSe guardó en:\n{backup.RutaCarpetaRespaldos}", 
            "Respaldo exitoso", 
            System.Windows.MessageBoxButton.OK, 
            System.Windows.MessageBoxImage.Information);

        // Opcional: Abrir la carpeta en el explorador de Windows
        System.Diagnostics.Process.Start("explorer.exe", backup.RutaCarpetaRespaldos);
    }
    catch (Exception ex)
    {
        System.Windows.MessageBox.Show($"Error al respaldar: {ex.Message}", "Error", 
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    }
}
}



public class UsuarioInfo
{
    public string Nombre { get; set; } = "Alan Mario Villegas Reyes";
    public string Rol { get; set; } = "ADMIN";
}


