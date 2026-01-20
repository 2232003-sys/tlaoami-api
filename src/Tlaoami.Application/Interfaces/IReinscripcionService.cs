using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IReinscripcionService
    {
        Task<ReinscripcionResultDto> ReinscribirAsync(ReinscripcionRequestDto dto);
    }
}
