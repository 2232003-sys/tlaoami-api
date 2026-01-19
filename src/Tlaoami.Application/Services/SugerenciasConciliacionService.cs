using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services;

public class SugerenciasConciliacionService : ISugerenciasConciliacionService
{
    private readonly TlaoamiDbContext _context;

    public SugerenciasConciliacionService(TlaoamiDbContext context)
    {
        _context = context;
    }

    public async Task<List<SugerenciaConciliacionDto>> GetSugerenciasAsync(Guid movimientoBancarioId)
    {
        var movimiento = await _context.MovimientosBancarios
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == movimientoBancarioId);

        if (movimiento == null)
        {
            throw new ApplicationException($"Movimiento bancario con ID {movimientoBancarioId} no encontrado");
        }

        if (movimiento.Estado != Domain.Entities.EstadoConciliacion.NoConciliado)
        {
            throw new InvalidOperationException("El movimiento ya está conciliado o ignorado");
        }

        var sugerencias = new List<SugerenciaConciliacionDto>();

        var facturasPendientes = await _context.Facturas
            .AsNoTracking()
            .Include(f => f.Alumno)
            .Where(f => f.Estado == Domain.Entities.EstadoFactura.Pendiente || 
                       f.Estado == Domain.Entities.EstadoFactura.ParcialmentePagada)
            .ToListAsync();

        foreach (var factura in facturasPendientes)
        {
            var similitud = CalcularSimilitud(movimiento, factura);
            
            if (similitud > 0.3m)
            {
                var razon = ObtenerRazon(movimiento, factura, similitud);
                
                sugerencias.Add(new SugerenciaConciliacionDto
                {
                    AlumnoId = factura.AlumnoId,
                    NombreAlumno = $"{factura.Alumno?.Nombre} {factura.Alumno?.Apellido}".Trim(),
                    EmailAlumno = factura.Alumno?.Email ?? string.Empty,
                    FacturaId = factura.Id,
                    NumeroFactura = factura.NumeroFactura,
                    MontoFactura = factura.Monto,
                    Similitud = similitud,
                    Razon = razon
                });
            }
        }

        return sugerencias.OrderByDescending(s => s.Similitud).Take(10).ToList();
    }

    private decimal CalcularSimilitud(Domain.Entities.MovimientoBancario movimiento, Domain.Entities.Factura factura)
    {
        decimal similitud = 0;

        var diferenciaMonto = Math.Abs(movimiento.Monto - factura.Monto);
        var porcentajeDiferencia = diferenciaMonto / factura.Monto;
        
        if (porcentajeDiferencia < 0.01m)
            similitud += 0.5m;
        else if (porcentajeDiferencia < 0.05m)
            similitud += 0.3m;
        else if (porcentajeDiferencia < 0.1m)
            similitud += 0.1m;

        var descripcionLower = movimiento.Descripcion.ToLower();
        var numeroFacturaLower = factura.NumeroFactura?.ToLower() ?? string.Empty;
        var nombreAlumno = factura.Alumno?.Nombre?.ToLower() ?? string.Empty;
        var apellidoAlumno = factura.Alumno?.Apellido?.ToLower() ?? string.Empty;

        if (descripcionLower.Contains(numeroFacturaLower) && !string.IsNullOrEmpty(numeroFacturaLower))
            similitud += 0.3m;
        
        if (descripcionLower.Contains(nombreAlumno) && !string.IsNullOrEmpty(nombreAlumno))
            similitud += 0.1m;
        
        if (descripcionLower.Contains(apellidoAlumno) && !string.IsNullOrEmpty(apellidoAlumno))
            similitud += 0.1m;

        return Math.Min(similitud, 1.0m);
    }

    private string ObtenerRazon(Domain.Entities.MovimientoBancario movimiento, Domain.Entities.Factura factura, decimal similitud)
    {
        var razones = new List<string>();

        var diferenciaMonto = Math.Abs(movimiento.Monto - factura.Monto);
        var porcentajeDiferencia = diferenciaMonto / factura.Monto;

        if (porcentajeDiferencia < 0.01m)
            razones.Add("Monto exacto");
        else if (porcentajeDiferencia < 0.05m)
            razones.Add("Monto muy similar");

        var descripcionLower = movimiento.Descripcion.ToLower();
        if (descripcionLower.Contains(factura.NumeroFactura?.ToLower() ?? string.Empty))
            razones.Add("Número de factura encontrado");

        if (descripcionLower.Contains(factura.Alumno?.Nombre?.ToLower() ?? string.Empty) ||
            descripcionLower.Contains(factura.Alumno?.Apellido?.ToLower() ?? string.Empty))
            razones.Add("Nombre de alumno encontrado");

        return razones.Any() ? string.Join(", ", razones) : "Similitud general";
    }
}
