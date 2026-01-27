namespace Tlaoami.Domain.Enums;

/// <summary>
/// Estado del pago reportado
/// </summary>
public enum EstatusPagoReportado
{
    PendienteBanco = 0,      // Reportado, esperando movimiento bancario
    MatchPropuesto = 1,      // Se encontró un movimiento candidato
    Conciliado = 2,          // Confirmado y aplicado
    Rechazado = 3,           // No se encontró match válido
    Cancelado = 4,           // Anulado por error o solicitud
}
