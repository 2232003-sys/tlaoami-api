using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IColegiaturasService
    {
        Task<ColegiaturaGeneracionResultDto> GenerarMensualAsync(ColegiaturaGeneracionRequestDto request);
        Task<RecargoAplicacionResultDto> AplicarRecargosAsync(RecargoAplicacionRequestDto request);
    }
}
