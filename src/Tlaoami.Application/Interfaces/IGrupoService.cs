using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IGrupoService
    {
        Task<IEnumerable<GrupoDto>> GetAllGruposAsync(bool incluirInactivos = false);
        Task<IEnumerable<GrupoDto>> GetGruposPorCicloAsync(Guid cicloId, bool incluirInactivos = false);
        Task<GrupoDto?> GetGrupoByIdAsync(Guid id);
        Task<GrupoDto> CreateGrupoAsync(GrupoCreateDto dto);
        Task<GrupoDto> UpdateGrupoAsync(Guid id, GrupoUpdateDto dto);
        Task<bool> DeleteGrupoAsync(Guid id); // Soft delete
        Task<GrupoDto> AssignDocenteTitularAsync(Guid grupoId, Guid? docenteTitularId);
        Task<IEnumerable<AlumnoEnGrupoDto>> GetAlumnosPorGrupoAsync(Guid grupoId);
    }
}
