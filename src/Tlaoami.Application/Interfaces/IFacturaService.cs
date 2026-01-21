using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IFacturaService
    {
        Task<IEnumerable<FacturaDto>> GetAllFacturasAsync();
        Task<IEnumerable<FacturaDetalleDto>> GetFacturasConFiltrosAsync(
            Guid? alumnoId = null,
            string? estado = null,
            DateTime? desde = null,
            DateTime? hasta = null);
        Task<FacturaDto?> GetFacturaByIdAsync(Guid id);
        Task<FacturaDetalleDto?> GetFacturaDetalleByIdAsync(Guid id);
        Task<IEnumerable<FacturaDetalleDto>> GetAllFacturasDetalleAsync();
        Task<IEnumerable<FacturaDetalleDto>> GetFacturasByAlumnoIdAsync(Guid alumnoId);
        Task<FacturaDto> CreateFacturaAsync(CrearFacturaDto crearFacturaDto);
        Task UpdateFacturaAsync(Guid id, FacturaDto facturaDto);
        Task DeleteFacturaAsync(Guid id);

        // Acciones de negocio
        Task EmitirFacturaAsync(Guid id);
        Task CancelarFacturaAsync(Guid id, string? motivo = null);
        Task EmitirReciboAsync(Guid id);
    }
}
