using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities;

/// <summary>
/// Vínculo de conciliación entre un pago reportado y un movimiento bancario
/// (propuesto por sistema o confirmado por usuario)
/// </summary>
public class ConciliacionMatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EscuelaId { get; set; }
    
    /// <summary>
    /// Pago reportado (puede ser null si es puro movimiento bancario no reportado)
    /// </summary>
    public Guid? PagoReportadoId { get; set; }
    
    /// <summary>
    /// Movimiento bancario (puede ser null si es reportado sin movimiento aún)
    /// </summary>
    public Guid? MovimientoBancarioId { get; set; }
    
    /// <summary>
    /// Alumno (redundante para queries rápidas)
    /// </summary>
    public Guid AlumnoId { get; set; }

    /// <summary>
    /// Score de confianza (0-100)
    /// 100 = Manual / muy confiable
    /// 90 = Monto exacto + fecha correcta
    /// 70 = Referencia contiene dato del alumno
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Regla que generó el match: "monto+fecha", "referencia", "manual", etc.
    /// </summary>
    public string? ReglaMatch { get; set; }

    /// <summary>
    /// Estado del match
    /// </summary>
    public EstatusConciliacionMatch Estatus { get; set; } = EstatusConciliacionMatch.Propuesto;

    /// <summary>
    /// Usuario que confirmó (si aplica)
    /// </summary>
    public Guid? ConfirmadoPorUsuarioId { get; set; }

    /// <summary>
    /// Fecha de confirmación
    /// </summary>
    public DateTime? ConfirmedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Alumno? Alumno { get; set; }
    public PagoReportado? PagoReportado { get; set; }
    public MovimientoBancario? MovimientoBancario { get; set; }
}
