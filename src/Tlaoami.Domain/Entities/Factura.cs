using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities
{
    public class Factura
    {
        public Guid Id { get; set; }

        public Guid AlumnoId { get; set; }
        public Alumno Alumno { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroFactura { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        public DateTime FechaEmision { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public EstadoFactura Estado { get; set; }

        public ICollection<Pago> Pagos { get; set; }
    }
}
