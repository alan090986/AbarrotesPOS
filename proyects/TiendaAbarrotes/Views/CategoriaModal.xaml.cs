using System.Windows;
using TiendaAbarrotes.Services;

namespace TiendaAbarrotes.Views;

public partial class CategoriaModal : Window
{
    public bool Confirmado { get; private set; }
    public int IdCategoriaCreada { get; private set; }

    public CategoriaModal()
    {
        InitializeComponent();
        Loaded += (_, _) => NombreText.Focus();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NombreText.Text))
        {
            MessageBox.Show("El nombre es obligatorio.");
            return;
        }

        try
        {
            var cat = new ProductoService().CrearCategoria(
                NombreText.Text.Trim(),
                string.IsNullOrWhiteSpace(DescripcionText.Text) ? null : DescripcionText.Text.Trim());

            IdCategoriaCreada = cat.IdCategoria;
            Confirmado = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Confirmado = false;
        Close();
    }
}