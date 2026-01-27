using System;

namespace Tlaoami.Application.Dtos
{
    public class AlumnoAsignacionCreateDto
    {
        public Guid ConceptoCobroId { get; set; }
        public Guid CicloId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal? MontoOverride { get; set; }
        public bool Activo { get; set; } = true;
    }
}
