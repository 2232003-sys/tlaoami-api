using System;
using System.Collections.Generic;

namespace Tlaoami.Application.Dtos
{
    public class FacturaDto
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public string? NumeroFactura { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string? Estado { get; set; }
    }
}
