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
        Task<EstadoCuentaDto?> GetEstadoCuentaAsync(Guid id);
        Task<AlumnoDto> CreateAlumnoAsync(AlumnoDto alumnoDto);
        Task UpdateAlumnoAsync(Guid id, AlumnoDto alumnoDto);
        Task DeleteAlumnoAsync(Guid id);
    }
}
