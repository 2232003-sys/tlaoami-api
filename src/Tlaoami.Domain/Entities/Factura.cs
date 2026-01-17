using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tlaoami.Domain.Entities
{
    public class Factura
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public Alumno? Alumno { get; set; }

        [Required]
        public string? NumeroFactura { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }

        [Required]
        public EstadoFactura Estado { get; set; } = EstadoFactura.Pendiente;
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }

    public enum EstadoFactura
    {
        Pendiente,
        ParcialmentePagada,
        Pagada,
        Vencida
    }
}
