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

        if (factura.Estado == EstadoFactura.Pagada)
        {
            throw new InvalidOperationException("No se puede crear un PaymentIntent para una factura ya pagada.");
        }

        // Verificar intent activo duplicado (Creado o Pendiente)
        var intentActivo = await _context.PaymentIntents
            .FirstOrDefaultAsync(pi => pi.FacturaId == dto.FacturaId &&
                (pi.Estado == EstadoPagoIntent.Creado || pi.Estado == EstadoPagoIntent.Pendiente));

        if (intentActivo != null)
        {
            throw new InvalidOperationException("Ya existe un PaymentIntent activo para esta factura.");
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

    public async Task<IEnumerable<PaymentIntentDto>> GetByFacturaIdAsync(Guid facturaId)
    {
        var paymentIntents = await _context.PaymentIntents
            .AsNoTracking()
            .Where(pi => pi.FacturaId == facturaId)
            .OrderByDescending(pi => pi.CreadoEnUtc)
            .ToListAsync();

        return paymentIntents.Select(ToDto);
    }

    public async Task<PaymentIntentDto> ConfirmarPagoAsync(Guid paymentIntentId, string usuario, string? comentario)
    {
        if (string.IsNullOrWhiteSpace(usuario))
        {
            throw new InvalidOperationException("El usuario conciliador es requerido.");
        }

        var paymentIntent = await _context.PaymentIntents.FindAsync(paymentIntentId);

        if (paymentIntent == null)
        {
            throw new ApplicationException("PaymentIntent no encontrado.");
        }

        if (paymentIntent.Estado == EstadoPagoIntent.Pagado)
        {
            return ToDto(paymentIntent);
        }

        if (paymentIntent.Estado == EstadoPagoIntent.Cancelado)
        {
            throw new InvalidOperationException("No se puede confirmar un PaymentIntent cancelado.");
        }

        if (paymentIntent.Estado != EstadoPagoIntent.Pendiente)
        {
            throw new InvalidOperationException("Solo se puede confirmar un PaymentIntent pendiente.");
        }

        // Verificar expiración
        if (paymentIntent.ExpiraEnUtc.HasValue && paymentIntent.ExpiraEnUtc.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("El PaymentIntent ha expirado.");
        }

        // Verificar si ya existe un pago asociado (idempotencia)
        var pagoExistente = await _context.Pagos
            .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);

        if (pagoExistente != null)
        {
            return ToDto(paymentIntent);
        }

        // Crear Pago real
        var factura = await _context.Facturas
            .Include(f => f.Pagos)
            .FirstOrDefaultAsync(f => f.Id == paymentIntent.FacturaId);

        if (factura == null)
        {
            throw new ApplicationException("Factura no encontrada.");
        }

        var nuevoPago = new Pago
        {
            Id = Guid.NewGuid(),
            FacturaId = paymentIntent.FacturaId,
            Monto = paymentIntent.Monto,
            FechaPago = DateTime.UtcNow,
            Metodo = MapMetodoPago(paymentIntent.Metodo),
            PaymentIntentId = paymentIntentId
        };

        _context.Pagos.Add(nuevoPago);

        // Actualizar estado del PaymentIntent
        paymentIntent.Estado = EstadoPagoIntent.Pagado;
        paymentIntent.ActualizadoEnUtc = DateTime.UtcNow;
        // TODO: auditoria cuando exista tabla (usuario/comentario)

        // Actualizar estado de la Factura
        var totalPagado = factura.Pagos.Sum(p => p.Monto) + nuevoPago.Monto;

        if (totalPagado >= factura.Monto)
        {
            factura.Estado = EstadoFactura.Pagada;
        }
        else if (totalPagado > 0)
        {
            factura.Estado = EstadoFactura.ParcialmentePagada;
        }

        await _context.SaveChangesAsync();

        return ToDto(paymentIntent);
    }

    private static MetodoPago MapMetodoPago(MetodoPagoIntent metodoPagoIntent)
    {
        return metodoPagoIntent switch
        {
            MetodoPagoIntent.Tarjeta => MetodoPago.Tarjeta,
            MetodoPagoIntent.Spei => MetodoPago.Transferencia,
            _ => MetodoPago.Transferencia
        };
    }

    public async Task<PaymentIntentDto> CancelarAsync(Guid paymentIntentId, string usuario, string? comentario)
    {
        if (string.IsNullOrWhiteSpace(usuario))
        {
            throw new InvalidOperationException("El usuario es requerido.");
        }

        var paymentIntent = await _context.PaymentIntents.FindAsync(paymentIntentId);

        if (paymentIntent == null)
        {
            throw new ApplicationException("PaymentIntent no encontrado.");
        }

        if (paymentIntent.Estado == EstadoPagoIntent.Cancelado)
        {
            return ToDto(paymentIntent);
        }

        if (paymentIntent.Estado == EstadoPagoIntent.Pagado)
        {
            throw new InvalidOperationException("No se puede cancelar un PaymentIntent pagado.");
        }

        if (paymentIntent.Estado != EstadoPagoIntent.Pendiente && paymentIntent.Estado != EstadoPagoIntent.Creado)
        {
            throw new InvalidOperationException("Solo se puede cancelar un PaymentIntent pendiente/creado.");
        }

        paymentIntent.Estado = EstadoPagoIntent.Cancelado;
        paymentIntent.ActualizadoEnUtc = DateTime.UtcNow;
        // TODO: auditoria cuando exista tabla (usuario/comentario)

        await _context.SaveChangesAsync();

        return ToDto(paymentIntent);
    }

    public async Task<PaymentIntentDto> ProcesarWebhookSimuladoAsync(Guid paymentIntentId, string estado, string? comentario)
    {
        var paymentIntent = await _context.PaymentIntents.FindAsync(paymentIntentId);

        if (paymentIntent == null)
        {
            throw new ApplicationException("PaymentIntent no encontrado.");
        }

        var estadoLower = estado.ToLowerInvariant();

        if (estadoLower == "pagado")
        {
            if (paymentIntent.Estado == EstadoPagoIntent.Pagado)
            {
                return ToDto(paymentIntent);
            }

            if (paymentIntent.Estado != EstadoPagoIntent.Pendiente)
            {
                throw new InvalidOperationException("Solo se puede marcar como pagado un PaymentIntent pendiente.");
            }

            paymentIntent.Estado = EstadoPagoIntent.Pagado;
            paymentIntent.ActualizadoEnUtc = DateTime.UtcNow;
            // TODO: auditoria cuando exista tabla
        }
        else if (estadoLower == "cancelado")
        {
            if (paymentIntent.Estado == EstadoPagoIntent.Cancelado)
            {
                return ToDto(paymentIntent);
            }

            if (paymentIntent.Estado == EstadoPagoIntent.Pagado)
            {
                throw new InvalidOperationException("No se puede cancelar un PaymentIntent pagado.");
            }

            paymentIntent.Estado = EstadoPagoIntent.Cancelado;
            paymentIntent.ActualizadoEnUtc = DateTime.UtcNow;
            // TODO: auditoria cuando exista tabla
        }
        else
        {
            throw new InvalidOperationException("Estado no valido. Use 'pagado' o 'cancelado'.");
        }

        await _context.SaveChangesAsync();

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
