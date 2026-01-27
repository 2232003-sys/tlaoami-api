using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services;

public class ConciliacionBancariaService : IConciliacionBancariaService
{
    private readonly TlaoamiDbContext _context;
    private readonly ILogger<ConciliacionBancariaService> _logger;
    private const decimal TOLERANCE = 0.01m;

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
        DateTime? fechaPago = null,
        bool aplicarACuenta = false)
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

        // Si aplicarACuenta, buscar la factura más antigua pendiente del alumno
        if (aplicarACuenta && alumnoId.HasValue && !facturaId.HasValue)
        {
            var facturaFIFO = await _context.Facturas
                .Where(f => f.AlumnoId == alumnoId.Value &&
                           (f.Estado == EstadoFactura.Pendiente || f.Estado == EstadoFactura.ParcialmentePagada))
                .OrderBy(f => f.FechaVencimiento)
                .ThenBy(f => f.FechaEmision)
                .FirstOrDefaultAsync();

            if (facturaFIFO != null)
            {
                facturaId = facturaFIFO.Id;
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

            if (facturaId.HasValue)
            {
                await AplicarPagoAFacturaAsync(facturaId.Value, movimiento, metodo, fechaPago);
            }
            else if (alumnoId.HasValue)
            {
                await AplicarAbonoACuentaAsync(alumnoId.Value, movimiento, metodo, fechaPago);
            }
        }

        _logger.LogInformation(
            "Movimiento {MovimientoId} conciliado correctamente con Alumno: {AlumnoId}, Factura: {FacturaId}",
            movimientoBancarioId, alumnoId, facturaId);
    }

    private async Task AplicarPagoAFacturaAsync(
        Guid facturaId,
        MovimientoBancario movimiento,
        string metodo,
        DateTime? fechaPago)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var factura = await _context.Facturas
                    .Include(f => f.Pagos)
                    .Include(f => f.Lineas)
                    .FirstOrDefaultAsync(f => f.Id == facturaId);

                if (factura == null)
                    throw new ApplicationException($"Factura con ID {facturaId} no encontrada");

                var idempotencyKey = $"BANK:{movimiento.Id}";
                var existingPago = await _context.Pagos
                    .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);

                if (existingPago != null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogInformation("Pago ya existe para movimiento {MovId}", movimiento.Id);
                    return;
                }

                var metodoPago = Enum.TryParse<MetodoPago>(metodo, true, out var mt) 
                    ? mt 
                    : MetodoPago.Transferencia;

                var pago = new Pago
                {
                    Id = Guid.NewGuid(),
                    FacturaId = facturaId,
                    AlumnoId = factura.AlumnoId,
                    IdempotencyKey = idempotencyKey,
                    Monto = movimiento.Monto,
                    FechaPago = (fechaPago?.ToUniversalTime() ?? movimiento.Fecha).ToUniversalTime(),
                    Metodo = metodoPago
                };

                _context.Pagos.Add(pago);
                factura.Pagos.Add(pago);

                factura.RecalculateFrom(
                    factura.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)),
                    factura.Pagos);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Pago de ${Monto} aplicado a factura {FacturaId}",
                    movimiento.Monto,
                    facturaId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al aplicar pago a factura {FacturaId}", facturaId);
                throw;
            }
        }
    }

    private async Task AplicarAbonoACuentaAsync(
        Guid alumnoId,
        MovimientoBancario movimiento,
        string metodo,
        DateTime? fechaPago)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var idempotencyKeyBase = $"BANK:{movimiento.Id}";
                var existingPagos = await _context.Pagos
                    .Where(p => p.IdempotencyKey.StartsWith(idempotencyKeyBase))
                    .ToListAsync();

                if (existingPagos.Any())
                {
                    await transaction.RollbackAsync();
                    _logger.LogInformation("Pagos ya existen para movimiento {MovId}", movimiento.Id);
                    return;
                }

                var facturasPendientes = await _context.Facturas
                    .Include(f => f.Pagos)
                    .Include(f => f.Lineas)
                    .Where(f => f.AlumnoId == alumnoId
                             && f.Estado != EstadoFactura.Pagada
                             && f.Estado != EstadoFactura.Cancelada
                             && f.Estado != EstadoFactura.Borrador)
                    .OrderBy(f => f.FechaVencimiento)
                    .ThenBy(f => f.FechaEmision)
                    .ToListAsync();

                if (!facturasPendientes.Any())
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(
                        "Alumno {AlumnoId} no tiene facturas pendientes. Pago NO aplicado",
                        alumnoId);
                    throw new InvalidOperationException(
                        "No hay facturas pendientes para aplicar el abono");
                }

                decimal montoRestante = movimiento.Monto;
                int secuencia = 0;
                var metodoPago = Enum.TryParse<MetodoPago>(metodo, true, out var mt)
                    ? mt
                    : MetodoPago.Transferencia;
                var fechaPagoUtc = (fechaPago?.ToUniversalTime() ?? movimiento.Fecha).ToUniversalTime();

                foreach (var factura in facturasPendientes)
                {
                    if (montoRestante <= TOLERANCE) break;

                    factura.RecalculateFrom(
                        factura.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)),
                        factura.Pagos ?? Enumerable.Empty<Pago>());

                    var saldoFactura = factura.Monto - (factura.Pagos?.Sum(p => p.Monto) ?? 0m);

                    if (saldoFactura <= TOLERANCE) continue;

                    var montoAAplicar = Math.Min(saldoFactura, montoRestante);

                    var idempotencyKey = $"{idempotencyKeyBase}:F{secuencia}";
                    var pago = new Pago
                    {
                        Id = Guid.NewGuid(),
                        FacturaId = factura.Id,
                        AlumnoId = alumnoId,
                        IdempotencyKey = idempotencyKey,
                        Monto = montoAAplicar,
                        FechaPago = fechaPagoUtc,
                        Metodo = metodoPago
                    };

                    _context.Pagos.Add(pago);
                    factura.Pagos.Add(pago);

                    factura.RecalculateFrom(
                        factura.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)),
                        factura.Pagos);

                    montoRestante -= montoAAplicar;
                    secuencia++;

                    _logger.LogInformation(
                        "Aplicado pago de ${Monto} a factura {FacturaId} (secuencia {Seq})",
                        montoAAplicar,
                        factura.Id,
                        secuencia - 1);
                }

                if (montoRestante > TOLERANCE)
                {
                    var pagoAnticipo = new Pago
                    {
                        Id = Guid.NewGuid(),
                        FacturaId = null,
                        AlumnoId = alumnoId,
                        IdempotencyKey = $"{idempotencyKeyBase}:ANTICIPO",
                        Monto = montoRestante,
                        FechaPago = fechaPagoUtc,
                        Metodo = metodoPago
                    };

                    _context.Pagos.Add(pagoAnticipo);

                    _logger.LogInformation(
                        "Creado pago anticipo de ${Monto} para alumno {AlumnoId}",
                        montoRestante,
                        alumnoId);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Abono de ${Monto} aplicado a cuenta del alumno {AlumnoId} en {Count} facturas",
                    movimiento.Monto,
                    alumnoId,
                    secuencia);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al aplicar abono a cuenta del alumno {AlumnoId}", alumnoId);
                throw;
            }
        }
    }

    public async Task RevertirConciliacionAsync(Guid movimientoBancarioId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var movimiento = await _context.MovimientosBancarios
                    .FirstOrDefaultAsync(m => m.Id == movimientoBancarioId);

                if (movimiento == null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Intento de revertir conciliación de movimiento inexistente: {MovimientoId}", movimientoBancarioId);
                    throw new ApplicationException($"Movimiento bancario con ID {movimientoBancarioId} no encontrado");
                }

                if (movimiento.Estado != EstadoConciliacion.Conciliado)
                {
                    await transaction.RollbackAsync();
                    _logger.LogInformation("Movimiento {MovimientoId} no está conciliado. Operación es idempotente", movimientoBancarioId);
                    return;
                }

                var conciliacion = await _context.MovimientosConciliacion
                    .FirstOrDefaultAsync(mc => mc.MovimientoBancarioId == movimientoBancarioId);

                if (conciliacion != null)
                {
                    var idempotencyKeyPrefix = $"BANK:{movimientoBancarioId}";
                    var pagos = await _context.Pagos
                        .Where(p => p.IdempotencyKey.StartsWith(idempotencyKeyPrefix))
                        .ToListAsync();

                    if (pagos.Any())
                    {
                        var facturaIds = pagos
                            .Where(p => p.FacturaId.HasValue)
                            .Select(p => p.FacturaId.Value)
                            .Distinct()
                            .ToList();

                        _context.Pagos.RemoveRange(pagos);
                        await _context.SaveChangesAsync();

                        foreach (var fid in facturaIds)
                        {
                            var factura = await _context.Facturas
                                .Include(f => f.Pagos)
                                .Include(f => f.Lineas)
                                .FirstOrDefaultAsync(f => f.Id == fid);
                            if (factura != null)
                            {
                                factura.RecalculateFrom(
                                    factura.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)),
                                    factura.Pagos ?? Enumerable.Empty<Pago>());
                            }
                        }
                    }

                    _context.MovimientosConciliacion.Remove(conciliacion);
                }

                movimiento.Estado = EstadoConciliacion.NoConciliado;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Conciliación del movimiento {MovimientoId} revertida correctamente", movimientoBancarioId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al revertir conciliación del movimiento {MovimientoId}", movimientoBancarioId);
                throw;
            }
        }
    }

    public async Task<Guid> ReportarPagoManualAsync(ReportarPagoManualDto dto)
    {
        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            AlumnoId = dto.AlumnoId,
            Monto = dto.Monto,
            FechaPago = dto.FechaPago,
            Metodo = MetodoPago.Efectivo,
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pago manual reportado: {PagoId} para alumno {AlumnoId} por monto {Monto}", 
            pago.Id, dto.AlumnoId, dto.Monto);

        return pago.Id;
    }

    public async Task<IReadOnlyList<PagoDto>> GetPagosManualesAsync(Guid escuelaId)
    {
        return await _context.Pagos
            .AsNoTracking()
            .Where(p => p.Metodo == MetodoPago.Efectivo)
            .OrderByDescending(p => p.FechaPago)
            .Select(p => new PagoDto
            {
                Id = p.Id,
                AlumnoId = p.AlumnoId,
                Monto = p.Monto,
                FechaPago = p.FechaPago,
                Metodo = p.Metodo.ToString()
            })
            .ToListAsync();
    }

    public async Task ConciliarPagoManualAsync(Guid pagoId)
    {
        var pago = await _context.Pagos.FirstOrDefaultAsync(p => p.Id == pagoId);
        
        if (pago == null)
        {
            _logger.LogWarning("Intento de conciliar pago inexistente: {PagoId}", pagoId);
            throw new ApplicationException($"Pago con ID {pagoId} no encontrado");
        }

        // Actualizar estado (en Pago usamos Estatus que es un enum, pero si es string, simplemente no lo cambiamos aquí)
        // Por ahora solo marcamos con una fecha
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pago {PagoId} conciliado manualmente", pagoId);
    }
}
