using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TiendaAbarrotes.Services;


namespace TiendaAbarrotes.ViewModels.Modules;

public partial class VentasViewModel : ObservableObject
{
    private readonly VentaService _ventaService = new();
    private readonly CajaService _cajaService = new();

    [ObservableProperty] private bool _cajaAbierta;
    [ObservableProperty] private string _cajaEstado = "Caja cerrada — Abre turno para vender";
    private int? _idCajaActual;

    public ObservableCollection<CartItemViewModel> Carrito { get; } = new();

    [ObservableProperty] private string _codigoActual = "";
    [ObservableProperty] private string _metodoPago = "EFECTIVO";
    [ObservableProperty] private string _mensaje = "";
    [ObservableProperty] private bool _mensajeEsError;

    public int TotalCentavos => Carrito.Sum(i => i.Subtotal);
    public string TotalFormateado => (TotalCentavos / 100m).ToString("C2");
    public int TotalArticulos => Carrito.Sum(i => i.Cantidad);

    public VentasViewModel()
    {
        Carrito.CollectionChanged += (_, _) => RecalcularTotales();
        VerificarCaja();
    }

    private void VerificarCaja()
    {
        var caja = _cajaService.ObtenerCajaAbierta();
        if (caja != null)
        {
            _idCajaActual = caja.IdCaja;
            CajaAbierta = true;
            CajaEstado = $"Turno #{caja.IdCaja} — Abierto a las {caja.FechaApertura:HH:mm}";
        }
        else
        {
            _idCajaActual = null;
            CajaAbierta = false;
            CajaEstado = "Caja cerrada — Abre turno para vender";
        }
    }

    public void ProcesarCodigo()
    {
        var codigo = CodigoActual?.Trim();
        CodigoActual = "";
        if (string.IsNullOrEmpty(codigo)) return;

        var producto = _ventaService.BuscarPorCodigoBarras(codigo);

        if (producto == null)
        {
            Mensaje = $"Producto '{codigo}' no encontrado";
            MensajeEsError = true;
            return;
        }

        var existente = Carrito.FirstOrDefault(i => i.IdProducto == producto.IdProducto);
        if (existente != null)
        {
            existente.Cantidad++;
        }
        else
        {
            var item = new CartItemViewModel
            {
                IdProducto     = producto.IdProducto,
                CodigoBarras   = producto.CodigoBarras,
                Nombre         = producto.Nombre,
                PrecioUnitario = producto.PrecioVenta
            };
            item.PropertyChanged += (_, _) => RecalcularTotales();
            Carrito.Add(item);
        }

        Mensaje = $"+ {producto.Nombre}";
        MensajeEsError = false;
    }

    [RelayCommand]
    private void QuitarItem(CartItemViewModel item) => Carrito.Remove(item);

    [RelayCommand]
    private void LimpiarCarrito()
    {
        Carrito.Clear();
        Mensaje = "";
    }

    [RelayCommand]
    private void Cobrar()
    {
        if (Carrito.Count == 0)
        {
            Mensaje = "El carrito está vacío";
            MensajeEsError = true;
            return;
        }

        if (!CajaAbierta || _idCajaActual == null)
        {
            Mensaje = "Debes abrir la caja antes de vender";
            MensajeEsError = true;
            return;
        }

        var modal = new Views.PagoModal(TotalCentavos)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        modal.ShowDialog();

        if (!modal.Confirmado) return;

        try
        {
            var idVenta = _ventaService.RegistrarVenta(Carrito, modal.MetodoPago, Session.IdUsuarioActual, _idCajaActual);
            Mensaje = $"Venta #{idVenta} registrada — Total: {TotalFormateado} — Cambio: {(modal.CambioCentavos / 100m):C2}";
            MensajeEsError = false;
            Carrito.Clear();
        }
        catch (Exception ex)
        {
            Mensaje = $"Error: {ex.Message} | {ex.InnerException?.Message}";
            MensajeEsError = true;
        }
    }

    [RelayCommand]
private void AbrirCaja()
{
    var modal = new Views.AbrirCajaModal { Owner = System.Windows.Application.Current.MainWindow };
    modal.ShowDialog();

    if (!modal.Confirmado) return;

    try
    {
        var caja = _cajaService.AbrirCaja(modal.MontoInicialCentavos, Session.IdUsuarioActual, modal.DesgloseResultado);
        _idCajaActual = caja.IdCaja;
        CajaAbierta = true;
        CajaEstado = $"Turno #{caja.IdCaja} — Abierto a las {caja.FechaApertura:HH:mm}";
        Mensaje = $"Caja abierta con {(modal.MontoInicialCentavos / 100m):C2}";
        MensajeEsError = false;
    }
    catch (Exception ex)
    {
        Mensaje = $"Error: {ex.Message}";
        MensajeEsError = true;
    }
}

[RelayCommand]
private void CerrarCaja()
    {
        if (_idCajaActual == null)
        {
            Mensaje = "No hay caja abierta";
            MensajeEsError = true;
            return;
        }

        // 🔒 Validar si es cajero: pedir autorización de Admin
        if (!Session.EsAdmin)
        {
            var authModal = new Views.AdminAuthModal
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            authModal.ShowDialog();

            if (!authModal.Autorizado)
            {
                Mensaje = "Cierre de caja cancelado. Se requiere autorización de administrador.";
                MensajeEsError = true;
                return;
            }
        }

        // Si es admin (o ya autorizó), abrir el modal de cierre normal
        var modal = new Views.CerrarCajaModal(_idCajaActual.Value)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        modal.ShowDialog();

        if (!modal.Confirmado) return;

        try
        {
            var caja = _cajaService.CerrarCaja(_idCajaActual.Value, modal.MontoRealCentavos, Session.IdUsuarioActual, modal.Observaciones, modal.DesgloseResultado);
            _idCajaActual = null;
            CajaAbierta = false;
            CajaEstado = "Caja cerrada — Abre turno para vender";
            
            var dif = caja.Diferencia!.Value / 100m;
            var signo = dif > 0 ? "Sobrante" : (dif < 0 ? "Faltante" : "Cuadre perfecto");
            
            System.Windows.MessageBox.Show(
                $"Cierre de caja exitoso.\n\n" +
                $"Monto Esperado: {(caja.MontoEsperado / 100m):C2}\n" +
                $"Monto Contado: {(caja.MontoReal!.Value / 100m):C2}\n" +
                $"Diferencia: {dif:C2} ({signo})",
                "Corte de Caja",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            Mensaje = $"Caja cerrada — Diferencia: {dif:C2}";
            MensajeEsError = false;
        }
        catch (Exception ex)
        {
            Mensaje = $"Error: {ex.Message}";
            MensajeEsError = true;
        }
    }

    private void RecalcularTotales()
    {
        OnPropertyChanged(nameof(TotalCentavos));
        OnPropertyChanged(nameof(TotalFormateado));
        OnPropertyChanged(nameof(TotalArticulos));
    }
}