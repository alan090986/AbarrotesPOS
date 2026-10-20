using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;
using TiendaAbarrotes.Models;
using TiendaAbarrotes.Utils;
using CommunityToolkit.Mvvm.Messaging;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.Services;

public class AuthService
{
    /// <summary>¿Ya hay al menos un usuario registrado?</summary>
    public bool HayUsuarios()
    {
        using var db = new TiendaDbContext();
        return db.Usuarios.Any();
    }

    /// <summary>Valida credenciales. Devuelve el usuario o null si fallan.</summary>
    public Usuario? Login(string username, string password)
    {
        using var db = new TiendaDbContext();
        var user = db.Usuarios.FirstOrDefault(u => u.Username == username && u.Activo);
        if (user == null) return null;

        if (!PasswordHasher.Verify(password, user.PasswordHash)) return null;

        // Actualizar último acceso
        user.UltimoAcceso = DateTime.Now;
        db.SaveChanges();

        // Devolver una copia "desconectada" de la BD (evita problemas con DbContext)
        return new Usuario
        {
            IdUsuario    = user.IdUsuario,
            Nombre       = user.Nombre,
            Username     = user.Username,
            Rol          = user.Rol,
            Activo       = user.Activo,
            UltimoAcceso = user.UltimoAcceso
        };
    }

    /// <summary>Crea un usuario. Valida que el username sea único.</summary>
    public Usuario CrearUsuario(string nombre, string username, string password, string rol)
    {
        using var db = new TiendaDbContext();

        if (db.Usuarios.Any(u => u.Username == username))
            throw new InvalidOperationException($"El usuario '{username}' ya existe.");

        var user = new Usuario
        {
            Nombre       = nombre,
            Username     = username,
            PasswordHash = PasswordHasher.Hash(password),
            Rol          = rol,
            Activo       = true
        };

        db.Usuarios.Add(user);
        db.SaveChanges();
        WeakReferenceMessenger.Default.Send<CatalogoCambiadoMessage>(new CatalogoCambiadoMessage());
        return user;
    }


    public List<Usuario> ListarUsuarios()
{
    using var db = new TiendaDbContext();
    return db.Usuarios.OrderBy(u => u.Nombre).ToList();
}

public void ActualizarUsuario(int id, string nombre, string username, string rol, bool activo)
{
    using var db = new TiendaDbContext();

    var user = db.Usuarios.Find(id) ?? throw new InvalidOperationException("Usuario no encontrado.");
    if (db.Usuarios.Any(u => u.Username == username && u.IdUsuario != id))
        throw new InvalidOperationException($"El usuario '{username}' ya existe.");

    user.Nombre   = nombre;
    user.Username = username;
    user.Rol      = rol;
    user.Activo   = activo;
    db.SaveChanges();
    WeakReferenceMessenger.Default.Send<CatalogoCambiadoMessage>(new CatalogoCambiadoMessage());
}

public void CambiarPassword(int id, string nuevaPassword)
{
    using var db = new TiendaDbContext();
    var user = db.Usuarios.Find(id) ?? throw new InvalidOperationException("Usuario no encontrado.");
    user.PasswordHash = PasswordHasher.Hash(nuevaPassword);
    db.SaveChanges();
    WeakReferenceMessenger.Default.Send<CatalogoCambiadoMessage>(new CatalogoCambiadoMessage());
}

public void EliminarUsuario(int id)
{
    using var db = new TiendaDbContext();
    var user = db.Usuarios.Find(id);
    if (user == null) return;

    // No permitir borrar al último admin
    if (user.Rol == "ADMIN" && db.Usuarios.Count(u => u.Rol == "ADMIN" && u.Activo) <= 1)
        throw new InvalidOperationException("No puedes eliminar al último administrador activo.");

    // Borrado lógico: solo lo desactivamos
    user.Activo = false;
    db.SaveChanges();
    WeakReferenceMessenger.Default.Send<CatalogoCambiadoMessage>(new CatalogoCambiadoMessage());

    
}
   
}