using TiendaAbarrotes.Data;
using TiendaAbarrotes.Models;

namespace TiendaAbarrotes.Services;

public class ConfiguracionService
{
    public bool HayConfiguracion()
    {
        using var db = new TiendaDbContext();
        return db.ConfiguracionTienda.Any();
    }

    public ConfiguracionTienda? Obtener()
    {
        using var db = new TiendaDbContext();
        return db.ConfiguracionTienda.FirstOrDefault();
    }

    public ConfiguracionTienda Guardar(ConfiguracionTienda config)
    {
        using var db = new TiendaDbContext();

        var existente = db.ConfiguracionTienda.FirstOrDefault();
        if (existente == null)
        {
            config.Id = 1;
            db.ConfiguracionTienda.Add(config);
        }
        else
        {
            existente.NombreTienda = config.NombreTienda;
            existente.Rfc          = config.Rfc;
            existente.Direccion    = config.Direccion;
            existente.Telefono     = config.Telefono;
            existente.Email        = config.Email;
            config = existente;
        }

        db.SaveChanges();
        return config;
    }
}