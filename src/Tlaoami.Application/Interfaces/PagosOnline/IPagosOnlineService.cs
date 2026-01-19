using Tlaoami.Application.Dtos.PagosOnline;

namespace Tlaoami.Application.Interfaces.PagosOnline;

public interface IPagosOnlineService
{
    Task<PaymentIntentDto> CrearAsync(CrearPaymentIntentDto dto);
    Task<PaymentIntentDto> GetByIdAsync(Guid id);
    Task<IEnumerable<PaymentIntentDto>> GetByFacturaIdAsync(Guid facturaId);
    Task<PaymentIntentDto> ConfirmarPagoAsync(Guid paymentIntentId, string usuario, string? comentario);
    Task<PaymentIntentDto> CancelarAsync(Guid paymentIntentId, string usuario, string? comentario);
    Task<PaymentIntentDto> ProcesarWebhookSimuladoAsync(Guid paymentIntentId, string estado, string? comentario);
}
