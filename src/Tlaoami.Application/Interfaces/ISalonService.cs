using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface ISalonService
    {
        Task<SalonDto> CreateAsync(SalonCreateDto dto);
        Task<SalonDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<SalonDto>> GetAllAsync(bool? activo = null);
        Task<SalonDto> UpdateAsync(Guid id, SalonUpdateDto dto);
        Task DeleteAsync(Guid id);
    }
}
