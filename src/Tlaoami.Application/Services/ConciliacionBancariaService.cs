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

    public async Task ConciliarMovimientoAsync(Guid movimientoBancarioId, Guid? alumnoId, Guid? facturaId, string? comentario)
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

        if (facturaId.HasValue)
        {
            var facturaExiste = await _context.Facturas.AnyAsync(f => f.Id == facturaId.Value);
            if (!facturaExiste)
            {
                _logger.LogWarning("Intento de conciliar con factura inexistente: {FacturaId}", facturaId.Value);
                throw new ApplicationException($"Factura con ID {facturaId.Value} no encontrada");
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
            _context.MovimientosConciliacion.Remove(conciliacion);
        }

        movimiento.Estado = EstadoConciliacion.NoConciliado;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Conciliación del movimiento {MovimientoId} revertida correctamente", movimientoBancarioId);
    }
}
