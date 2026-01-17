using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Mappers;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class PagoService : IPagoService
    {
        private readonly TlaoamiDbContext _context;

        public PagoService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PagoDto>> GetAllPagosAsync()
        {
            var pagos = await _context.Pagos.ToListAsync();
            return pagos.Select(MappingFunctions.ToPagoDto);
        }

        public async Task<PagoDto?> GetPagoByIdAsync(Guid id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            return pago != null ? MappingFunctions.ToPagoDto(pago) : null;
        }

        public async Task<PagoDto> CreatePagoAsync(PagoDto pagoDto)
        {
            var pago = new Pago
            {
                FacturaId = pagoDto.FacturaId,
                Monto = pagoDto.Monto,
                FechaPago = pagoDto.FechaPago,
                Metodo = (MetodoPago)Enum.Parse(typeof(MetodoPago), pagoDto.Metodo!, true)
            };

            var factura = await _context.Facturas.FindAsync(pago.FacturaId);
            if (factura != null)
            {
                factura.Estado = EstadoFactura.Pagada;
            }

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            return MappingFunctions.ToPagoDto(pago);
        }

        public async Task UpdatePagoAsync(Guid id, PagoDto pagoDto)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                pago.Monto = pagoDto.Monto;
                pago.FechaPago = pagoDto.FechaPago;
                pago.Metodo = (MetodoPago)Enum.Parse(typeof(MetodoPago), pagoDto.Metodo!, true);

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePagoAsync(Guid id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                _context.Pagos.Remove(pago);
                await _context.SaveChangesAsync();
            }
        }
    }
}
