using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IPagoService
    {
        Task<IEnumerable<PagoDto>> GetAllPagosAsync();
        Task<PagoDto?> GetPagoByIdAsync(Guid id);
        Task<PagoDto> CreatePagoAsync(PagoDto pagoDto);
        Task UpdatePagoAsync(Guid id, PagoDto pagoDto);
        Task DeletePagoAsync(Guid id);
    }
}
