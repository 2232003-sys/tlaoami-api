using System;

namespace Tlaoami.Domain.Entities
{
    public class ReceptorFiscal
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public required string Rfc { get; set; } // RFC del tutor/padre (formato validado)
        public required string NombreFiscal { get; set; } // Razón social
        public required string CodigoPostalFiscal { get; set; } // CP fiscal (5 dígitos)
        public required string RegimenFiscal { get; set; } // e.g., "603" (Personas físicas)
        public string? UsoCfdiDefault { get; set; } // e.g., "P0000000" (sin especificar)
        public string? Email { get; set; } // Email para notificaciones
        public bool Activo { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        // Relationships
        public Alumno? Alumno { get; set; }
    }
}
