using System;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    /// <summary>
    /// Servicio de Reinscripción: gestiona reinscripción de alumnos a nuevos ciclos.
    /// Valida adeudo, desasigna grupo anterior, asigna nuevo grupo.
    /// </summary>
    public interface IReinscripcionService
    {
        /// <summary>
        /// Crea una solicitud de reinscripción.
        /// Valida adeudo, si lo hay => lanza BusinessException 409 REINSCRIPCION_BLOQUEADA_ADEUDO.
        /// Si pasa validaciones, desasigna grupo anterior y asigna nuevo grupo.
        /// </summary>
        Task<ReinscripcionDto> CrearReinscripcionAsync(ReinscripcionCreateDto dto, Guid? usuarioId = null);

        /// <summary>
        /// Obtiene detalles de una reinscripción por ID
        /// </summary>
        Task<ReinscripcionDto?> GetReinscripcionAsync(Guid reinscripcionId);

        /// <summary>
        /// Lista reinscripciones de un alumno (opcionalmente filtradas por ciclo)
        /// </summary>
        Task<IEnumerable<ReinscripcionDto>> GetReinscripcionesPorAlumnoAsync(Guid alumnoId, Guid? cicloDestinoId = null);
    }
}
