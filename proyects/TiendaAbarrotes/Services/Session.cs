using TiendaAbarrotes.Models;

namespace TiendaAbarrotes.Services;

public static class Session
{
    public static Usuario? UsuarioActual { get; set; }
    public static ConfiguracionTienda? Tienda { get; set; }

    public static bool EstaLogueado => UsuarioActual != null;
    public static bool EsAdmin => UsuarioActual?.Rol == "ADMIN";
    public static int IdUsuarioActual => UsuarioActual?.IdUsuario ?? 0;
    public static string NombreUsuario => UsuarioActual?.Nombre ?? "";

    public static void Cerrar()
    {
        UsuarioActual = null;
    }
}