using System.Windows;
using System.Windows.Input;
using TiendaAbarrotes.Services;

namespace TiendaAbarrotes.Views;

public partial class AdminAuthModal : Window
{
    public bool Autorizado { get; private set; }

    public AdminAuthModal()
    {
        InitializeComponent();
        Loaded += (_, _) => UsernameText.Focus();
    }

    private void Autorizar_Click(object sender, RoutedEventArgs e) => Verificar();

    private void PasswordText_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Verificar();
    }

    private void Verificar()
    {
        var username = UsernameText.Text?.Trim() ?? "";
        var password = PasswordText.Password ?? "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Ingresa usuario y contraseña.", "Datos requeridos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var user = new AuthService().Login(username, password);

        if (user == null || user.Rol != "ADMIN")
        {
            MessageBox.Show("Credenciales inválidas o el usuario no es Administrador.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Error);
            PasswordText.Clear();
            PasswordText.Focus();
            return;
        }

        Autorizado = true;
        Close();
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Autorizado = false;
        Close();
    }
}