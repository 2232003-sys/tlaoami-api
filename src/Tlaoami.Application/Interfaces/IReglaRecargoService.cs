using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IReglaRecargoService
    {
        Task<List<ReglaRecargoDto>> GetAllAsync(Guid? cicloId = null, bool? activa = null);
        Task<ReglaRecargoDto> GetByIdAsync(Guid id);
        Task<ReglaRecargoDto> CreateAsync(ReglaRecargoCreateDto dto);
        Task<ReglaRecargoDto> UpdateAsync(Guid id, ReglaRecargoUpdateDto dto);
        Task InactivateAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
