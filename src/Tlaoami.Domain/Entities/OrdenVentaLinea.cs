using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Línea de detalle de una orden de venta.
    /// Representa un producto/servicio con cantidad y precio.
    /// </summary>
    public class OrdenVentaLinea
    {
        public Guid Id { get; set; }
        
        public Guid OrdenVentaId { get; set; }
        public OrdenVenta? OrdenVenta { get; set; }
        
        /// <summary>
        /// ID del concepto de cobro (producto/servicio).
        /// Reutilizamos ConceptoCobro para catalogar productos.
        /// </summary>
        public Guid ProductoId { get; set; }
        public ConceptoCobro? Producto { get; set; }
        
        public int Cantidad { get; set; }
        
        public decimal PrecioUnitario { get; set; }
        
        /// <summary>
        /// Subtotal de la línea = Cantidad * PrecioUnitario
        /// </summary>
        public decimal Subtotal { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }
    }
}
