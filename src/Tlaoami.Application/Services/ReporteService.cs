using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class ReporteService : IReporteService
    {
        private readonly TlaoamiDbContext _context;

        public ReporteService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdeudoDto>> GetAdeudosAsync(
            Guid? cicloId = null,
            Guid? grupoId = null,
            int? grado = null,
            DateTime? fechaCorte = null)
        {
            // Obtener facturas no canceladas con sus pagos
            var facturasQuery = _context.Facturas
                .AsNoTracking()
                .Include(f => f.Alumno)
                .ThenInclude(a => a.AsignacionesGrupo)
                .ThenInclude(ag => ag.Grupo)
                .Include(f => f.Pagos)
                .Where(f => f.Estado != EstadoFactura.Cancelada);

            // Aplicar filtros
            if (cicloId.HasValue)
            {
                facturasQuery = facturasQuery.Where(f => 
                    f.Alumno.AsignacionesGrupo.Any(ag => ag.Activo && ag.Grupo.CicloEscolarId == cicloId.Value));
            }

            if (grupoId.HasValue)
            {
                facturasQuery = facturasQuery.Where(f => 
                    f.Alumno.AsignacionesGrupo.Any(ag => ag.Activo && ag.GrupoId == grupoId.Value));
            }

            if (grado.HasValue)
            {
                facturasQuery = facturasQuery.Where(f => 
                    f.Alumno.AsignacionesGrupo.Any(ag => ag.Activo && ag.Grupo.Grado == grado.Value));
            }

            var facturas = await facturasQuery.ToListAsync();

            // Agrupar por alumno y calcular adeudos
            var adeudos = facturas
                .GroupBy(f => f.Alumno)
                .Select(g =>
                {
                    var alumno = g.Key;
                    var asignacionActiva = alumno.AsignacionesGrupo.FirstOrDefault(ag => ag.Activo);
                    
                    var totalFacturado = g.Sum(f => f.Monto);
                    
                    // Filtrar pagos por fecha de corte si aplica
                    var pagosValidos = g.SelectMany(f => f.Pagos ?? new List<Pago>())
                        .Where(p => !fechaCorte.HasValue || p.FechaPago <= fechaCorte.Value)
                        .ToList();
                    
                    var totalPagado = pagosValidos.Sum(p => p.Monto);
                    var saldo = totalFacturado - totalPagado;
                    
                    // Tolerancia de 0.01
                    if (Math.Abs(saldo) < 0.01m)
                        saldo = 0;

                    var ultimoPago = pagosValidos.OrderByDescending(p => p.FechaPago).FirstOrDefault();

                    return new AdeudoDto
                    {
                        AlumnoId = alumno.Id,
                        Matricula = alumno.Matricula,
                        NombreCompleto = $"{alumno.Nombre} {alumno.Apellido}",
                        Grupo = asignacionActiva?.Grupo?.Nombre,
                        Grado = asignacionActiva?.Grupo?.Grado,
                        TotalFacturado = totalFacturado,
                        TotalPagado = totalPagado,
                        Saldo = saldo,
                        UltimoPagoAtUtc = ultimoPago?.FechaPago
                    };
                })
                .OrderBy(a => a.Matricula)
                .ToList();

            return adeudos;
        }

        public async Task<IEnumerable<PagoReporteDto>> GetPagosAsync(
            DateTime from,
            DateTime to,
            Guid? grupoId = null,
            string? metodo = null)
        {
            var pagosQuery = _context.Pagos
                .AsNoTracking()
                .Include(p => p.Factura)
                .ThenInclude(f => f!.Alumno)
                .ThenInclude(a => a.AsignacionesGrupo)
                .ThenInclude(ag => ag.Grupo)
                .Where(p => p.FechaPago >= from && p.FechaPago <= to);

            // Filtro por grupo (si el pago tiene factura vinculada)
            if (grupoId.HasValue)
            {
                pagosQuery = pagosQuery.Where(p => 
                    p.Factura != null && 
                    p.Factura.Alumno.AsignacionesGrupo.Any(ag => ag.Activo && ag.GrupoId == grupoId.Value));
            }

            // Filtro por método
            if (!string.IsNullOrWhiteSpace(metodo))
            {
                if (Enum.TryParse<MetodoPago>(metodo, true, out var metodoPago))
                {
                    pagosQuery = pagosQuery.Where(p => p.Metodo == metodoPago);
                }
            }

            var pagos = await pagosQuery
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            var pagosReporte = pagos.Select(p =>
            {
                var alumno = p.Factura?.Alumno;
                
                return new PagoReporteDto
                {
                    PagoId = p.Id,
                    FechaUtc = p.FechaPago,
                    AlumnoId = p.AlumnoId ?? alumno?.Id,
                    AlumnoNombre = alumno != null ? $"{alumno.Nombre} {alumno.Apellido}" : null,
                    FacturaId = p.FacturaId,
                    Monto = p.Monto,
                    Metodo = p.Metodo.ToString(),
                    Referencia = p.IdempotencyKey,
                    CapturadoPorUserId = null // TODO: agregar cuando tengamos auditoría
                };
            }).ToList();

            return pagosReporte;
        }

        public async Task<string> ExportAdeudosToCsvAsync(
            Guid? cicloId = null,
            Guid? grupoId = null,
            int? grado = null,
            DateTime? fechaCorte = null)
        {
            var adeudos = await GetAdeudosAsync(cicloId, grupoId, grado, fechaCorte);
            
            var csv = new StringBuilder();
            csv.AppendLine("Matricula,Nombre Completo,Grupo,Grado,Total Facturado,Total Pagado,Saldo,Ultimo Pago");

            foreach (var adeudo in adeudos)
            {
                csv.AppendLine($"{adeudo.Matricula}," +
                              $"\"{adeudo.NombreCompleto}\"," +
                              $"\"{adeudo.Grupo ?? ""}\"," +
                              $"{adeudo.Grado?.ToString() ?? ""}," +
                              $"{adeudo.TotalFacturado.ToString("F2", CultureInfo.InvariantCulture)}," +
                              $"{adeudo.TotalPagado.ToString("F2", CultureInfo.InvariantCulture)}," +
                              $"{adeudo.Saldo.ToString("F2", CultureInfo.InvariantCulture)}," +
                              $"{adeudo.UltimoPagoAtUtc?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""}");
            }

            return csv.ToString();
        }

        public async Task<string> ExportPagosToCsvAsync(
            DateTime from,
            DateTime to,
            Guid? grupoId = null,
            string? metodo = null)
        {
            var pagos = await GetPagosAsync(from, to, grupoId, metodo);
            
            var csv = new StringBuilder();
            csv.AppendLine("Fecha,Alumno Nombre,Factura ID,Monto,Metodo,Referencia");

            foreach (var pago in pagos)
            {
                csv.AppendLine($"{pago.FechaUtc.ToString("yyyy-MM-dd HH:mm:ss")}," +
                              $"\"{pago.AlumnoNombre ?? "N/A"}\"," +
                              $"{pago.FacturaId?.ToString() ?? ""}," +
                              $"{pago.Monto.ToString("F2", CultureInfo.InvariantCulture)}," +
                              $"{pago.Metodo}," +
                              $"\"{pago.Referencia ?? ""}\"");
            }

            return csv.ToString();
        }
    }
}
