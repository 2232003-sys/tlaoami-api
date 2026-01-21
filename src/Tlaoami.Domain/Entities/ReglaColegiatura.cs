using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Regla específica para colegiaturas por ciclo/grupo.
    /// </summary>
    public class ReglaColegiatura
    {
        public Guid Id { get; set; }
        public Guid CicloId { get; set; }
        public CicloEscolar? CicloEscolar { get; set; }
        public Guid? GrupoId { get; set; }
        public Grupo? Grupo { get; set; }
        public int? Grado { get; set; }
        public string? Turno { get; set; }
        public Guid ConceptoCobroId { get; set; }
        public ConceptoCobro? ConceptoCobro { get; set; }
        public decimal MontoBase { get; set; }
        public int DiaVencimiento { get; set; }
        public bool Activa { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
