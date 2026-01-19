using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces;

public interface ISugerenciasConciliacionService
{
    Task<List<SugerenciaConciliacionDto>> GetSugerenciasAsync(Guid movimientoBancarioId);
}
