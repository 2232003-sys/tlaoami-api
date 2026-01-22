using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tlaoami.Domain.Entities
{
    public class Alumno
    {
        public Guid Id { get; set; }

        [Required]
        public string? Matricula { get; set; }  // Unique identifier

        [Required]
        public string? Nombre { get; set; }

        [Required]
        public string? Apellido { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Telefono { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaInscripcion { get; set; }

        public ReceptorFiscal? ReceptorFiscal { get; set; }

        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();

        public ICollection<AlumnoGrupo> AsignacionesGrupo { get; set; } = new List<AlumnoGrupo>();
    }
}
