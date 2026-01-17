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
    public class FacturaService : IFacturaService
    {
        private readonly TlaoamiDbContext _context;

        public FacturaService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FacturaDto>> GetAllFacturasAsync()
        {
            var facturas = await _context.Facturas.ToListAsync();
            return facturas.Select(MappingFunctions.ToFacturaDto);
        }

        public async Task<FacturaDto?> GetFacturaByIdAsync(Guid id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            return factura != null ? MappingFunctions.ToFacturaDto(factura) : null;
        }

        public async Task<FacturaDto> CreateFacturaAsync(FacturaDto facturaDto)
        {
            var factura = new Factura
            {
                AlumnoId = facturaDto.AlumnoId,
                NumeroFactura = facturaDto.NumeroFactura,
                Monto = facturaDto.Monto,
                FechaEmision = facturaDto.FechaEmision,
                FechaVencimiento = facturaDto.FechaVencimiento,
                Estado = (EstadoFactura)Enum.Parse(typeof(EstadoFactura), facturaDto.Estado!, true)
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            return MappingFunctions.ToFacturaDto(factura);
        }

        public async Task UpdateFacturaAsync(Guid id, FacturaDto facturaDto)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura != null)
            {
                factura.NumeroFactura = facturaDto.NumeroFactura;
                factura.Monto = facturaDto.Monto;
                factura.FechaEmision = facturaDto.FechaEmision;
                factura.FechaVencimiento = facturaDto.FechaVencimiento;
                factura.Estado = (EstadoFactura)Enum.Parse(typeof(EstadoFactura), facturaDto.Estado!, true);

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteFacturaAsync(Guid id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura != null)
            {
                _context.Facturas.Remove(factura);
                await _context.SaveChangesAsync();
            }
        }
    }
}
