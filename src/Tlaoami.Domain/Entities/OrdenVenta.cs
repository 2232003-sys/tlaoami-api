using System;
using System.Collections.Generic;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Orden de venta para productos/servicios escolares.
    /// Representa una transacción comercial que, al confirmarse, genera un cargo financiero.
    /// </summary>
    public class OrdenVenta
    {
        public Guid Id { get; set; }
        
        public Guid AlumnoId { get; set; }
        public Alumno? Alumno { get; set; }
        
        public DateTime Fecha { get; set; }
        
        public EstatusOrdenVenta Estatus { get; set; } = EstatusOrdenVenta.Borrador;
        
        /// <summary>
        /// Total calculado de la orden (suma de líneas).
        /// </summary>
        public decimal Total { get; set; }
        
        /// <summary>
        /// Notas adicionales de la orden.
        /// </summary>
        public string? Notas { get; set; }
        
        /// <summary>
        /// Referencia a la factura generada al confirmar la orden.
        /// </summary>
        public Guid? FacturaId { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public DateTime? ConfirmadaAtUtc { get; set; }
        
        public ICollection<OrdenVentaLinea> Lineas { get; set; } = new List<OrdenVentaLinea>();
    }
}
