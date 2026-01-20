using System;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IAsignacionGrupoService
    {
        /// <summary>
        /// Asigna un alumno a un grupo.
        /// Cierra la asignación activa anterior si existe.
        /// Un alumno solo puede tener un grupo activo a la vez.
        /// </summary>
        Task<AlumnoGrupoDto> AsignarAlumnoAGrupoAsync(AsignarAlumnoGrupoDto dto);

        /// <summary>
        /// Obtiene el grupo actual (activo) de un alumno.
        /// </summary>
        Task<GrupoDto?> GetGrupoActualDeAlumnoAsync(Guid alumnoId);

        /// <summary>
        /// Desactiva la asignación actual de un alumno a su grupo.
        /// </summary>
        Task<bool> DesasignarAlumnoDeGrupoAsync(Guid alumnoId);

        /// <summary>
        /// Obtiene el historial de asignaciones de un alumno (lectura).
        /// </summary>
        Task<IEnumerable<AlumnoGrupoDto>> GetHistorialAsignacionesAlumnoAsync(Guid alumnoId);
    }
}
