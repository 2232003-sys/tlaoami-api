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
        Task<FacturaDetalleDto?> GetFacturaDetalleByIdAsync(Guid id);
        Task<IEnumerable<FacturaDetalleDto>> GetAllFacturasDetalleAsync();
        Task<IEnumerable<FacturaDetalleDto>> GetFacturasByAlumnoIdAsync(Guid alumnoId);
        Task<FacturaDto> CreateFacturaAsync(FacturaDto facturaDto);
        Task UpdateFacturaAsync(Guid id, FacturaDto facturaDto);
        Task DeleteFacturaAsync(Guid id);
    }
}
