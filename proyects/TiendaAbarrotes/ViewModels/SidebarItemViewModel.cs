using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;

namespace TiendaAbarrotes.ViewModels;

public partial class SidebarItemViewModel : ObservableObject
{
    public string Titulo { get; set; } = "";
    public PackIconKind Icon { get; set; }
    public string ViewKey { get; set; } = "";

    [ObservableProperty]
    private bool _isActive;
}