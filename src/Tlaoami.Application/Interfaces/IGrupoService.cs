using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IGrupoService
    {
        Task<IEnumerable<GrupoDto>> GetAllGruposAsync();
        Task<IEnumerable<GrupoDto>> GetGruposPorCicloAsync(Guid cicloId);
        Task<GrupoDto?> GetGrupoByIdAsync(Guid id);
        Task<GrupoDto> CreateGrupoAsync(GrupoCreateDto dto);
        Task<GrupoDto> UpdateGrupoAsync(Guid id, GrupoCreateDto dto);
        Task<bool> DeleteGrupoAsync(Guid id);
        Task<GrupoDto> AssignDocenteTitularAsync(Guid grupoId, Guid? docenteTitularId);
    }
}
