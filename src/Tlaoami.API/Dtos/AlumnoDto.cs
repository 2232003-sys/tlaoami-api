using System;
using System.Collections.Generic;

namespace Tlaoami.API.Dtos
{
    public class AlumnoDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public ICollection<FacturaDto> Facturas { get; set; }
    }
}
