using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface ICicloEscolarService
    {
        Task<IEnumerable<CicloEscolarDto>> GetAllCiclosAsync();
        Task<CicloEscolarDto?> GetCicloByIdAsync(Guid id);
        Task<CicloEscolarDto?> GetCicloActivoAsync();
        Task<CicloEscolarDto> CreateCicloAsync(CicloEscolarCreateDto dto);
        Task<CicloEscolarDto> UpdateCicloAsync(Guid id, CicloEscolarCreateDto dto);
        Task<bool> DeleteCicloAsync(Guid id);
    }
}
