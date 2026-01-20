using System;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Regla de cobro por ciclo: Define cuándo y cómo se cobran conceptos en un ciclo.
    /// 
    /// Ejemplo:
    /// - Ciclo 2026-1, Grado 1, Concepto "COLEGIATURA", Mensual, MontoBase 5000
    /// - Genera la intención de cobrar $5000 mensuales a alumnos de Grado 1 en ese ciclo.
    /// 
    /// Esta entidad NO crea facturas/cargos automáticamente. Solo define reglas.
    /// La generación efectiva es responsabilidad de un futuro servicio de batch/scheduler.
    /// </summary>
    public class ReglaCobroPorCiclo
    {
        /// <summary>Identificador único (UUID)</summary>
        public Guid Id { get; set; }

        /// <summary>Ciclo escolar al que aplica la regla (requerido)</summary>
        public Guid CicloId { get; set; }

        /// <summary>
        /// Grado al que aplica (1..6 para primaria, nullable = aplica a todos).
        /// Permite reglas específicas por grado.
        /// </summary>
        public int? Grado { get; set; }

        /// <summary>
        /// Turno al que aplica (Matutino/Vespertino, nullable = aplica a todos).
        /// Permite reglas específicas por turno.
        /// </summary>
        public string? Turno { get; set; }

        /// <summary>Concepto de cobro a aplicar (requerido, FK)</summary>
        public Guid ConceptoCobroId { get; set; }

        /// <summary>
        /// Tipo de generación: Unica (una sola vez), Mensual (cada mes), Anual (una vez por año).
        /// Define la frecuencia de cobro.
        /// </summary>
        public TipoGeneracionRegla TipoGeneracion { get; set; }

        /// <summary>
        /// Día de corte para cobros periódicos (1..28, nullable si TipoGeneracion=Unica).
        /// Ej: DiaCorte=5 → Se cobran los días 5 de cada mes.
        /// </summary>
        public int? DiaCorte { get; set; }

        /// <summary>
        /// Monto base sugerido para la generación de cargos.
        /// Puede ser ajustado posteriormente (ej: por promociones, descuentos).
        /// </summary>
        public decimal MontoBase { get; set; }

        /// <summary>
        /// Indica si la regla está activa.
        /// Soft delete: marcar como inactiva en lugar de eliminar.
        /// Default: true.
        /// </summary>
        public bool Activa { get; set; } = true;

        /// <summary>Timestamp de creación en UTC</summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>Timestamp de última actualización en UTC (nullable)</summary>
        public DateTime? UpdatedAtUtc { get; set; }

        // Propiedades de navegación (lazily loaded si es necesario)
        public CicloEscolar? CicloEscolar { get; set; }
        public ConceptoCobro? ConceptoCobro { get; set; }
    }
}
