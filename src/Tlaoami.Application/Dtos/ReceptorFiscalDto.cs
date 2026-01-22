using System;

namespace Tlaoami.Application.Dtos
{
    // Response DTO
    public class ReceptorFiscalDto
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public required string Rfc { get; set; }
        public required string NombreFiscal { get; set; }
        public required string CodigoPostalFiscal { get; set; }
        public required string RegimenFiscal { get; set; }
        public string? UsoCfdiDefault { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    // Create/Update DTO
    public class ReceptorFiscalUpsertDto
    {
        public required string Rfc { get; set; }
        public required string NombreFiscal { get; set; }
        public required string CodigoPostalFiscal { get; set; }
        public required string RegimenFiscal { get; set; } // "603", "605", "606", etc.
        public string? UsoCfdiDefault { get; set; } // "P0000000", "G01000000", etc.
        public string? Email { get; set; }
    }
}
