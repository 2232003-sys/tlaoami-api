using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IAlumnoAsignacionesService
    {
        Task<AlumnoAsignacionDto> CreateAsignacionAsync(Guid alumnoId, AlumnoAsignacionCreateDto dto);
        Task<bool> CancelarAsignacionAsync(Guid asignacionId);
        Task<IReadOnlyList<AlumnoAsignacionDto>> ListarAsignacionesPorAlumnoAsync(Guid alumnoId);
    }
}
