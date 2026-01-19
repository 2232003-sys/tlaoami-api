using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Interfaces.PagosOnline;

public interface IPagoOnlineProvider
{
    string Nombre { get; }

    Task<ProviderCrearIntentResultado> CrearIntentAsync(ProviderCrearIntentRequest request);
}
