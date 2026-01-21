using System;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities
{
    public class BecaAlumno
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public Alumno? Alumno { get; set; }
        public Guid CicloId { get; set; }
        public CicloEscolar? Ciclo { get; set; }
        public BecaTipo Tipo { get; set; }
        public decimal Valor { get; set; }
        public bool Activa { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
