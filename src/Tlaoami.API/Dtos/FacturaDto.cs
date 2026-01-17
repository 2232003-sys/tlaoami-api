using System;
using System.Collections.Generic;
using Tlaoami.Domain.Enums;

namespace Tlaoami.API.Dtos
{
    public class FacturaDto
    {
        public Guid Id { get; set; }
        public string NumeroFactura { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; }
        public ICollection<PagoDto> Pagos { get; set; }
    }
}
