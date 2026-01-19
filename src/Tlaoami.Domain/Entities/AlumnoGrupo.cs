using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Histórico de asignaciones de alumnos a grupos.
    /// Un alumno puede tener solo un grupo activo a la vez.
    /// </summary>
    public class AlumnoGrupo
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public Alumno? Alumno { get; set; }

        public Guid GrupoId { get; set; }
        public Grupo? Grupo { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }  // null = activo

        /// <summary>
        /// Si FechaFin es null, es la asignación activa.
        /// </summary>
        public bool Activo { get; set; } = true;
    }
}
