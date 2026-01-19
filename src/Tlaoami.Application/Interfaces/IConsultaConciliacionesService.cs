using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces;

public interface IConsultaConciliacionesService
{
    Task<List<ConciliacionDetalleDto>> GetConciliacionesAsync(DateTime? desde = null, DateTime? hasta = null);
}
