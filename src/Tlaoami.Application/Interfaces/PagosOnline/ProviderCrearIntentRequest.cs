using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Interfaces.PagosOnline;

public record ProviderCrearIntentRequest(Guid PaymentIntentId, Guid FacturaId, decimal Monto, MetodoPagoIntent Metodo);
