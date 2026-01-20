using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces;

public interface IPagoService
{
    Task<(PagoDto pago, bool created)> RegistrarPagoAsync(PagoCreateDto pagoCreateDto);
    Task<IEnumerable<PagoDto>> GetPagosByFacturaIdAsync(Guid facturaId);
    Task<PagoDto?> GetPagoByIdAsync(Guid id);
}
