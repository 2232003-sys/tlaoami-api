using System;
using System.Collections.Generic;

namespace Tlaoami.Application.Dtos
{
    /// <summary>
    /// DTO detallado de una factura con información del alumno y pagos
    /// </summary>
    public class FacturaDetalleDto
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public string? AlumnoNombreCompleto { get; set; }
        public string? NumeroFactura { get; set; }
        public decimal Monto { get; set; }
        public decimal Saldo { get; set; }
        public decimal TotalPagado { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string? Estado { get; set; }
        public List<PagoDto> Pagos { get; set; } = new();
    }
}
