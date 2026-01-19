using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
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
}
