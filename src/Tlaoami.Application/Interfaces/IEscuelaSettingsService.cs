using System.Threading.Tasks;
using Tlaoami.Application.Settings;

namespace Tlaoami.Application.Interfaces
{
    public interface IEscuelaSettingsService
    {
        Task<EscuelaSettingsDto?> GetSettingsAsync();
        Task<EscuelaSettingsDto> UpdateSettingsAsync(EscuelaSettingsDto dto);
    }
}
