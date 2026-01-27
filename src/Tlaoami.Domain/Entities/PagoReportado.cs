using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities;

/// <summary>
/// Pago reportado por el alumno/tutor (WhatsApp, foto, comentario, etc.)
/// </summary>
public class PagoReportado
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EscuelaId { get; set; }
    public Guid AlumnoId { get; set; }

    /// <summary>
    /// Fecha que reportó el pago
    /// </summary>
    public DateTime FechaReportada { get; set; }

    /// <summary>
    /// Monto que asegura haber pagado
    /// </summary>
    public decimal MontoReportado { get; set; }

    /// <summary>
    /// Cómo dice que pagó (transferencia, efectivo, tarjeta, otro)
    /// </summary>
    public MetodoPagoReportado MetodoPago { get; set; }

    /// <summary>
    /// Lo que escribió el papá (ej: "Transfiriendo", "Pagué en Oxxo", etc.)
    /// </summary>
    public string? ReferenciaTexto { get; set; }

    /// <summary>
    /// URL de comprobante subido
    /// </summary>
    public string? ComprobanteUrl { get; set; }

    /// <summary>
    /// Notas internas
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// Estado en el flujo de conciliación
    /// </summary>
    public EstatusPagoReportado Estatus { get; set; } = EstatusPagoReportado.PendienteBanco;

    public Guid? CreadoPorUsuarioId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Alumno? Alumno { get; set; }
    public ICollection<ConciliacionMatch> Matches { get; set; } = new List<ConciliacionMatch>();
}
