using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces;

public interface IPagoService
{
    Task<PagoDto> RegistrarPagoAsync(PagoCreateDto pagoCreateDto);
}
