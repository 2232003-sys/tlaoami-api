using System;

namespace Tlaoami.Application.Dtos
{
    public class ReglaRecargoCreateDto
    {
        public Guid CicloId { get; set; }
        public Guid ConceptoCobroId { get; set; }
        public int DiasGracia { get; set; }
        public decimal Porcentaje { get; set; }
        public bool Activa { get; set; } = true;
    }

    public class ReglaRecargoUpdateDto
    {
        public int? DiasGracia { get; set; }
        public decimal? Porcentaje { get; set; }
        public bool? Activa { get; set; }
    }

    public class ReglaRecargoDto
    {
        public Guid Id { get; set; }
        public Guid CicloId { get; set; }
        public Guid ConceptoCobroId { get; set; }
        public int DiasGracia { get; set; }
        public decimal Porcentaje { get; set; }
        public bool Activa { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
