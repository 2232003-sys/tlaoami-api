using System;
using Tlaoami.Domain.Enums;

namespace Tlaoami.API.Dtos
{
    public class PagoDto
    {
        public Guid Id { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string Metodo { get; set; }
    }
}
