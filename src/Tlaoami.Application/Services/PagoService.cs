using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Tlaoami.Application.Services;

public class PagoService : IPagoService
{
    private readonly TlaoamiDbContext _context;

    public PagoService(TlaoamiDbContext context)
    {
        _context = context;
    }

    public async Task<PagoDto> RegistrarPagoAsync(PagoCreateDto pagoCreateDto)
    {
        var factura = await _context.Facturas.FindAsync(pagoCreateDto.FacturaId);
        if (factura == null)
        {
            throw new Exception("Factura no encontrada");
        }

        if (factura.Estado == "Pagada")
        {
            throw new Exception("La factura ya ha sido pagada.");
        }

        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            FacturaId = pagoCreateDto.FacturaId,
            Monto = pagoCreateDto.Monto,
            FechaPago = pagoCreateDto.FechaPago
        };

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        var totalPagado = await _context.Pagos
            .Where(p => p.FacturaId == pagoCreateDto.FacturaId)
            .SumAsync(p => p.Monto);

        if (totalPagado >= factura.Monto)
        {
            factura.Estado = "Pagada";
            await _context.SaveChangesAsync();
        }

        return new PagoDto
        {
            Id = pago.Id,
            FacturaId = pago.FacturaId,
            Monto = pago.Monto,
            FechaPago = pago.FechaPago
        };
    }
}
