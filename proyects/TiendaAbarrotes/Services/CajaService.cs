using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;
using TiendaAbarrotes.Models;
using CommunityToolkit.Mvvm.Messaging;
using TiendaAbarrotes.ViewModels;

namespace TiendaAbarrotes.Services;

public class CajaService
{
    /// <summary>Devuelve la caja abierta actual, o null si no hay ninguna.</summary>
    public Caja? ObtenerCajaAbierta()
    {
        using var db = new TiendaDbContext();
        return db.Cajas
            .Include(c => c.UsuarioApertura)
            .FirstOrDefault(c => c.Estado == "ABIERTA");
    }

    /// <summary>Abre un nuevo turno. Falla si ya hay una caja abierta.</summary>
   public Caja AbrirCaja(int montoInicialCentavos, int idUsuario, List<CajaDesglose>? desglose = null)
{
    using var db = new TiendaDbContext();

    var abierta = db.Cajas.FirstOrDefault(c => c.Estado == "ABIERTA");
    if (abierta != null)
        throw new InvalidOperationException($"Ya hay una caja abierta (turno #{abierta.IdCaja}).");

    var caja = new Caja
    {
        MontoInicial      = montoInicialCentavos,
        IdUsuarioApertura = idUsuario,
        Estado            = "ABIERTA",
        FechaApertura     = DateTime.Now
    };

    db.Cajas.Add(caja);
    db.SaveChanges(); // Guarda para generar el IdCaja

    // Guardar desglose de apertura
    if (desglose != null && desglose.Count > 0)
    {
        foreach (var d in desglose)
        {
            d.IdCaja = caja.IdCaja;
            d.Tipo = "APERTURA";
            db.CajaDesgloses.Add(d);
        }
        db.SaveChanges();
    }

    return caja;
}

    /// <summary>Cierra el turno calculando totales y diferencia.</summary>
    public Caja CerrarCaja(int idCaja, int montoRealCentavos, int idUsuario, string? observaciones, List<CajaDesglose>? desglose = null)
{
    using var db = new TiendaDbContext();

    var caja = db.Cajas.Find(idCaja) ?? throw new InvalidOperationException("Caja no encontrada.");
    if (caja.Estado != "ABIERTA") throw new InvalidOperationException("Esta caja ya está cerrada.");

    var ventas = db.Ventas
        .Where(v => v.IdCaja == idCaja && v.Estado == "COMPLETADA")
        .ToList();

    caja.TotalVentasEfectivo        = ventas.Where(v => v.MetodoPago == "EFECTIVO").Sum(v => v.Total);
    caja.TotalVentasTarjeta         = ventas.Where(v => v.MetodoPago == "TARJETA").Sum(v => v.Total);
    caja.TotalVentasTransferencia   = ventas.Where(v => v.MetodoPago == "TRANSFERENCIA").Sum(v => v.Total);

    caja.MontoEsperado      = caja.MontoInicial + caja.TotalVentasEfectivo;
    caja.MontoReal          = montoRealCentavos;
    caja.Diferencia         = montoRealCentavos - caja.MontoEsperado;
    caja.FechaCierre        = DateTime.Now;
    caja.IdUsuarioCierre    = idUsuario;
    caja.Estado             = "CERRADA";
    caja.Observaciones      = observaciones;

    // Guardar desglose de cierre
    if (desglose != null && desglose.Count > 0)
    {
        foreach (var d in desglose)
        {
            d.IdCaja = idCaja;
            d.Tipo = "CIERRE";
            db.CajaDesgloses.Add(d);
        }
    }

    db.SaveChanges();
    return caja;
}

    public List<Caja> ObtenerHistorial(int top = 20)
    {
        using var db = new TiendaDbContext();
        return db.Cajas
            .Include(c => c.UsuarioApertura)
            .Include(c => c.UsuarioCierre)
            .OrderByDescending(c => c.FechaApertura)
            .Take(top)
            .ToList();
    }
    
    
}