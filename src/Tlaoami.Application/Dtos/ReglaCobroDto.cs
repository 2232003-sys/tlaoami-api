using Tlaoami.Domain.Enums;

namespace Tlaoami.Application.Dtos
{
    /// <summary>DTO para crear una nueva Regla de Cobro por Ciclo</summary>
    public class ReglaCobroCreateDto
    {
        public Guid CicloId { get; set; }
        public int? Grado { get; set; }
        public string? Turno { get; set; }
        public Guid ConceptoCobroId { get; set; }
        public TipoGeneracionRegla TipoGeneracion { get; set; }
        public int? DiaCorte { get; set; }
        public decimal MontoBase { get; set; }
        public bool Activa { get; set; } = true;
    }

    /// <summary>DTO para actualizar una Regla de Cobro por Ciclo</summary>
    public class ReglaCobroUpdateDto
    {
        public int? Grado { get; set; }
        public string? Turno { get; set; }
        public TipoGeneracionRegla? TipoGeneracion { get; set; }
        public int? DiaCorte { get; set; }
        public decimal? MontoBase { get; set; }
        public bool? Activa { get; set; }
    }

    /// <summary>DTO de respuesta para Regla de Cobro por Ciclo</summary>
    public class ReglaCobroDto
    {
        public Guid Id { get; set; }
        public Guid CicloId { get; set; }
        public int? Grado { get; set; }
        public string? Turno { get; set; }
        public Guid ConceptoCobroId { get; set; }
        public TipoGeneracionRegla TipoGeneracion { get; set; }
        public int? DiaCorte { get; set; }
        public decimal MontoBase { get; set; }
        public bool Activa { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
