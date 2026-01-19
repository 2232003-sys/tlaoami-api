using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos.PagosOnline;
using Tlaoami.Application.Interfaces.PagosOnline;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services.PagosOnline;

public class PagosOnlineService : IPagosOnlineService
{
    private readonly TlaoamiDbContext _context;
    private readonly IPagoOnlineProvider _provider;

    public PagosOnlineService(TlaoamiDbContext context, IPagoOnlineProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public async Task<PaymentIntentDto> CrearAsync(CrearPaymentIntentDto dto)
    {
        var factura = await _context.Facturas.FindAsync(dto.FacturaId);
        if (factura == null)
        {
            throw new ApplicationException("Factura no encontrada.");
        }

        var now = DateTime.UtcNow;
        var paymentIntent = new PaymentIntent
        {
            Id = Guid.NewGuid(),
            FacturaId = dto.FacturaId,
            Monto = factura.Monto,
            Metodo = dto.Metodo,
            Estado = EstadoPagoIntent.Creado,
            CreadoEnUtc = now,
            ActualizadoEnUtc = now
        };

        var providerResult = await _provider.CrearIntentAsync(new ProviderCrearIntentRequest(
            paymentIntent.Id,
            paymentIntent.FacturaId,
            paymentIntent.Monto,
            paymentIntent.Metodo
        ));

        paymentIntent.Proveedor = _provider.Nombre;
        paymentIntent.ProveedorReferencia = providerResult.ProveedorReferencia;
        paymentIntent.ReferenciaSpei = providerResult.ReferenciaSpei;
        paymentIntent.ClabeDestino = providerResult.ClabeDestino;
        paymentIntent.ExpiraEnUtc = providerResult.ExpiraEnUtc;
        paymentIntent.Estado = EstadoPagoIntent.Pendiente;
        paymentIntent.ActualizadoEnUtc = DateTime.UtcNow;

        // TODO: cuando haya webhook, actualizar PaymentIntent a Pagado y entonces crear Pago real + actualizar Factura.

        _context.PaymentIntents.Add(paymentIntent);
        await _context.SaveChangesAsync();

        return ToDto(paymentIntent);
    }

    public async Task<PaymentIntentDto> GetByIdAsync(Guid id)
    {
        var paymentIntent = await _context.PaymentIntents
            .AsNoTracking()
            .FirstOrDefaultAsync(pi => pi.Id == id);

        if (paymentIntent == null)
        {
            throw new ApplicationException("PaymentIntent no encontrado.");
        }

        return ToDto(paymentIntent);
    }

    private static PaymentIntentDto ToDto(PaymentIntent paymentIntent)
    {
        return new PaymentIntentDto
        {
            Id = paymentIntent.Id,
            FacturaId = paymentIntent.FacturaId,
            Monto = paymentIntent.Monto,
            Metodo = paymentIntent.Metodo.ToString(),
            Estado = paymentIntent.Estado.ToString(),
            Proveedor = paymentIntent.Proveedor,
            ProveedorReferencia = paymentIntent.ProveedorReferencia,
            ReferenciaSpei = paymentIntent.ReferenciaSpei,
            ClabeDestino = paymentIntent.ClabeDestino,
            ExpiraEnUtc = paymentIntent.ExpiraEnUtc,
            CreadoEnUtc = paymentIntent.CreadoEnUtc
        };
    }
}
