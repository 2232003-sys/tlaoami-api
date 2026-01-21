using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IBecaAlumnoService
    {
        Task<List<BecaAlumnoDto>> GetAllAsync(Guid? cicloId = null, Guid? alumnoId = null, bool? activa = null);
        Task<BecaAlumnoDto> GetByIdAsync(Guid id);
        Task<BecaAlumnoDto> CreateAsync(BecaAlumnoCreateDto dto);
        Task<BecaAlumnoDto> UpdateAsync(Guid id, BecaAlumnoUpdateDto dto);
        Task InactivateAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
