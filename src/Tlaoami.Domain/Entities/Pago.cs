using System;

namespace Tlaoami.Domain.Entities
{
    public class Pago
    {
        public Guid Id { get; set; }
        public Guid? FacturaId { get; set; }
        public Guid? AlumnoId { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public MetodoPago Metodo { get; set; }
        public Guid? PaymentIntentId { get; set; }
        public Factura? Factura { get; set; }
    }

    public enum MetodoPago
    {
        Tarjeta,
        Transferencia,
        Efectivo
    }
}
