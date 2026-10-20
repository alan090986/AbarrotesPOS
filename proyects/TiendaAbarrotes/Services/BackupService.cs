using System.IO;

namespace TiendaAbarrotes.Services;

public class BackupService
{
    private readonly string _dbPath;
    private readonly string _backupFolder;

    public BackupService()
    {
        // La misma ruta que usa tu TiendaDbContext
        _dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TiendaAbarrotes", "tienda.db");

        _backupFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TiendaAbarrotes", "Backups");
    }

    public string RutaCarpetaRespaldos => _backupFolder;

    public void CrearRespaldo()
    {
        try
        {
            // Si no existe la BD, no hay nada que respaldar
            if (!File.Exists(_dbPath)) return;

            // Crear carpeta de respaldos si no existe
            if (!Directory.Exists(_backupFolder))
                Directory.CreateDirectory(_backupFolder);

            // Nombre del archivo: tienda_20261017_1530.db
            var nombreArchivo = $"tienda_{DateTime.Now:yyyyMMdd_HHmmss}.db";
            var destino = Path.Combine(_backupFolder, nombreArchivo);

            // Copiar el archivo
            File.Copy(_dbPath, destino, overwrite: true);

            // Limpiar respaldos viejos (mantener solo los últimos 30)
            LimpiarRespaldosAntiguos();
        }
        catch (Exception ex)
        {
            // No queremos que un error de respaldo tumbe la app
            System.Diagnostics.Debug.WriteLine($"Error al crear respaldo: {ex.Message}");
        }
    }

    private void LimpiarRespaldosAntiguos()
    {
        try
        {
            var archivos = new DirectoryInfo(_backupFolder)
                .GetFiles("tienda_*.db")
                .OrderByDescending(f => f.CreationTime)
                .Skip(30) // Conservar los 30 más recientes
                .ToList();

            foreach (var archivo in archivos)
            {
                archivo.Delete();
            }
        }
        catch
        {
            // Ignorar errores de limpieza
        }
    }
}