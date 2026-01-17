using System;

namespace Tlaoami.Domain.Entities
{
    public class Pago
    {
        public Guid Id { get; set; }
        public Guid FacturaId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public MetodoPago Metodo { get; set; }
        public Factura? Factura { get; set; }
    }

    public enum MetodoPago
    {
        Tarjeta,
        Transferencia,
        Efectivo
    }
}
