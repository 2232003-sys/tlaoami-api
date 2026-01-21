using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services;

public class ConciliacionBancariaService : IConciliacionBancariaService
{
    private readonly TlaoamiDbContext _context;
    private readonly ILogger<ConciliacionBancariaService> _logger;

    public ConciliacionBancariaService(
        TlaoamiDbContext context,
        ILogger<ConciliacionBancariaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ConciliarMovimientoAsync(
        Guid movimientoBancarioId,
        Guid? alumnoId,
        Guid? facturaId,
        string? comentario,
        bool crearPago = false,
        string metodo = "Transferencia",
        DateTime? fechaPago = null)
    {
        var movimiento = await _context.MovimientosBancarios
            .FirstOrDefaultAsync(m => m.Id == movimientoBancarioId);

        if (movimiento == null)
        {
            _logger.LogWarning("Intento de conciliar movimiento inexistente: {MovimientoId}", movimientoBancarioId);
            throw new ApplicationException($"Movimiento bancario con ID {movimientoBancarioId} no encontrado");
        }

        if (movimiento.Estado == EstadoConciliacion.Conciliado)
        {
            _logger.LogInformation("Movimiento {MovimientoId} ya está conciliado. Operación es idempotente", movimientoBancarioId);
            return;
        }

        if (movimiento.Estado == EstadoConciliacion.Ignorado)
        {
            _logger.LogWarning("Intento de conciliar movimiento ignorado: {MovimientoId}", movimientoBancarioId);
            throw new InvalidOperationException("No se puede conciliar un movimiento marcado como ignorado");
        }

        if (alumnoId.HasValue)
        {
            var alumnoExiste = await _context.Alumnos.AnyAsync(a => a.Id == alumnoId.Value);
            if (!alumnoExiste)
            {
                _logger.LogWarning("Intento de conciliar con alumno inexistente: {AlumnoId}", alumnoId.Value);
                throw new ApplicationException($"Alumno con ID {alumnoId.Value} no encontrado");
            }
        }

        Factura? factura = null;
        if (facturaId.HasValue)
        {
            factura = await _context.Facturas.FirstOrDefaultAsync(f => f.Id == facturaId.Value);
            if (factura == null)
            {
                _logger.LogWarning("Intento de conciliar con factura inexistente: {FacturaId}", facturaId.Value);
                throw new ApplicationException($"Factura con ID {facturaId.Value} no encontrada");
            }

            // Validar que la factura no esté completamente pagada
            if (factura.Estado == EstadoFactura.Pagada)
            {
                _logger.LogWarning("Intento de conciliar movimiento con factura ya pagada: {FacturaId}", facturaId.Value);
                throw new InvalidOperationException("No se puede conciliar un movimiento con una factura ya pagada");
            }
        }

        var conciliacion = new MovimientoConciliacion
        {
            Id = Guid.NewGuid(),
            MovimientoBancarioId = movimientoBancarioId,
            AlumnoId = alumnoId,
            FacturaId = facturaId,
            FechaConciliacion = DateTime.UtcNow,
            Comentario = comentario
        };

        movimiento.Estado = EstadoConciliacion.Conciliado;

        _context.MovimientosConciliacion.Add(conciliacion);
        await _context.SaveChangesAsync();

        // Si se solicita, registrar pago (con o sin factura)
        if (crearPago)
        {
            if (movimiento.Tipo != TipoMovimiento.Deposito)
            {
                throw new InvalidOperationException("Solo se pueden registrar pagos desde movimientos de tipo Depósito");
            }

            if (factura == null && !alumnoId.HasValue)
            {
                throw new InvalidOperationException("Se requiere alumnoId para registrar un pago sin factura");
            }

            var idempotencyKey = $"BANK:{movimientoBancarioId}";
            var existingPago = await _context.Pagos.AsNoTracking().FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);
            if (existingPago == null)
            {
                var metodoPago = Enum.TryParse<MetodoPago>(metodo, true, out var mt) ? mt : MetodoPago.Transferencia;
                var pago = new Pago
                {
                    Id = Guid.NewGuid(),
                    FacturaId = factura?.Id,
                    AlumnoId = alumnoId ?? factura?.AlumnoId,
                    IdempotencyKey = idempotencyKey,
                    Monto = movimiento.Monto,
                    FechaPago = fechaPago?.ToUniversalTime() ?? movimiento.Fecha,
                    Metodo = metodoPago,
                    PaymentIntentId = movimiento.Id // correlacionar para revertir
                };

                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();
            }

            // Recalcular factura solo si hay factura involucrada
            if (factura != null)
            {
                var totalPagado = (await _context.Pagos
                    .Where(p => p.FacturaId == factura.Id)
                    .Select(p => p.Monto)
                    .ToListAsync()).Sum();

                var saldo = factura.Monto - totalPagado;
                var hoy = DateTime.UtcNow.Date;

                if (saldo <= 0)
                    factura.Estado = EstadoFactura.Pagada;
                else if (hoy > factura.FechaVencimiento.Date)
                    factura.Estado = EstadoFactura.Vencida;
                else if (totalPagado > 0)
                    factura.Estado = EstadoFactura.ParcialmentePagada;
                else
                    factura.Estado = EstadoFactura.Pendiente;

                await _context.SaveChangesAsync();
            }
        }

        _logger.LogInformation(
            "Movimiento {MovimientoId} conciliado correctamente con Alumno: {AlumnoId}, Factura: {FacturaId}",
            movimientoBancarioId, alumnoId, facturaId);
    }

    public async Task RevertirConciliacionAsync(Guid movimientoBancarioId)
    {
        var movimiento = await _context.MovimientosBancarios
            .FirstOrDefaultAsync(m => m.Id == movimientoBancarioId);

        if (movimiento == null)
        {
            _logger.LogWarning("Intento de revertir conciliación de movimiento inexistente: {MovimientoId}", movimientoBancarioId);
            throw new ApplicationException($"Movimiento bancario con ID {movimientoBancarioId} no encontrado");
        }

        if (movimiento.Estado != EstadoConciliacion.Conciliado)
        {
            _logger.LogInformation("Movimiento {MovimientoId} no está conciliado. Operación es idempotente", movimientoBancarioId);
            return;
        }

        var conciliacion = await _context.MovimientosConciliacion
            .FirstOrDefaultAsync(mc => mc.MovimientoBancarioId == movimientoBancarioId);

        if (conciliacion != null)
        {
            // Revertir pago(s) asociados por PaymentIntentId (correlación)
            var pagos = await _context.Pagos
                .Where(p => p.PaymentIntentId == movimientoBancarioId)
                .ToListAsync();

            if (pagos.Any())
            {
                var facturaIds = pagos.Select(p => p.FacturaId).Distinct().ToList();
                _context.Pagos.RemoveRange(pagos);
                await _context.SaveChangesAsync();

                foreach (var fid in facturaIds)
                {
                    var factura = await _context.Facturas
                        .Include(f => f.Pagos)
                        .FirstOrDefaultAsync(f => f.Id == fid);
                    if (factura != null)
                    {
                        var totalPagado = factura.Pagos?.Sum(p => p.Monto) ?? 0m;
                        var saldo = factura.Monto - totalPagado;
                        var hoy = DateTime.UtcNow.Date;

                        if (saldo <= 0)
                            factura.Estado = EstadoFactura.Pagada;
                        else if (hoy > factura.FechaVencimiento.Date)
                            factura.Estado = EstadoFactura.Vencida;
                        else if (totalPagado > 0)
                            factura.Estado = EstadoFactura.ParcialmentePagada;
                        else
                            factura.Estado = EstadoFactura.Pendiente;
                    }
                }
            }

            _context.MovimientosConciliacion.Remove(conciliacion);
        }

        movimiento.Estado = EstadoConciliacion.NoConciliado;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Conciliación del movimiento {MovimientoId} revertida correctamente", movimientoBancarioId);
    }
}
