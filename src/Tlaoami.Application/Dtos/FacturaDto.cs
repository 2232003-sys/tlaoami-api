using System;

namespace Tlaoami.Application.Dtos
{
    public class FacturaDto
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public string? NumeroFactura { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string? Periodo { get; set; }
        public Guid? ConceptoCobroId { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public decimal Saldo { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string? Estado { get; set; }
        public string? ReciboFolio { get; set; }
        public DateTime? ReciboEmitidoAtUtc { get; set; }
    }
}
