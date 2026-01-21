using System;

namespace Tlaoami.Application.Dtos
{
    public class ReglaColegiaturaCreateDto
    {
        public Guid CicloId { get; set; }
        public Guid? GrupoId { get; set; }
        public int? Grado { get; set; }
        public string? Turno { get; set; }
        public Guid ConceptoCobroId { get; set; }
        public decimal MontoBase { get; set; }
        public int DiaVencimiento { get; set; }
        public bool Activa { get; set; } = true;
    }

    public class ReglaColegiaturaUpdateDto
    {
        public Guid? GrupoId { get; set; }
        public int? Grado { get; set; }
        public string? Turno { get; set; }
        public decimal? MontoBase { get; set; }
        public int? DiaVencimiento { get; set; }
        public bool? Activa { get; set; }
    }

    public class ReglaColegiaturaDto
    {
        public Guid Id { get; set; }
        public Guid CicloId { get; set; }
        public Guid? GrupoId { get; set; }
        public int? Grado { get; set; }
        public string? Turno { get; set; }
        public Guid ConceptoCobroId { get; set; }
        public decimal MontoBase { get; set; }
        public int DiaVencimiento { get; set; }
        public bool Activa { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
