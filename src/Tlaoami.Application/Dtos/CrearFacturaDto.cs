using System;
using System.ComponentModel.DataAnnotations;

namespace Tlaoami.Application.Dtos
{
    public class CrearFacturaDto
    {
        [Required]
        public Guid AlumnoId { get; set; }
        
        [Required]
        public decimal Monto { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Concepto { get; set; } = string.Empty;
        
        [Required]
        public DateTime FechaEmision { get; set; }
        
        public DateTime? FechaVencimiento { get; set; }

        public string? Periodo { get; set; }

        public Guid? ConceptoCobroId { get; set; }
    }
}
