
using System;
using System.Collections.Generic;

namespace Tlaoami.Application.Dtos
{
    public class EstadoCuentaDto
    {
        public Guid AlumnoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public decimal TotalFacturado { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public List<FacturaDto> FacturasPagadas { get; set; } = new List<FacturaDto>();
        public List<FacturaDto> FacturasPendientes { get; set; } = new List<FacturaDto>();
    }
}
