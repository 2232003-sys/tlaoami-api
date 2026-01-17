using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IFacturaService
    {
        Task<IEnumerable<FacturaDto>> GetAllFacturasAsync();
        Task<FacturaDto?> GetFacturaByIdAsync(Guid id);
        Task<FacturaDto> CreateFacturaAsync(FacturaDto facturaDto);
        Task UpdateFacturaAsync(Guid id, FacturaDto facturaDto);
        Task DeleteFacturaAsync(Guid id);
    }
}
