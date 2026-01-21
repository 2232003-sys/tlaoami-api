using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Mappers;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enums;
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
            var facturas = await _context.Facturas
                .Include(f => f.Pagos)
                .Include(f => f.Lineas)
                .ToListAsync();

            var changed = false;
            foreach (var f in facturas)
            {
                var prev = f.Estado;
                f.RecalculateFrom(f.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)), f.Pagos);
                if (prev != f.Estado) changed = true;
            }
            if (changed) await _context.SaveChangesAsync();
            return facturas.Select(MappingFunctions.ToFacturaDto);
        }

        public async Task<IEnumerable<FacturaDetalleDto>> GetFacturasConFiltrosAsync(
            Guid? alumnoId = null,
            string? estado = null,
            DateTime? desde = null,
            DateTime? hasta = null)
        {
            var query = _context.Facturas
                .Include(f => f.Alumno)
                .Include(f => f.Lineas)
                .Include(f => f.Pagos)
                .AsQueryable();

            if (alumnoId.HasValue)
                query = query.Where(f => f.AlumnoId == alumnoId.Value);

            if (!string.IsNullOrEmpty(estado))
            {
                if (Enum.TryParse<EstadoFactura>(estado, true, out var estadoEnum))
                    query = query.Where(f => f.Estado == estadoEnum);
            }

            if (desde.HasValue)
                query = query.Where(f => f.FechaEmision >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(f => f.FechaEmision <= hasta.Value);

            var facturas = await query.OrderByDescending(f => f.FechaEmision).ToListAsync();

            var changed = false;
            foreach (var f in facturas)
            {
                var prev = f.Estado;
                f.RecalculateFrom(f.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)), f.Pagos);
                if (prev != f.Estado) changed = true;
            }
            if (changed) await _context.SaveChangesAsync();

            return facturas.Select(MappingFunctions.ToFacturaDetalleDto);
        }

        public async Task<FacturaDto?> GetFacturaByIdAsync(Guid id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Lineas)
                .Include(f => f.Pagos)
                .FirstOrDefaultAsync(f => f.Id == id);
            return factura != null ? MappingFunctions.ToFacturaDto(factura) : null;
        }

        public async Task<FacturaDetalleDto?> GetFacturaDetalleByIdAsync(Guid id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Alumno)
                .Include(f => f.Lineas)
                .Include(f => f.Pagos)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            return factura != null ? MappingFunctions.ToFacturaDetalleDto(factura) : null;
        }

        public async Task<IEnumerable<FacturaDetalleDto>> GetAllFacturasDetalleAsync()
        {
            var facturas = await _context.Facturas
                .Include(f => f.Alumno)
                .Include(f => f.Lineas)
                .Include(f => f.Pagos)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();

            var changed = false;
            foreach (var f in facturas)
            {
                var prev = f.Estado;
                f.RecalculateFrom(f.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)), f.Pagos);
                if (prev != f.Estado) changed = true;
            }
            if (changed) await _context.SaveChangesAsync();

            return facturas.Select(MappingFunctions.ToFacturaDetalleDto);
        }

        public async Task<IEnumerable<FacturaDetalleDto>> GetFacturasByAlumnoIdAsync(Guid alumnoId)
        {
            var facturas = await _context.Facturas
                .Include(f => f.Alumno)
                .Include(f => f.Lineas)
                .Include(f => f.Pagos)
                .Where(f => f.AlumnoId == alumnoId)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();

            var changed = false;
            foreach (var f in facturas)
            {
                var prev = f.Estado;
                f.RecalculateFrom(f.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)), f.Pagos);
                if (prev != f.Estado) changed = true;
            }
            if (changed) await _context.SaveChangesAsync();

            return facturas.Select(MappingFunctions.ToFacturaDetalleDto);
        }

        public async Task<FacturaDto> CreateFacturaAsync(CrearFacturaDto crearFacturaDto)
        {
            // Generar número de factura automático
            var ultimaFactura = await _context.Facturas
                .OrderByDescending(f => f.NumeroFactura)
                .FirstOrDefaultAsync();
            
            var siguienteNumero = 1;
            if (ultimaFactura?.NumeroFactura != null && ultimaFactura.NumeroFactura.StartsWith("FAC-"))
            {
                var numeroParte = ultimaFactura.NumeroFactura.Substring(4);
                if (int.TryParse(numeroParte, out var numero))
                    siguienteNumero = numero + 1;
            }

            var numeroFactura = $"FAC-{siguienteNumero:D6}";

            // Si no se especifica fecha vencimiento, por defecto 30 días después
            var fechaEmision = crearFacturaDto.FechaEmision.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(crearFacturaDto.FechaEmision, DateTimeKind.Utc) 
                : crearFacturaDto.FechaEmision.ToUniversalTime();
            
            var fechaVencimiento = crearFacturaDto.FechaVencimiento.HasValue
                ? (crearFacturaDto.FechaVencimiento.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(crearFacturaDto.FechaVencimiento.Value, DateTimeKind.Utc)
                    : crearFacturaDto.FechaVencimiento.Value.ToUniversalTime())
                : fechaEmision.AddDays(30);

            var factura = new Factura
            {
                AlumnoId = crearFacturaDto.AlumnoId,
                NumeroFactura = numeroFactura,
                Concepto = crearFacturaDto.Concepto,
                Periodo = crearFacturaDto.Periodo?.Trim(),
                ConceptoCobroId = crearFacturaDto.ConceptoCobroId,
                TipoDocumento = TipoDocumento.Factura,
                Monto = crearFacturaDto.Monto,
                FechaEmision = fechaEmision,
                FechaVencimiento = fechaVencimiento,
                Estado = EstadoFactura.Borrador,
                Pagos = new List<Pago>(),
                Lineas = new List<FacturaLinea>()
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
                factura.Concepto = facturaDto.Concepto;
                factura.Periodo = facturaDto.Periodo;
                factura.ConceptoCobroId = facturaDto.ConceptoCobroId;
                if (!string.IsNullOrWhiteSpace(facturaDto.TipoDocumento) && Enum.TryParse<TipoDocumento>(facturaDto.TipoDocumento, true, out var tipo))
                {
                    factura.TipoDocumento = tipo;
                }
                factura.Monto = facturaDto.Monto;
                factura.FechaEmision = facturaDto.FechaEmision;
                factura.FechaVencimiento = facturaDto.FechaVencimiento;
                factura.Estado = (EstadoFactura)Enum.Parse(typeof(EstadoFactura), facturaDto.Estado!, true);
                factura.ReciboFolio ??= facturaDto.ReciboFolio;
                factura.ReciboEmitidoAtUtc ??= facturaDto.ReciboEmitidoAtUtc;

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
        // Replaced by Factura.RecalculateFrom

        public async Task EmitirFacturaAsync(Guid id)
        {
            var factura = await _context.Facturas.FirstOrDefaultAsync(f => f.Id == id);
            if (factura == null)
                throw new ApplicationException("Factura no encontrada");

            if (factura.Estado == EstadoFactura.Cancelada)
                throw new InvalidOperationException("No se puede emitir una factura cancelada");

            if (factura.TipoDocumento == TipoDocumento.Recibo)
            {
                // Ya emitido como recibo; idempotente
                return;
            }

            if (factura.Estado == EstadoFactura.Borrador)
            {
                factura.Estado = EstadoFactura.Pendiente; // Emitida ≈ Pendiente en el modelo actual
                factura.IssuedAt ??= DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return;
            }

            // Idempotente: ya emitida o con pagos -> no-op
        }

        public async Task CancelarFacturaAsync(Guid id, string? motivo = null)
        {
            var factura = await _context.Facturas
                .Include(f => f.Pagos)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (factura == null)
                throw new ApplicationException("Factura no encontrada");

            if (factura.Estado == EstadoFactura.Cancelada)
            {
                // Idempotente
                return;
            }

            if (factura.Estado == EstadoFactura.Pagada)
                throw new InvalidOperationException("No se puede cancelar una factura pagada");

            // Permitir cancelar desde Borrador, Pendiente, ParcialmentePagada, Vencida
            factura.Estado = EstadoFactura.Cancelada;
            factura.CanceledAt ??= DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(motivo))
                factura.CancelReason = motivo;

            await _context.SaveChangesAsync();
        }

        public async Task EmitirReciboAsync(Guid id)
        {
            var factura = await _context.Facturas.FirstOrDefaultAsync(f => f.Id == id);
            if (factura == null)
                throw new ApplicationException("Factura no encontrada");

            if (factura.Estado == EstadoFactura.Cancelada)
                throw new InvalidOperationException("No se puede emitir un recibo de una factura cancelada");

            if (factura.TipoDocumento == TipoDocumento.Recibo && !string.IsNullOrWhiteSpace(factura.ReciboFolio))
            {
                // Idempotente
                return;
            }

            if (factura.Estado != EstadoFactura.Borrador && factura.Estado != EstadoFactura.Pendiente)
                throw new InvalidOperationException("Solo se puede emitir recibo para facturas en Borrador o Pendiente.");

            factura.TipoDocumento = TipoDocumento.Recibo;
            factura.ReciboEmitidoAtUtc ??= DateTime.UtcNow;
            factura.ReciboFolio ??= await GenerarReciboFolioAsync();
            if (factura.Estado == EstadoFactura.Borrador)
            {
                factura.Estado = EstadoFactura.Pendiente;
                factura.IssuedAt ??= DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<string> GenerarReciboFolioAsync()
        {
            var ultimo = await _context.Facturas
                .Where(f => f.ReciboFolio != null && f.ReciboFolio.StartsWith("REC-"))
                .OrderByDescending(f => f.ReciboFolio)
                .Select(f => f.ReciboFolio)
                .FirstOrDefaultAsync();

            var siguiente = 1;
            if (!string.IsNullOrWhiteSpace(ultimo))
            {
                var parte = ultimo!.Substring(4);
                if (int.TryParse(parte, out var numero))
                {
                    siguiente = numero + 1;
                }
            }

            return $"REC-{siguiente:D6}";
        }
    }
}
