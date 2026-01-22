using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Mappers;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services;

public class ConsultaConciliacionesService : IConsultaConciliacionesService
{
    private readonly TlaoamiDbContext _context;

    public ConsultaConciliacionesService(TlaoamiDbContext context)
    {
        _context = context;
    }

    public async Task<List<ConciliacionDetalleDto>> GetConciliacionesAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.MovimientosConciliacion
            .AsNoTracking()
            .Where(mc => mc.MovimientoBancario != null);

        // Filtrar por rango de fechas si se proporcionan
        if (desde.HasValue)
        {
            query = query.Where(mc => mc.FechaConciliacion >= desde.Value);
        }

        if (hasta.HasValue)
        {
            // Si hasta tiene hora 00:00:00, incluir todo ese día; si no, incluir hasta esa hora exacta
            if (hasta.Value.TimeOfDay == TimeSpan.Zero)
            {
                query = query.Where(mc => mc.FechaConciliacion < hasta.Value.Date.AddDays(1));
            }
            else
            {
                query = query.Where(mc => mc.FechaConciliacion <= hasta.Value);
            }
        }

        var conciliaciones = await query
            .OrderByDescending(mc => mc.FechaConciliacion)
            .Select(mc => new ConciliacionDetalleDto
            {
                ConciliacionId = mc.Id,
                FechaConciliacion = mc.FechaConciliacion,
                Comentario = mc.Comentario ?? string.Empty,
                MovimientoBancario = new MovimientoBancarioSimpleDto
                {
                    Id = mc.MovimientoBancario!.Id,
                    Fecha = mc.MovimientoBancario.Fecha,
                    Descripcion = mc.MovimientoBancario.Descripcion,
                    Monto = mc.MovimientoBancario.Monto,
                    Saldo = mc.MovimientoBancario.Saldo,
                    Tipo = mc.MovimientoBancario.Tipo.ToString(),
                    Estado = mc.MovimientoBancario.Estado.ToString()
                },
                Alumno = mc.Alumno != null ? new AlumnoSimpleDto
                {
                    Id = mc.Alumno.Id,
                    Nombre = mc.Alumno.Nombre ?? string.Empty,
                    Apellido = mc.Alumno.Apellido ?? string.Empty,
                    Email = mc.Alumno.Email
                } : null,
                Factura = mc.Factura != null ? new FacturaSimpleDto
                {
                    Id = mc.Factura.Id,
                    NumeroFactura = mc.Factura.NumeroFactura ?? string.Empty,
                    Monto = mc.Factura.Monto,
                    FechaEmision = mc.Factura.FechaEmision,
                    FechaVencimiento = mc.Factura.FechaVencimiento,
                    Estado = mc.Factura.Estado.ToString()
                } : null
            })
            .ToListAsync();

        return conciliaciones;
    }

    public async Task<MovimientoDetalleDto> GetMovimientoDetalleAsync(Guid movimientoBancarioId)
    {
        var movimiento = await _context.MovimientosBancarios
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == movimientoBancarioId);

        if (movimiento == null)
        {
            throw new ApplicationException("Movimiento bancario no encontrado");
        }

        var conciliacion = await _context.MovimientosConciliacion
            .AsNoTracking()
            .Include(mc => mc.Alumno)
            .Include(mc => mc.Factura)
            .FirstOrDefaultAsync(mc => mc.MovimientoBancarioId == movimientoBancarioId);

        var idPrefix = $"BANK:{movimientoBancarioId}";
        var pagos = await _context.Pagos
            .AsNoTracking()
            .Include(p => p.Factura)
            .Where(p => p.IdempotencyKey.StartsWith(idPrefix))
            .OrderBy(p => p.FechaPago)
            .ToListAsync();

        // Batch load alumnos to avoid N+1
        var alumnoIds = new HashSet<Guid>();
        if (conciliacion?.AlumnoId.HasValue == true)
            alumnoIds.Add(conciliacion.AlumnoId.Value);
        foreach (var pago in pagos)
        {
            if (pago.AlumnoId.HasValue)
                alumnoIds.Add(pago.AlumnoId.Value);
        }

        var alumnos = await _context.Alumnos
            .AsNoTracking()
            .Where(a => alumnoIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id);

        // Map pagos with alumno data
        var pagosDto = pagos.Select(p => new PagoDetalleDto
        {
            Id = p.Id,
            FacturaId = p.FacturaId,
            AlumnoId = p.AlumnoId,
            IdempotencyKey = p.IdempotencyKey,
            Monto = p.Monto,
            FechaPago = p.FechaPago,
            Metodo = p.Metodo.ToString(),
            Alumno = p.AlumnoId.HasValue && alumnos.TryGetValue(p.AlumnoId.Value, out var alumno)
                ? new AlumnoSimpleDto
                {
                    Id = alumno.Id,
                    Matricula = alumno.Matricula,
                    Nombre = alumno.Nombre,
                    Apellido = alumno.Apellido,
                    Email = alumno.Email
                }
                : null
        }).ToList();

        var facturasAfectadas = pagos
            .Where(p => p.Factura != null)
            .Select(p => p.Factura!)
            .GroupBy(f => f.Id)
            .Select(g => g.First())
            .Select(f => new FacturaSimpleDto
            {
                Id = f.Id,
                NumeroFactura = f.NumeroFactura ?? string.Empty,
                Monto = f.Monto,
                FechaEmision = f.FechaEmision,
                FechaVencimiento = f.FechaVencimiento,
                Estado = f.Estado.ToString()
            })
            .ToList();

        // Outcome
        var outcome = new OutcomeDto();
        var hoy = DateTime.UtcNow;
        var esViejoSinConciliar = movimiento.Estado == EstadoConciliacion.NoConciliado && movimiento.Fecha < hoy.AddDays(-7);

        if (conciliacion != null)
        {
            outcome.Status = "Success";
            outcome.Code = "CONCILIADO";
            outcome.Message = "Movimiento conciliado";
        }
        else if (pagos.Any())
        {
            outcome.Status = "Success";
            outcome.Code = "PAGOS_REGISTRADOS";
            outcome.Message = "Existen pagos relacionados por IdempotencyKey";
        }
        else if (esViejoSinConciliar)
        {
            outcome.Status = "Warning";
            outcome.Code = "OLD_UNCONCILED";
            outcome.Message = "Movimiento antiguo sin conciliación";
        }
        else
        {
            outcome.Status = "Info";
            outcome.Code = "SIN_DATOS";
            outcome.Message = "Sin conciliación ni pagos relacionados";
        }

        var detalle = new MovimientoDetalleDto
        {
            Movimiento = new MovimientoDetalleMovimientoDto
            {
                Id = movimiento.Id,
                Fecha = movimiento.Fecha,
                Descripcion = movimiento.Descripcion,
                Monto = movimiento.Monto,
                Tipo = movimiento.Tipo.ToString(),
                Estado = movimiento.Estado.ToString()
            },
            Conciliacion = conciliacion != null ? new MovimientoDetalleConciliacionDto
            {
                ConciliacionId = conciliacion.Id,
                FechaConciliacion = conciliacion.FechaConciliacion,
                Comentario = conciliacion.Comentario ?? string.Empty,
                Alumno = conciliacion.Alumno != null ? new AlumnoSimpleDto
                {
                    Id = conciliacion.Alumno.Id,
                    Matricula = conciliacion.Alumno.Matricula,
                    Nombre = conciliacion.Alumno.Nombre,
                    Apellido = conciliacion.Alumno.Apellido,
                    Email = conciliacion.Alumno.Email
                } : null,
                Factura = conciliacion.Factura != null ? new FacturaSimpleDto
                {
                    Id = conciliacion.Factura.Id,
                    NumeroFactura = conciliacion.Factura.NumeroFactura ?? string.Empty,
                    Monto = conciliacion.Factura.Monto,
                    FechaEmision = conciliacion.Factura.FechaEmision,
                    FechaVencimiento = conciliacion.Factura.FechaVencimiento,
                    Estado = conciliacion.Factura.Estado.ToString()
                } : null
            } : null,
            PagosRelacionados = pagosDto,
            FacturasAfectadas = facturasAfectadas,
            Outcome = outcome
        };

        return detalle;
    }
}
