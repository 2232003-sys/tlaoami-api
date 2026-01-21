using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enums;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class ColegiaturasService : IColegiaturasService
    {
        private readonly TlaoamiDbContext _context;

        public ColegiaturasService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<ColegiaturaGeneracionResultDto> GenerarMensualAsync(ColegiaturaGeneracionRequestDto request)
        {
            ValidatePeriodo(request.Periodo);
            if (request.CicloId == Guid.Empty)
                throw new ValidationException("CicloId es requerido.", code: "CICLO_REQUERIDO");
            if (!request.GrupoId.HasValue || request.GrupoId == Guid.Empty)
                throw new ValidationException("GrupoId es requerido para primaria.", code: "GRUPO_REQUERIDO");

            var periodo = ParsePeriodo(request.Periodo);
            var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.Id == request.GrupoId.Value);
            if (grupo == null)
                throw new NotFoundException($"Grupo con ID {request.GrupoId} no encontrado.", code: "GRUPO_NO_ENCONTRADO");
            if (grupo.CicloEscolarId != request.CicloId)
                throw new ValidationException("El grupo no pertenece al ciclo indicado.", code: "GRUPO_CICLO_INCONSISTENTE");

            var regla = await ResolverReglaColegiaturaAsync(request.CicloId, grupo);
            var conceptoColegiaturaId = regla.ConceptoCobroId;
            var concepto = await _context.ConceptosCobro.FirstOrDefaultAsync(c => c.Id == conceptoColegiaturaId);

            var asignaciones = await _context.AsignacionesGrupo
                .Include(ag => ag.Alumno)
                .Where(ag => ag.GrupoId == request.GrupoId.Value && ag.Activo)
                .ToListAsync();

            var alumnos = asignaciones.Where(a => a.Alumno != null && a.Alumno.Activo).ToList();
            var alumnosIds = alumnos.Select(a => a.AlumnoId).ToList();

            var becas = await _context.BecasAlumno
                .Where(b => b.Activa && b.CicloId == request.CicloId && alumnosIds.Contains(b.AlumnoId))
                .ToListAsync();

            var facturasExistentes = await _context.Facturas
                .Where(f => f.Periodo == request.Periodo && f.ConceptoCobroId == conceptoColegiaturaId && alumnosIds.Contains(f.AlumnoId) && f.Estado != EstadoFactura.Cancelada)
                .Select(f => new { f.Id, f.AlumnoId })
                .ToListAsync();

            var result = new ColegiaturaGeneracionResultDto
            {
                TotalAlumnos = alumnos.Count
            };

            if (request.DryRun || !alumnos.Any())
            {
                result.OmitidasPorExistir = facturasExistentes.Count;
                result.Creadas = request.DryRun ? alumnos.Count - result.OmitidasPorExistir : 0;
                return result;
            }

            var siguienteNumero = await ObtenerSiguienteNumeroFacturaAsync();
            foreach (var asignacion in alumnos)
            {
                if (facturasExistentes.Any(f => f.AlumnoId == asignacion.AlumnoId))
                {
                    result.OmitidasPorExistir++;
                    continue;
                }

                try
                {
                    var beca = becas.FirstOrDefault(b => b.AlumnoId == asignacion.AlumnoId);
                    var montoFinal = CalcularMontoConBeca(regla.MontoBase, beca);

                    var numeroFactura = $"FAC-{siguienteNumero:D6}";
                    siguienteNumero++;

                    var fechaEmision = new DateTime(periodo.Year, periodo.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var fechaVencimiento = new DateTime(periodo.Year, periodo.Month, regla.DiaVencimiento, 0, 0, 0, DateTimeKind.Utc);

                    var factura = new Factura
                    {
                        AlumnoId = asignacion.AlumnoId,
                        NumeroFactura = numeroFactura,
                        Concepto = concepto?.Nombre ?? "Colegiatura",
                        Periodo = request.Periodo,
                        ConceptoCobroId = conceptoColegiaturaId,
                        TipoDocumento = TipoDocumento.Factura,
                        Monto = montoFinal,
                        FechaEmision = fechaEmision,
                        FechaVencimiento = fechaVencimiento,
                        Estado = EstadoFactura.Borrador,
                        Lineas = new List<FacturaLinea>(),
                        Pagos = new List<Pago>()
                    };

                    factura.Lineas.Add(new FacturaLinea
                    {
                        Id = Guid.NewGuid(),
                        Factura = factura,
                        ConceptoCobroId = conceptoColegiaturaId,
                        Descripcion = $"Colegiatura {request.Periodo}",
                        Subtotal = montoFinal,
                        Descuento = 0,
                        Impuesto = 0,
                        CreatedAtUtc = DateTime.UtcNow
                    });

                    factura.RecalculateFrom(factura.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)), factura.Pagos ?? Enumerable.Empty<Pago>());

                    if (request.Emitir)
                    {
                        factura.Estado = EstadoFactura.Pendiente;
                        factura.IssuedAt = DateTime.UtcNow;
                    }

                    _context.Facturas.Add(factura);
                    result.Creadas++;
                }
                catch (Exception ex)
                {
                    result.Errores.Add($"Alumno {asignacion.AlumnoId}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<RecargoAplicacionResultDto> AplicarRecargosAsync(RecargoAplicacionRequestDto request)
        {
            ValidatePeriodo(request.Periodo);
            if (request.CicloId == Guid.Empty)
                throw new ValidationException("CicloId es requerido.", code: "CICLO_REQUERIDO");

            var regla = await _context.ReglasRecargo.FirstOrDefaultAsync(r => r.CicloId == request.CicloId && r.Activa);
            if (regla == null)
                throw new NotFoundException("Regla de recargo activa no encontrada para el ciclo.", code: "RECARGO_NO_ENCONTRADO");
            if (regla.Porcentaje <= 0 || regla.Porcentaje > 1)
                throw new ValidationException("Porcentaje de recargo inválido.", code: "PORCENTAJE_INVALIDO");
            if (regla.DiasGracia < 0 || regla.DiasGracia > 31)
                throw new ValidationException("DiasGracia debe estar entre 0 y 31.", code: "DIAS_GRACIA_INVALIDO");

            var conceptoColegiatura = await _context.ConceptosCobro.FirstOrDefaultAsync(c => c.Clave.ToUpper() == "COLEGIATURA");
            if (conceptoColegiatura == null)
                throw new NotFoundException("Concepto COLEGIATURA no encontrado.", code: "CONCEPTO_COLEGIATURA_FALTA");

            var conceptoRecargo = await _context.ConceptosCobro.FirstOrDefaultAsync(c => c.Id == regla.ConceptoCobroId);
            if (conceptoRecargo == null)
                throw new NotFoundException("Concepto de recargo no encontrado.", code: "CONCEPTO_RECARGO_FALTA");

            var hoy = DateTime.UtcNow.Date;
            var facturas = await _context.Facturas
                .Include(f => f.Lineas)
                .Include(f => f.Pagos)
                .Where(f => f.Periodo == request.Periodo && f.ConceptoCobroId == conceptoColegiatura.Id && f.Estado != EstadoFactura.Cancelada && f.Estado != EstadoFactura.Borrador)
                .ToListAsync();

            var result = new RecargoAplicacionResultDto
            {
                FacturasEvaluadas = facturas.Count
            };

            foreach (var factura in facturas)
            {
                try
                {
                    factura.RecalculateFrom(factura.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)), factura.Pagos ?? Enumerable.Empty<Pago>());
                    var saldo = factura.Monto - (factura.Pagos?.Sum(p => p.Monto) ?? 0m);
                    if (saldo <= 0.01m)
                    {
                        result.OmitidasPorExistir++;
                        continue;
                    }

                    if (factura.FechaVencimiento.Date.AddDays(regla.DiasGracia) > hoy)
                    {
                        result.OmitidasPorExistir++;
                        continue;
                    }

                    if (factura.Lineas.Any(l => l.ConceptoCobroId == conceptoRecargo.Id))
                    {
                        result.OmitidasPorExistir++;
                        continue;
                    }

                    var recargo = Math.Round(saldo * regla.Porcentaje, 2, MidpointRounding.AwayFromZero);
                    if (recargo < 0.01m)
                    {
                        result.OmitidasPorExistir++;
                        continue;
                    }

                    if (request.DryRun)
                    {
                        result.Aplicadas++;
                        continue;
                    }

                    factura.Lineas.Add(new FacturaLinea
                    {
                        Id = Guid.NewGuid(),
                        Factura = factura,
                        ConceptoCobroId = conceptoRecargo.Id,
                        Descripcion = $"Recargo mora {request.Periodo}",
                        Subtotal = recargo,
                        Descuento = 0,
                        Impuesto = 0,
                        CreatedAtUtc = DateTime.UtcNow
                    });

                    factura.RecalculateFrom(factura.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)), factura.Pagos ?? Enumerable.Empty<Pago>());
                    result.Aplicadas++;
                }
                catch (Exception ex)
                {
                    result.Errores.Add($"Factura {factura.Id}: {ex.Message}");
                }
            }

            if (!request.DryRun)
            {
                await _context.SaveChangesAsync();
            }

            return result;
        }

        private static void ValidatePeriodo(string periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo) || periodo.Length != 7)
                throw new ValidationException("Periodo debe tener formato YYYY-MM.", code: "PERIODO_INVALIDO");
            if (!DateTime.TryParseExact(periodo, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                throw new ValidationException("Periodo debe tener formato YYYY-MM.", code: "PERIODO_INVALIDO");
        }

        private static DateTime ParsePeriodo(string periodo)
        {
            return DateTime.SpecifyKind(DateTime.ParseExact(periodo, "yyyy-MM", CultureInfo.InvariantCulture), DateTimeKind.Utc);
        }

        private static decimal CalcularMontoConBeca(decimal montoBase, BecaAlumno? beca)
        {
            if (beca == null || !beca.Activa)
                return Math.Round(montoBase, 2, MidpointRounding.AwayFromZero);

            decimal montoFinal = montoBase;
            if (beca.Tipo == BecaTipo.Porcentaje)
                montoFinal = montoBase * (1 - beca.Valor);
            else if (beca.Tipo == BecaTipo.MontoFijo)
                montoFinal = montoBase - beca.Valor;

            if (montoFinal < 0)
                montoFinal = 0;

            return Math.Round(montoFinal, 2, MidpointRounding.AwayFromZero);
        }

        private async Task<int> ObtenerSiguienteNumeroFacturaAsync()
        {
            var ultima = await _context.Facturas
                .OrderByDescending(f => f.NumeroFactura)
                .Select(f => f.NumeroFactura)
                .FirstOrDefaultAsync();

            var siguiente = 1;
            if (!string.IsNullOrEmpty(ultima) && ultima!.StartsWith("FAC-"))
            {
                var parte = ultima.Substring(4);
                if (int.TryParse(parte, out var numero))
                    siguiente = numero + 1;
            }
            return siguiente;
        }

        private async Task<ReglaColegiatura> ResolverReglaColegiaturaAsync(Guid cicloId, Grupo grupo)
        {
            var reglas = await _context.ReglasColegiatura
                .Where(r => r.CicloId == cicloId && r.Activa)
                .ToListAsync();

            var porGrupo = reglas.FirstOrDefault(r => r.GrupoId == grupo.Id);
            if (porGrupo != null)
                return porGrupo;

            var porGradoTurno = reglas.FirstOrDefault(r => r.GrupoId == null && r.Grado == grupo.Grado && (string.IsNullOrWhiteSpace(r.Turno) || r.Turno == grupo.Turno));
            if (porGradoTurno != null)
                return porGradoTurno;

            var generica = reglas.FirstOrDefault(r => r.GrupoId == null && r.Grado == null && r.Turno == null);
            if (generica != null)
                return generica;

            throw new NotFoundException("No existe una regla de colegiatura activa para el grupo/ciclo indicado.", code: "REGLA_NO_ENCONTRADA");
        }
    }
}
