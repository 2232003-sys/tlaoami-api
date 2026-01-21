using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IReglaColegiaturaService
    {
        Task<List<ReglaColegiaturaDto>> GetAllAsync(Guid? cicloId = null, Guid? grupoId = null, int? grado = null, bool? activa = null);
        Task<ReglaColegiaturaDto> GetByIdAsync(Guid id);
        Task<ReglaColegiaturaDto> CreateAsync(ReglaColegiaturaCreateDto dto);
        Task<ReglaColegiaturaDto> UpdateAsync(Guid id, ReglaColegiaturaUpdateDto dto);
        Task InactivateAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
