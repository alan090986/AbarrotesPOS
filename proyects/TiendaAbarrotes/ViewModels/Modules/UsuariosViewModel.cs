using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TiendaAbarrotes.Services;
using CommunityToolkit.Mvvm.Messaging;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.ViewModels.Modules;

public partial class UsuariosViewModel : ObservableObject, IRefreshable, IRecipient<CatalogoCambiadoMessage>
{
    private readonly AuthService _auth = new();

    public ObservableCollection<UsuarioItemViewModel> Usuarios { get; } = new();

    [ObservableProperty] private string _mensaje = "";
    [ObservableProperty] private bool _mensajeEsError;
    [ObservableProperty] private UsuarioItemViewModel? _seleccionado;

        public UsuariosViewModel()
    {
        CargarUsuarios();
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Receive(CatalogoCambiadoMessage message) => CargarUsuarios();

    public void Refresh() => CargarUsuarios();

    private void CargarUsuarios()
    {
        Usuarios.Clear();
        foreach (var u in _auth.ListarUsuarios())
            Usuarios.Add(UsuarioItemViewModel.FromModel(u));
    }

    [RelayCommand]
    private void NuevoUsuario()
    {
        var modal = new Views.UsuarioFormModal
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        modal.ShowDialog();

        if (modal.Confirmado)
        {
            try
            {
                _auth.CrearUsuario(modal.Nombre, modal.Username, modal.Password, modal.Rol);
                Mensaje = $"Usuario '{modal.Nombre}' creado.";
                MensajeEsError = false;
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
                MensajeEsError = true;
            }
        }
    }

    [RelayCommand]
    private void EditarUsuario(UsuarioItemViewModel item)
    {
        var modal = new Views.UsuarioFormModal(item.IdUsuario)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        modal.ShowDialog();

        if (modal.Confirmado)
        {
            try
            {
                _auth.ActualizarUsuario(item.IdUsuario, modal.Nombre, modal.Username, modal.Rol, modal.Activo);
                if (!string.IsNullOrWhiteSpace(modal.Password))
                    _auth.CambiarPassword(item.IdUsuario, modal.Password);

                Mensaje = $"Usuario '{modal.Nombre}' actualizado.";
                MensajeEsError = false;
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
                MensajeEsError = true;
            }
        }
    }

    [RelayCommand]
    private void EliminarUsuario(UsuarioItemViewModel item)
    {
        var confirm = System.Windows.MessageBox.Show(
            $"¿Desactivar al usuario '{item.Nombre}'?\n\nNo podrá iniciar sesión, pero su historial de ventas y cajas se conserva.",
            "Confirmar", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

        if (confirm != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            _auth.EliminarUsuario(item.IdUsuario);
            Mensaje = $"Usuario '{item.Nombre}' desactivado.";
            MensajeEsError = false;
            CargarUsuarios();
        }
        catch (Exception ex)
        {
            Mensaje = $"Error: {ex.Message}";
            MensajeEsError = true;
        }
    }
}