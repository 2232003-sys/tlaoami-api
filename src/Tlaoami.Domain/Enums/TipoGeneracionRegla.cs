namespace Tlaoami.Domain.Enums
{
    /// <summary>
    /// Define el tipo de generación de cargos/facturas.
    /// </summary>
    public enum TipoGeneracionRegla
    {
        Unica = 0,      // Se genera una sola vez
        Mensual = 1,    // Se genera cada mes
        Anual = 2       // Se genera una vez por año
    }
}
