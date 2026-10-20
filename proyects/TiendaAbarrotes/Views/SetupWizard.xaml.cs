using System.Windows;
using TiendaAbarrotes.Models;
using TiendaAbarrotes.Services;

namespace TiendaAbarrotes.Views;

public partial class SetupWizard : Window
{
    private readonly AuthService _auth = new();
    private readonly ConfiguracionService _config = new();

    public bool Completado { get; private set; }

    public SetupWizard()
    {
        InitializeComponent();
        Loaded += (_, _) => NombreTiendaText.Focus();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(NombreTiendaText.Text))
        {
            MostrarError("El nombre de la tienda es obligatorio.");
            return;
        }
        if (string.IsNullOrWhiteSpace(NombreAdminText.Text) ||
            string.IsNullOrWhiteSpace(UsernameText.Text))
        {
            MostrarError("El nombre y usuario del administrador son obligatorios.");
            return;
        }
        if (string.IsNullOrWhiteSpace(PasswordText.Password) || PasswordText.Password.Length < 4)
        {
            MostrarError("La contraseña debe tener al menos 4 caracteres.");
            return;
        }
        if (PasswordText.Password != PasswordConfirmText.Password)
        {
            MostrarError("Las contraseñas no coinciden.");
            return;
        }

        try
        {
            // 1. Guardar configuración de la tienda
            _config.Guardar(new ConfiguracionTienda
            {
                NombreTienda = NombreTiendaText.Text.Trim(),
                Rfc          = string.IsNullOrWhiteSpace(RfcText.Text) ? null : RfcText.Text.Trim(),
                Telefono     = string.IsNullOrWhiteSpace(TelefonoText.Text) ? null : TelefonoText.Text.Trim(),
                Direccion    = string.IsNullOrWhiteSpace(DireccionText.Text) ? null : DireccionText.Text.Trim(),
                Email        = string.IsNullOrWhiteSpace(EmailText.Text) ? null : EmailText.Text.Trim()
            });

            // 2. Crear usuario admin
            _auth.CrearUsuario(
                NombreAdminText.Text.Trim(),
                UsernameText.Text.Trim(),
                PasswordText.Password,
                "ADMIN");

            Completado = true;
            Close();
        }
        catch (Exception ex)
        {
            MostrarError($"Error al guardar: {ex.Message}");
        }
    }

    private void MostrarError(string msg)
    {
        MessageBox.Show(msg, "Faltan datos", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Completado = false;
        Close();
    }
}