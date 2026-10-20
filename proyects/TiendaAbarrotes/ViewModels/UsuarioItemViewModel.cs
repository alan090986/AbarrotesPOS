using TiendaAbarrotes.Models;

namespace TiendaAbarrotes.ViewModels;

public class UsuarioItemViewModel
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    public string Username { get; set; } = "";
    public string Rol { get; set; } = "";
    public bool Activo { get; set; }
    public DateTime? UltimoAcceso { get; set; }

    public string ActivoTexto => Activo ? "Activo" : "Inactivo";
    public string UltimoAccesoTexto => UltimoAcceso.HasValue
        ? UltimoAcceso.Value.ToString("dd/MM/yyyy HH:mm")
        : "Nunca";
    public string RolTexto => Rol switch
    {
        "ADMIN"   => "Administrador",
        "CAJERO"  => "Cajero",
        "ALMACEN" => "Almacén",
        _         => Rol
    };

    public static UsuarioItemViewModel FromModel(Usuario u) => new()
    {
        IdUsuario    = u.IdUsuario,
        Nombre       = u.Nombre,
        Username     = u.Username,
        Rol          = u.Rol,
        Activo       = u.Activo,
        UltimoAcceso = u.UltimoAcceso
    };
}