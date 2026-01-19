using System;

namespace Tlaoami.Application.Dtos
{
    public class AsignarAlumnoGrupoDto
    {
        public Guid AlumnoId { get; set; }
        public Guid GrupoId { get; set; }
        public DateTime FechaInicio { get; set; }
    }

    public class AlumnoGrupoDto
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public Guid GrupoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Activo { get; set; }
    }
}
