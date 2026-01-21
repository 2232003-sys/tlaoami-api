using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Línea de detalle para una factura.
    /// </summary>
    public class FacturaLinea
    {
        public Guid Id { get; set; }
        public Guid FacturaId { get; set; }
        public Factura? Factura { get; set; }
        public Guid? ConceptoCobroId { get; set; }
        public ConceptoCobro? ConceptoCobro { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
