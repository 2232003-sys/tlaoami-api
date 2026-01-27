using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enums;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Finanzas
{
    /// <summary>
    /// Servicio para generar cargos recurrentes basados en AlumnoAsignaciones.
    /// Usa el mismo modelo de Factura/FacturaLinea que ColegiaturasService.
    /// </summary>
    public class GenerarCargosRecurrentesService : IGenerarCargosRecurrentesService
    {
        private readonly TlaoamiDbContext _context;
        private readonly IEscuelaSettingsService _settingsService;
        private readonly IReglaRecargoService _recargoService;

        public GenerarCargosRecurrentesService(
            TlaoamiDbContext context,
            IEscuelaSettingsService settingsService,
            IReglaRecargoService recargoService)
        {
            _context = context;
            _settingsService = settingsService;
            _recargoService = recargoService;
        }

        /// <summary>
        /// Genera cargos mensuales para todas las asignaciones activas del periodo.
        /// Lógica:
        /// 1. Obtener EscuelaSettings (día de corte)
        /// 2. Obtener asignaciones activas para el periodo
        /// 3. Verificar si ya existe factura del mes
        /// 4. Crear Factura con línea
        /// 5. Aplicar recargo si hoy > día corte
        /// </summary>
        public async Task<GenerarCargosRecurrentesResultDto> GenerarCargosAsync(string periodo, Guid cicloId, bool emitir = true)
        {
            ValidatePeriodo(periodo);
            if (cicloId == Guid.Empty)
                throw new ValidationException("CicloId es requerido.", code: "CICLO_REQUERIDO");

            var ciclo = await _context.CiclosEscolares.FindAsync(cicloId);
            if (ciclo == null)
                throw new NotFoundException("Ciclo escolar no encontrado.", code: "CICLO_NO_ENCONTRADO");

            // 1. Obtener settings de la escuela
            var settings = await _settingsService.GetSettingsAsync();
            var diaCorte = settings?.DiaCorteColegiatura ?? 5; // Default día 5 si no hay settings

            var periodoDate = ParsePeriodo(periodo);

            // 2. Obtener asignaciones activas del periodo
            var asignaciones = await _context.AlumnoAsignaciones
                .Include(a => a.Alumno)
                .Include(a => a.ConceptoCobro)
                .Where(a => a.CicloId == cicloId
                    && a.Activo
                    && a.FechaInicio <= periodoDate.AddMonths(1).AddDays(-1) // Antes de fin de mes
                    && (a.FechaFin == null || a.FechaFin >= periodoDate)) // Vigente al inicio del mes
                .ToListAsync();

            var alumnosIds = asignaciones.Select(a => a.AlumnoId).Distinct().ToList();
            if (!alumnosIds.Any())
            {
                return new GenerarCargosRecurrentesResultDto
                {
                    TotalAsignaciones = 0,
                    CargosCreados = 0,
                    OmitidasPorExistir = 0
                };
            }

            // 3. Verificar facturas existentes
            var conceptosIds = asignaciones.Select(a => a.ConceptoCobroId).Distinct().ToList();
            var facturasExistentes = await _context.Facturas
                .Where(f => f.Periodo == periodo
                    && conceptosIds.Contains(f.ConceptoCobroId!.Value)
                    && alumnosIds.Contains(f.AlumnoId)
                    && f.Estado != EstadoFactura.Cancelada)
                .Select(f => new { f.AlumnoId, f.ConceptoCobroId })
                .ToListAsync();

            // Obtener becas activas
            var becas = await _context.BecasAlumno
                .Where(b => b.Activa && b.CicloId == cicloId && alumnosIds.Contains(b.AlumnoId))
                .ToListAsync();

            // Obtener regla de recargo del ciclo para aplicar si corresponde
            var reglaRecargo = await _context.ReglasRecargo
                .FirstOrDefaultAsync(r => r.CicloId == cicloId && r.Activa);

            var conceptoRecargo = reglaRecargo != null
                ? await _context.ConceptosCobro.FindAsync(reglaRecargo.ConceptoCobroId)
                : null;

            var result = new GenerarCargosRecurrentesResultDto
            {
                TotalAsignaciones = asignaciones.Count
            };

            var hoy = DateTime.UtcNow.Date;
            var fechaVencimiento = new DateTime(periodoDate.Year, periodoDate.Month, diaCorte, 0, 0, 0, DateTimeKind.Utc);
            var aplicarRecargo = reglaRecargo != null && hoy > fechaVencimiento.AddDays(reglaRecargo.DiasGracia);

            var siguienteNumero = await ObtenerSiguienteNumeroFacturaAsync();

            // 4. Crear Facturas para cada asignación
            foreach (var asignacion in asignaciones)
            {
                // Verificar si ya existe
                if (facturasExistentes.Any(f => f.AlumnoId == asignacion.AlumnoId && f.ConceptoCobroId == asignacion.ConceptoCobroId))
                {
                    result.OmitidasPorExistir++;
                    continue;
                }

                try
                {
                    var alumno = asignacion.Alumno;
                    if (alumno == null || !alumno.Activo)
                    {
                        result.Errores.Add($"Alumno {asignacion.AlumnoId} inactivo o no encontrado");
                        continue;
                    }

                    var concepto = asignacion.ConceptoCobro;
                    if (concepto == null || !concepto.Activo)
                    {
                        result.Errores.Add($"Concepto {asignacion.ConceptoCobroId} inactivo o no encontrado");
                        continue;
                    }

                    // Determinar monto base: override de asignación o null (regla por defecto en lógica futura)
                    var montoBase = asignacion.MontoOverride ?? 0m;
                    if (montoBase <= 0m)
                    {
                        // Si no hay override, intentar obtener de ReglaColegiatura si es colegiatura
                        var reglaColegiatura = await ObtenerReglaColegiaturaParaAlumnoAsync(alumno.Id, cicloId, concepto.Id);
                        montoBase = reglaColegiatura?.MontoBase ?? 0m;
                    }

                    if (montoBase <= 0m)
                    {
                        result.Errores.Add($"Alumno {alumno.Matricula}: monto base 0 o no definido para concepto {concepto.Nombre}");
                        continue;
                    }

                    // Aplicar beca
                    var beca = becas.FirstOrDefault(b => b.AlumnoId == asignacion.AlumnoId);
                    var montoFinal = CalcularMontoConBeca(montoBase, beca);

                    var numeroFactura = $"FAC-{siguienteNumero:D6}";
                    siguienteNumero++;

                    var fechaEmision = new DateTime(periodoDate.Year, periodoDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                    var factura = new Factura
                    {
                        Id = Guid.NewGuid(),
                        AlumnoId = asignacion.AlumnoId,
                        NumeroFactura = numeroFactura,
                        Concepto = concepto.Nombre,
                        Periodo = periodo,
                        ConceptoCobroId = concepto.Id,
                        TipoDocumento = TipoDocumento.Factura,
                        Monto = montoFinal,
                        FechaEmision = fechaEmision,
                        FechaVencimiento = fechaVencimiento,
                        Estado = EstadoFactura.Borrador,
                        Lineas = new List<FacturaLinea>(),
                        Pagos = new List<Pago>()
                    };

                    // Agregar línea del concepto
                    factura.Lineas.Add(new FacturaLinea
                    {
                        Id = Guid.NewGuid(),
                        Factura = factura,
                        ConceptoCobroId = concepto.Id,
                        Descripcion = $"{concepto.Nombre} {periodo}",
                        Subtotal = montoFinal,
                        Descuento = 0m,
                        Impuesto = 0m,
                        CreatedAtUtc = DateTime.UtcNow
                    });

                    // 5. Aplicar recargo si corresponde
                    if (aplicarRecargo && conceptoRecargo != null && concepto.AplicaRecargo)
                    {
                        var montoRecargo = Math.Round(montoFinal * reglaRecargo!.Porcentaje, 2, MidpointRounding.AwayFromZero);
                        if (montoRecargo >= 0.01m)
                        {
                            factura.Lineas.Add(new FacturaLinea
                            {
                                Id = Guid.NewGuid(),
                                Factura = factura,
                                ConceptoCobroId = conceptoRecargo.Id,
                                Descripcion = $"Recargo mora {periodo}",
                                Subtotal = montoRecargo,
                                Descuento = 0m,
                                Impuesto = 0m,
                                CreatedAtUtc = DateTime.UtcNow
                            });
                        }
                    }

                    // Recalcular totales
                    factura.RecalculateFrom(
                        factura.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)),
                        factura.Pagos ?? Enumerable.Empty<Pago>());

                    // Emitir si se solicitó
                    if (emitir)
                    {
                        factura.Estado = EstadoFactura.Pendiente;
                        factura.IssuedAt = DateTime.UtcNow;
                    }

                    _context.Facturas.Add(factura);
                    result.CargosCreados++;
                }
                catch (Exception ex)
                {
                    result.Errores.Add($"Asignación {asignacion.Id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
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
            return DateTime.SpecifyKind(
                DateTime.ParseExact(periodo, "yyyy-MM", CultureInfo.InvariantCulture),
                DateTimeKind.Utc);
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

        /// <summary>
        /// Intenta obtener la regla de colegiatura para el alumno en el ciclo.
        /// Resuelve por grupo del alumno, luego por grado/turno, luego genérica.
        /// </summary>
        private async Task<ReglaColegiatura?> ObtenerReglaColegiaturaParaAlumnoAsync(Guid alumnoId, Guid cicloId, Guid conceptoCobroId)
        {
            var asignacionGrupo = await _context.AsignacionesGrupo
                .Include(ag => ag.Grupo)
                .FirstOrDefaultAsync(ag => ag.AlumnoId == alumnoId && ag.Activo);

            if (asignacionGrupo?.Grupo == null)
                return null;

            var reglas = await _context.ReglasColegiatura
                .Where(r => r.CicloId == cicloId && r.ConceptoCobroId == conceptoCobroId && r.Activa)
                .ToListAsync();

            // Prioridad: GrupoId específico > Grado+Turno > Genérica
            var porGrupo = reglas.FirstOrDefault(r => r.GrupoId == asignacionGrupo.GrupoId);
            if (porGrupo != null)
                return porGrupo;

            var porGradoTurno = reglas.FirstOrDefault(r =>
                r.GrupoId == null
                && r.Grado == asignacionGrupo.Grupo.Grado
                && (string.IsNullOrWhiteSpace(r.Turno) || r.Turno == asignacionGrupo.Grupo.Turno));
            if (porGradoTurno != null)
                return porGradoTurno;

            var generica = reglas.FirstOrDefault(r => r.GrupoId == null && r.Grado == null && r.Turno == null);
            return generica;
        }
    }
}
