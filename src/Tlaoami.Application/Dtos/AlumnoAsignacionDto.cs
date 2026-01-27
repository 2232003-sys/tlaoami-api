using System;

namespace Tlaoami.Application.Dtos
{
    public class AlumnoAsignacionDto
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public Guid ConceptoCobroId { get; set; }
        public Guid CicloId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal? MontoOverride { get; set; }
        public bool Activo { get; set; }
    }
}
