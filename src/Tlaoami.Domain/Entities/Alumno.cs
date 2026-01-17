using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tlaoami.Domain.Entities
{
    public class Alumno
    {
        public Guid Id { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(100)]
        public string Apellido { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public ICollection<Factura> Facturas { get; set; }
    }
}
