using TiendaAbarrotes.Models;

namespace TiendaAbarrotes.ViewModels;

public class CategoriaItemViewModel
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = "";

    public static CategoriaItemViewModel FromModel(Categoria c) => new()
    {
        IdCategoria = c.IdCategoria,
        Nombre = c.Nombre
    };
}