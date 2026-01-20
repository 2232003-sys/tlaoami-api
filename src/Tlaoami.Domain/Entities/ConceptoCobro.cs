using System;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Concepto de cobro: define "por qué" se genera una carga/factura.
    /// Ejemplos: Colegiatura, Reinscripción, Actividad Deportiva.
    /// Actúa como catálogo: Define qué puede cobrarse.
    /// </summary>
    public class ConceptoCobro
    {
        /// <summary>Identificador único (UUID)</summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Código único del concepto. Ej: "COLEGIATURA", "REINSCRIPCION", "DEPORTES".
        /// Case-insensitive unique (aplicar en nivel de BD con índice).
        /// Longitud: 3-30 caracteres.
        /// </summary>
        public string Clave { get; set; } = string.Empty;

        /// <summary>
        /// Nombre amigable del concepto. Ej: "Colegiatura Mensual", "Cuota de Reinscripción".
        /// Longitud: 3-120 caracteres.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Define si el concepto es periódico (Mensual, Anual) o de una sola vez (Unica).
        /// Nullable: si es null, implica que el concepto es ad-hoc (no periódico).
        /// </summary>
        public Periodicidad? Periodicidad { get; set; }

        /// <summary>
        /// Indica si el concepto requiere ser reportado en CFDI (Comprobante Fiscal Digital).
        /// Default: true (la mayoría de conceptos escolares requieren CFDI).
        /// </summary>
        public bool RequiereCFDI { get; set; } = true;

        /// <summary>
        /// Indica si el concepto está activo.
        /// Soft delete: en lugar de eliminar, se marca como inactivo.
        /// Default: true.
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Orden de presentación en listados.
        /// Default: 0. Mayor valor = más prioridad en la UI.
        /// </summary>
        public int Orden { get; set; } = 0;

        /// <summary>Timestamp de creación en UTC</summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>Timestamp de última actualización en UTC (nullable)</summary>
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
