using System.Windows;
using TiendaAbarrotes.Data;
using TiendaAbarrotes.Services;
using TiendaAbarrotes.Views;

namespace TiendaAbarrotes;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            // 1. Respaldo automático AL ARRANCAR (antes de abrir la BD)
            new BackupService().CrearRespaldo();

            // 2. Inicializar BD
            using (var db = new TiendaDbContext())
            {
                DatabaseInitializer.Initialize(db);
                // DatabaseInitializer.SeedTestData(db); // Quitar en producción
            }

            // ... lógica de Setup / Login / MainWindow (sin cambios)
            
            var auth = new AuthService();
            var config = new ConfiguracionService();

            if (!config.HayConfiguracion() || !auth.HayUsuarios())
            {
                var setup = new SetupWizard();
                setup.ShowDialog();
                if (!setup.Completado) { Shutdown(); return; }
            }

            var login = new LoginWindow();
            login.ShowDialog();
            if (!login.Autenticado) { Shutdown(); return; }

            var main = new MainWindow();
            MainWindow = main;
            main.Show();

            ShutdownMode = ShutdownMode.OnMainWindowClose;

            // 3. Respaldo automático AL CERRAR
            main.Closed += (s, args) =>
            {
                new BackupService().CrearRespaldo();
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error crítico al iniciar:\n\n{ex.Message}", "Error de arranque", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}