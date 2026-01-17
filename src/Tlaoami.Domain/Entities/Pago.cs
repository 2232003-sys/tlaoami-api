using System;
using System.ComponentModel.DataAnnotations.Schema;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities
{
    public class Pago
    {
        public Guid Id { get; set; }

        public Guid FacturaId { get; set; }
        public Factura Factura { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        public MetodoPago Metodo { get; set; }
    }
}
