using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Mappers;
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

        if (factura.Estado == EstadoFactura.Pagada)
        {
            throw new Exception("La factura ya ha sido pagada.");
        }

        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            FacturaId = pagoCreateDto.FacturaId,
            Monto = pagoCreateDto.Monto,
            FechaPago = pagoCreateDto.FechaPago,
            Metodo = (MetodoPago)Enum.Parse(typeof(MetodoPago), pagoCreateDto.Metodo, true)
        };

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        var totalPagado = await _context.Pagos
            .Where(p => p.FacturaId == pagoCreateDto.FacturaId)
            .SumAsync(p => p.Monto);

        if (totalPagado >= factura.Monto)
        {
            factura.Estado = EstadoFactura.Pagada;
            await _context.SaveChangesAsync();
        }

        return MappingFunctions.ToPagoDto(pago);
    }

    public async Task<IEnumerable<PagoDto>> GetPagosByFacturaIdAsync(Guid facturaId)
    {
        var pagos = await _context.Pagos
            .Where(p => p.FacturaId == facturaId)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

        return pagos.Select(MappingFunctions.ToPagoDto);
    }

    public async Task<PagoDto?> GetPagoByIdAsync(Guid id)
    {
        var pago = await _context.Pagos.FindAsync(id);
        
        if (pago == null)
        {
            return null;
        }

        return MappingFunctions.ToPagoDto(pago);
    }
}
