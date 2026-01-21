using System;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Dtos
{
    public class PagoDto
    {
        public Guid Id { get; set; }
        public Guid? FacturaId { get; set; }
        public Guid? AlumnoId { get; set; }
        public string? IdempotencyKey { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string? Metodo { get; set; }
    }
}
