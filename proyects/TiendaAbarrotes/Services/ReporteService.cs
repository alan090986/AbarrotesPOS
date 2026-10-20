using Microsoft.EntityFrameworkCore;
using TiendaAbarrotes.Data;

namespace TiendaAbarrotes.Services;

public class ReporteService
{
    public class VentaReporteItem
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = "";
        public string MetodoPago { get; set; } = "";
        public int Total { get; set; }
        public string TotalTexto => (Total / 100m).ToString("C2");
        public string FechaTexto => Fecha.ToString("dd/MM/yyyy HH:mm");
    }

    public List<VentaReporteItem> ObtenerVentas(DateTime inicio, DateTime fin, int? idUsuario = null)
    {
        using var db = new TiendaDbContext();
        
        // Ajustar fin para que incluya todo el día
        var finDelDia = fin.Date.AddDays(1).AddSeconds(-1);

        var query = db.Ventas
            .Include(v => v.Usuario)
            .Where(v => v.Estado == "COMPLETADA" && v.Fecha >= inicio.Date && v.Fecha <= finDelDia);

        if (idUsuario.HasValue && idUsuario.Value > 0)
            query = query.Where(v => v.IdUsuario == idUsuario.Value);

        return query.OrderByDescending(v => v.Fecha)
            .Select(v => new VentaReporteItem
            {
                IdVenta = v.IdVenta,
                Fecha = v.Fecha,
                Usuario = v.Usuario.Nombre,
                MetodoPago = v.MetodoPago,
                Total = v.Total
            })
            .ToList();
    }

    public int TotalVentas(DateTime inicio, DateTime fin, int? idUsuario = null)
    {
        using var db = new TiendaDbContext();
        var finDelDia = fin.Date.AddDays(1).AddSeconds(-1);

        var query = db.Ventas
            .Where(v => v.Estado == "COMPLETADA" && v.Fecha >= inicio.Date && v.Fecha <= finDelDia);

        if (idUsuario.HasValue && idUsuario.Value > 0)
            query = query.Where(v => v.IdUsuario == idUsuario.Value);

        return query.Sum(v => (int?)v.Total) ?? 0;
    }
}