using Tlaoami.Application.Dtos.PagosOnline;

namespace Tlaoami.Application.Interfaces.PagosOnline;

public interface IPagosOnlineService
{
    Task<PaymentIntentDto> CrearAsync(CrearPaymentIntentDto dto);
    Task<PaymentIntentDto> GetByIdAsync(Guid id);
}
