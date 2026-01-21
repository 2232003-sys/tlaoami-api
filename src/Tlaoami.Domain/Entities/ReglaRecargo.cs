using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Regla de recargos por mora aplicada a colegiaturas vencidas.
    /// </summary>
    public class ReglaRecargo
    {
        public Guid Id { get; set; }
        public Guid CicloId { get; set; }
        public CicloEscolar? CicloEscolar { get; set; }
        public Guid ConceptoCobroId { get; set; }
        public ConceptoCobro? ConceptoCobro { get; set; }
        public int DiasGracia { get; set; }
        public decimal Porcentaje { get; set; }
        public bool Activa { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
