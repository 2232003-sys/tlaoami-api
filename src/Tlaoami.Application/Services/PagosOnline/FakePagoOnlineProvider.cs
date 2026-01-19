using System.Security.Cryptography;
using Tlaoami.Application.Interfaces.PagosOnline;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Services.PagosOnline;

public class FakePagoOnlineProvider : IPagoOnlineProvider
{
    public string Nombre => "Fake";

    public Task<ProviderCrearIntentResultado> CrearIntentAsync(ProviderCrearIntentRequest request)
    {
        return request.Metodo switch
        {
            MetodoPagoIntent.Tarjeta => Task.FromResult(new ProviderCrearIntentResultado(
                $"FAKE-{request.PaymentIntentId.ToString("N")[..8]}",
                null,
                null,
                null
            )),
            MetodoPagoIntent.Spei => Task.FromResult(new ProviderCrearIntentResultado(
                null,
                GenerarReferenciaSpei(),
                "000000000000000000",
                null
            )),
            _ => throw new InvalidOperationException("Metodo de pago no soportado.")
        };
    }

    private static string GenerarReferenciaSpei()
    {
        Span<char> buffer = stackalloc char[20];
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return new string(buffer);
    }
}
