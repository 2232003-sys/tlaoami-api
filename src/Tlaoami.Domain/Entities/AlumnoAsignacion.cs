using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Asignación de conceptos de cobro a un alumno dentro de un ciclo escolar.
    /// Define "qué debe pagar"; no genera cargos automáticamente.
    /// </summary>
    public class AlumnoAsignacion
    {
        public Guid Id { get; set; }

        public Guid AlumnoId { get; set; }
        public Alumno? Alumno { get; set; }

        public Guid ConceptoCobroId { get; set; }
        public ConceptoCobro? ConceptoCobro { get; set; }

        public Guid CicloId { get; set; }
        public CicloEscolar? CicloEscolar { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; } // null = vigente

        /// <summary>
        /// Permite sobrescribir el monto base del concepto para este alumno.
        /// Null = usar regla/monto base del concepto/colegiatura.
        /// </summary>
        public decimal? MontoOverride { get; set; }

        /// <summary>
        /// Activo indica si la asignación está vigente.
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Timestamps de auditoría.
        /// </summary>
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
