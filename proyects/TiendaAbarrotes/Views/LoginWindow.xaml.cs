using System.Windows;
using System.Windows.Input;
using TiendaAbarrotes.Services;

namespace TiendaAbarrotes.Views;

public partial class LoginWindow : Window
{
    private readonly AuthService _auth = new();

    public bool Autenticado { get; private set; }

    public LoginWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => UsernameText.Focus();
    }

    private void Login_Click(object sender, RoutedEventArgs e) => IntentarLogin();

    private void PasswordText_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) IntentarLogin();
    }

    private void IntentarLogin()
    {
        ErrorText.Visibility = Visibility.Collapsed;

        var username = UsernameText.Text?.Trim() ?? "";
        var password = PasswordText.Password ?? "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MostrarError("Ingresa usuario y contraseña.");
            return;
        }

        var usuario = _auth.Login(username, password);
        if (usuario == null)
        {
            MostrarError("Usuario o contraseña incorrectos.");
            PasswordText.Clear();
            PasswordText.Focus();
            return;
        }

        // Guardar en Session
        Session.UsuarioActual = usuario;
        Session.Tienda = new ConfiguracionService().Obtener();

        Autenticado = true;
        Close();
    }

    private void MostrarError(string msg)
    {
        ErrorText.Text = msg;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        Autenticado = false;
        Close();
    }
}