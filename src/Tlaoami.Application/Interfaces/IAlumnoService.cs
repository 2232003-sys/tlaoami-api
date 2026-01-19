using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IAlumnoService
    {
        Task<IEnumerable<AlumnoDto>> GetAllAlumnosAsync();
        Task<AlumnoDto?> GetAlumnoByIdAsync(Guid id);
        Task<AlumnoDto?> GetAlumnoByMatriculaAsync(string matricula);
        Task<AlumnoDto?> GetAlumnoConGrupoActualAsync(Guid id);
        Task<EstadoCuentaDto?> GetEstadoCuentaAsync(Guid id);
        Task<AlumnoDto> CreateAlumnoAsync(AlumnoCreateDto dto);
        Task<AlumnoDto> UpdateAlumnoAsync(Guid id, AlumnoUpdateDto dto);
        Task<bool> DeleteAlumnoAsync(Guid id);
    }
}
