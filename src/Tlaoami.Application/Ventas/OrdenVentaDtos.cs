using System;
using System.Collections.Generic;

namespace Tlaoami.Application.Ventas
{
    public class OrdenVentaDto
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string? Notas { get; set; }
        public Guid? FacturaId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ConfirmadaAtUtc { get; set; }
        public List<OrdenVentaLineaDto> Lineas { get; set; } = new List<OrdenVentaLineaDto>();
    }

    public class OrdenVentaLineaDto
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class OrdenVentaCreateDto
    {
        public Guid AlumnoId { get; set; }
        public string? Notas { get; set; }
    }

    public class AgregarLineaDto
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; } // Optional: usar precio de ConceptoCobro si null
    }

    public class ConfirmarOrdenResultDto
    {
        public Guid OrdenVentaId { get; set; }
        public Guid FacturaId { get; set; }
        public decimal TotalFacturado { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
