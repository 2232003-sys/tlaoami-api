using Tlaoami.Domain.Enums;

namespace Tlaoami.Application.Dtos
{
    /// <summary>DTO para crear un nuevo Concepto de Cobro</summary>
    public class ConceptoCobroCreateDto
    {
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public Periodicidad? Periodicidad { get; set; }
        public bool RequiereCFDI { get; set; } = true;
        public bool Activo { get; set; } = true;
        public int Orden { get; set; } = 0;
    }

    /// <summary>DTO para actualizar un Concepto de Cobro (no se puede cambiar Clave)</summary>
    public class ConceptoCobroUpdateDto
    {
        public string? Nombre { get; set; }
        public Periodicidad? Periodicidad { get; set; }
        public bool? RequiereCFDI { get; set; }
        public bool? Activo { get; set; }
        public int? Orden { get; set; }
    }

    /// <summary>DTO de respuesta para Concepto de Cobro</summary>
    public class ConceptoCobroDto
    {
        public Guid Id { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public Periodicidad? Periodicidad { get; set; }
        public bool RequiereCFDI { get; set; }
        public bool Activo { get; set; }
        public int Orden { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
