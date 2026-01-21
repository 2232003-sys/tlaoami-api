using System;

namespace Tlaoami.Application.Dtos
{
    public class AdeudoDto
    {
        public Guid AlumnoId { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Grupo { get; set; }
        public int? Grado { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal Saldo { get; set; }
        public DateTime? UltimoPagoAtUtc { get; set; }
    }

    public class PagoReporteDto
    {
        public Guid PagoId { get; set; }
        public DateTime FechaUtc { get; set; }
        public Guid? AlumnoId { get; set; }
        public string? AlumnoNombre { get; set; }
        public Guid? FacturaId { get; set; }
        public decimal Monto { get; set; }
        public string Metodo { get; set; } = string.Empty;
        public string? Referencia { get; set; }
        public Guid? CapturadoPorUserId { get; set; }
    }
}
