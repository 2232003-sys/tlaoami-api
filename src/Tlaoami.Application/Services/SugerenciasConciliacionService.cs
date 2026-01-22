using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Infrastructure;
using System.Text.RegularExpressions;

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

        // Parse descripción para extraer matrícula o token numérico
        var matriculaEncontrada = ExtraerMatricula(movimiento.Descripcion);
        string? tokenNumerico = null;

        var facturasPendientes = await _context.Facturas
            .AsNoTracking()
            .Include(f => f.Alumno)
            .Where(f => f.Estado == Domain.Entities.EstadoFactura.Pendiente || 
                       f.Estado == Domain.Entities.EstadoFactura.ParcialmentePagada)
            .OrderBy(f => f.FechaVencimiento)
            .ThenBy(f => f.FechaEmision)
            .ToListAsync();

        // Heurística: si no hay matrícula exacta pero la descripción trae ALUMNO 1109 y existe un alumno con matrícula A1109 o 1109
        if (string.IsNullOrEmpty(matriculaEncontrada))
        {
            tokenNumerico = ExtraerNumeroAlumno(movimiento.Descripcion);
            if (!string.IsNullOrEmpty(tokenNumerico))
            {
                var alumnoCoincidente = facturasPendientes
                    .Where(f => f.Alumno != null)
                    .Select(f => f.Alumno!)
                    .GroupBy(a => a.Id)
                    .Select(g => g.First())
                    .FirstOrDefault(a => NormalizarMatriculaNumerica(a.Matricula) == tokenNumerico);

                if (alumnoCoincidente != null)
                {
                    matriculaEncontrada = alumnoCoincidente.Matricula;
                }
            }
        }

        // Si detectamos matrícula, priorizamos ese alumno
        if (!string.IsNullOrEmpty(matriculaEncontrada))
        {
            var matriculaUpper = matriculaEncontrada.ToUpper();
            var alumnoConMatricula = await _context.Alumnos
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Matricula.ToUpper() == matriculaUpper);

            if (alumnoConMatricula != null)
            {
                var facturasDelAlumno = facturasPendientes
                    .Where(f => f.AlumnoId == alumnoConMatricula.Id)
                    .ToList();

                // Buscar factura con monto exacto
                var facturaExacta = facturasDelAlumno
                    .FirstOrDefault(f => Math.Abs(f.Monto - movimiento.Monto) < 0.01m);

                if (facturaExacta != null)
                {
                    sugerencias.Add(new SugerenciaConciliacionDto
                    {
                        AlumnoId = alumnoConMatricula.Id,
                        Matricula = alumnoConMatricula.Matricula,
                        NombreAlumno = $"{alumnoConMatricula.Nombre} {alumnoConMatricula.Apellido}".Trim(),
                        EmailAlumno = alumnoConMatricula.Email ?? string.Empty,
                        FacturaId = facturaExacta.Id,
                        NumeroFactura = facturaExacta.NumeroFactura,
                        MontoFactura = facturaExacta.Monto,
                        AplicarACuenta = false,
                        Confidence = 0.95m,
                        Reason = $"Matrícula {matriculaEncontrada} detectada en descripción, monto exacto"
                    });
                    return sugerencias;
                }

                // Si no hay exacta, ofrecer FIFO + otras opciones
                if (facturasDelAlumno.Any())
                {
                    var primeraFactura = facturasDelAlumno.First();
                    sugerencias.Add(new SugerenciaConciliacionDto
                    {
                        AlumnoId = alumnoConMatricula.Id,
                        Matricula = alumnoConMatricula.Matricula,
                        NombreAlumno = $"{alumnoConMatricula.Nombre} {alumnoConMatricula.Apellido}".Trim(),
                        EmailAlumno = alumnoConMatricula.Email ?? string.Empty,
                        FacturaId = primeraFactura.Id,
                        NumeroFactura = primeraFactura.NumeroFactura,
                        MontoFactura = primeraFactura.Monto,
                        AplicarACuenta = true,
                        Confidence = 0.85m,
                        Reason = $"Matrícula {matriculaEncontrada} detectada, aplicar FIFO"
                    });
                    
                    // Agregar otras facturas del alumno
                    foreach (var factura in facturasDelAlumno.Skip(1).Take(5))
                    {
                        sugerencias.Add(new SugerenciaConciliacionDto
                        {
                            AlumnoId = alumnoConMatricula.Id,
                            Matricula = alumnoConMatricula.Matricula,
                            NombreAlumno = $"{alumnoConMatricula.Nombre} {alumnoConMatricula.Apellido}".Trim(),
                            EmailAlumno = alumnoConMatricula.Email ?? string.Empty,
                            FacturaId = factura.Id,
                            NumeroFactura = factura.NumeroFactura,
                            MontoFactura = factura.Monto,
                            AplicarACuenta = false,
                            Confidence = 0.60m,
                            Reason = $"Factura pendiente del alumno {matriculaEncontrada}"
                        });
                    }
                }
            }
        }

        // Si no hay matrícula o no encontramos alumno, buscar por monto exacto y similitud
        if (!sugerencias.Any())
        {
            foreach (var factura in facturasPendientes)
            {
                var confidence = CalcularConfianza(movimiento, factura);
                
                if (confidence > 0.3m)
                {
                    var razon = ObtenerRazon(movimiento, factura, confidence);
                    
                    sugerencias.Add(new SugerenciaConciliacionDto
                    {
                        AlumnoId = factura.AlumnoId,
                        Matricula = factura.Alumno?.Matricula ?? string.Empty,
                        NombreAlumno = $"{factura.Alumno?.Nombre} {factura.Alumno?.Apellido}".Trim(),
                        EmailAlumno = factura.Alumno?.Email ?? string.Empty,
                        FacturaId = factura.Id,
                        NumeroFactura = factura.NumeroFactura,
                        MontoFactura = factura.Monto,
                        AplicarACuenta = false,
                        Confidence = confidence,
                        Reason = razon
                    });
                }
            }
        }

        return sugerencias.OrderByDescending(s => s.Confidence).Take(10).ToList();
    }

    /// <summary>
    /// Extrae matrícula de la descripción (ej: A1109, LEG-001, MAT-001)
    /// </summary>
    private string? ExtraerMatricula(string descripcion)
    {
        if (string.IsNullOrEmpty(descripcion))
            return null;

        // Patrones comunes: A1109, LEG-001, MAT-001, etc
        var patterns = new[]
        {
            @"(?:^|\s)([A-Z]{1,3}\d{1,6})(?:\s|$|[^a-zA-Z0-9])", // A1109, LEG001, MAT001
            @"(?:^|\s)([A-Z]{2,5}-\d{3,6})(?:\s|$|[^a-zA-Z0-9])", // LEG-001, MAT-001
            @"^MAT(?:RICULA)?[:\s]*([A-Z0-9]{1,10})", // MATRICULA: ABC123
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(descripcion, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                var matricula = match.Groups[1].Value.Trim().ToUpper();
                if (!string.IsNullOrEmpty(matricula) && matricula.Length >= 3)
                    return matricula;
            }
        }

        return null;
    }

    /// <summary>
    /// Extrae token numérico de alumno en descripciones tipo "ALUMNO 1109" o números sueltos de 3-6 dígitos
    /// </summary>
    private string? ExtraerNumeroAlumno(string descripcion)
    {
        if (string.IsNullOrEmpty(descripcion))
            return null;

        // Busca patrones ALUMNO 1109
        var matchAlumno = Regex.Match(descripcion, @"alumno\s+(\d{3,6})", RegexOptions.IgnoreCase);
        if (matchAlumno.Success)
            return matchAlumno.Groups[1].Value;

        // Busca cualquier token numérico de 3 a 6 dígitos
        var matchNumero = Regex.Match(descripcion, @"\b(\d{3,6})\b");
        if (matchNumero.Success)
            return matchNumero.Groups[1].Value;

        return null;
    }

    /// <summary>
    /// Devuelve solo la parte numérica de una matrícula (A1109 -> 1109, LEG-001 -> 001)
    /// </summary>
    private string NormalizarMatriculaNumerica(string matricula)
    {
        if (string.IsNullOrEmpty(matricula)) return string.Empty;
        var digits = new string(matricula.Where(char.IsDigit).ToArray());
        return digits;
    }
    private decimal CalcularConfianza(Domain.Entities.MovimientoBancario movimiento, Domain.Entities.Factura factura)
    {
        decimal confianza = 0;

        var diferenciaMonto = Math.Abs(movimiento.Monto - factura.Monto);
        var porcentajeDiferencia = diferenciaMonto / factura.Monto;
        
        if (porcentajeDiferencia < 0.01m)
            confianza += 0.5m;
        else if (porcentajeDiferencia < 0.05m)
            confianza += 0.3m;
        else if (porcentajeDiferencia < 0.1m)
            confianza += 0.1m;

        var descripcionLower = movimiento.Descripcion.ToLower();
        var numeroFacturaLower = factura.NumeroFactura?.ToLower() ?? string.Empty;
        var nombreAlumno = factura.Alumno?.Nombre?.ToLower() ?? string.Empty;
        var apellidoAlumno = factura.Alumno?.Apellido?.ToLower() ?? string.Empty;

        if (descripcionLower.Contains(numeroFacturaLower) && !string.IsNullOrEmpty(numeroFacturaLower))
            confianza += 0.3m;
        
        if (descripcionLower.Contains(nombreAlumno) && !string.IsNullOrEmpty(nombreAlumno))
            confianza += 0.1m;
        
        if (descripcionLower.Contains(apellidoAlumno) && !string.IsNullOrEmpty(apellidoAlumno))
            confianza += 0.1m;

        return Math.Min(confianza, 1.0m);
    }

    private string ObtenerRazon(Domain.Entities.MovimientoBancario movimiento, Domain.Entities.Factura factura, decimal confianza)
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
