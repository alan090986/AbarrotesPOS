namespace TiendaAbarrotes.ViewModels;

/// <summary>Se emite cuando se registra una venta nueva en el POS.</summary>
public class VentaRegistradaMessage { }

/// <summary>Se emite cuando se registra un movimiento de stock (entrada, salida, ajuste).</summary>
public class StockCambiadoMessage { }

/// <summary>Se emite cuando se abre o cierra una caja.</summary>
public class CajaCambiadaMessage { }

/// <summary>Se emite cuando cambia el catálogo de productos o usuarios.</summary>
public class CatalogoCambiadoMessage { }