using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;

namespace TiendaAbarrotes.Views;

public partial class UsuarioFormModal : Window
{
    public string Nombre { get; private set; } = "";
    public string Username { get; private set; } = "";
    public string Password { get; private set; } = "";
    public string Rol { get; private set; } = "CAJERO";
    public bool Activo { get; private set; } = true;
    public bool Confirmado { get; private set; }

    private readonly int? _idUsuario;

    /// <summary>Constructor para ALTA de usuario.</summary>
    public UsuarioFormModal()
    {
        InitializeComponent();
        _idUsuario = null;
        Loaded += (_, _) => NombreText.Focus();
    }

    /// <summary>Constructor para EDICIÓN.</summary>
    public UsuarioFormModal(int idUsuario)
    {
        InitializeComponent();
        _idUsuario = idUsuario;

        TituloLabel.Text = "EDITAR USUARIO";
        SubtituloLabel.Text = "Editar usuario";
        PasswordLabel.Text = "Nueva contraseña (opcional)";
        PasswordHint.Text = "Déjala vacía para conservar la actual";
        ActivoCheck.Visibility = Visibility.Visible;

        CargarDatos();
        Loaded += (_, _) => NombreText.Focus();
    }

    private void CargarDatos()
    {
        using var db = new TiendaDbContext();
        var u = db.Usuarios.Find(_idUsuario!.Value);
        if (u == null) return;

        NombreText.Text = u.Nombre;
        UsernameText.Text = u.Username;
        ActivoCheck.IsChecked = u.Activo;

        foreach (ComboBoxItem item in RolCombo.Items)
        {
            if (item.Tag?.ToString() == u.Rol)
            {
                RolCombo.SelectedItem = item;
                break;
            }
        }
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NombreText.Text))
        {
            MessageBox.Show("El nombre es obligatorio.");
            return;
        }
        if (string.IsNullOrWhiteSpace(UsernameText.Text))
        {
            MessageBox.Show("El usuario es obligatorio.");
            return;
        }

        // En alta, la contraseña es obligatoria
        if (_idUsuario == null && PasswordText.Password.Length < 4)
        {
            MessageBox.Show("La contraseña debe tener al menos 4 caracteres.");
            return;
        }
        // En edición, si escribió algo, debe tener mínimo 4
        if (_idUsuario != null && PasswordText.Password.Length > 0 && PasswordText.Password.Length < 4)
        {
            MessageBox.Show("La contraseña debe tener al menos 4 caracteres.");
            return;
        }

        if (RolCombo.SelectedItem is not ComboBoxItem rolItem)
        {
            MessageBox.Show("Selecciona un rol.");
            return;
        }

        Nombre = NombreText.Text.Trim();
        Username = UsernameText.Text.Trim();
        Password = PasswordText.Password;
        Rol = rolItem.Tag?.ToString() ?? "CAJERO";
        Activo = ActivoCheck.IsChecked == true;

        Confirmado = true;
        Close();
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Confirmado = false;
        Close();
    }
}