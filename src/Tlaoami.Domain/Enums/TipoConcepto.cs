namespace Tlaoami.Domain.Enums
{
    /// <summary>
    /// Tipo del concepto de cobro.
    /// Recurrente: se cobra de forma periódica (ej. mensual).
    /// Unico: se cobra una sola vez.
    /// Producto: venta de producto/servicio no recurrente.
    /// </summary>
    public enum TipoConcepto
    {
        Recurrente = 0,
        Unico = 1,
        Producto = 2
    }
}
