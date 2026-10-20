using CommunityToolkit.Mvvm.ComponentModel;

namespace TiendaAbarrotes.ViewModels;

public partial class DenominacionItem : ObservableObject
{
    public decimal Valor { get; set; }

    // Texto a mostrar (sin decimales si es billete, con decimales si es moneda)
    public string ValorTexto => Valor >= 1 ? $"${Valor:N0}" : $"${Valor:N2}";
    public string Tipo => Valor >= 20 ? "Billete" : "Moneda";

    [ObservableProperty] private string _cantidadTexto = "0";

    public int Cantidad
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CantidadTexto)) return 0;
            return int.TryParse(CantidadTexto, out var n) && n >= 0 ? n : 0;
        }
    }

    public int TotalCentavos => (int)(Valor * 100) * Cantidad;
    public string TotalTexto => (TotalCentavos / 100m).ToString("C2");

    partial void OnCantidadTextoChanged(string value)
    {
        OnPropertyChanged(nameof(Cantidad));
        OnPropertyChanged(nameof(TotalCentavos));
        OnPropertyChanged(nameof(TotalTexto));
    }
}