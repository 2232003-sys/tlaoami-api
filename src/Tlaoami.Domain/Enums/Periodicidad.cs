namespace Tlaoami.Domain.Enums
{
    /// <summary>
    /// Define la frecuencia de un concepto de cobro.
    /// Null si el concepto no es periódico (ej: reinscripción = única).
    /// </summary>
    public enum Periodicidad
    {
        Unica = 0,      // Se cobra una sola vez
        Mensual = 1,    // Se cobra cada mes
        Anual = 2       // Se cobra una vez por año
    }
}
