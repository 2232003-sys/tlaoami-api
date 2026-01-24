using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class ReinscripcionService : IReinscripcionService
    {
        private readonly TlaoamiDbContext _context;
        private readonly IAsignacionGrupoService _asignacionService;
        private readonly IAlumnoService _alumnoService;

        public ReinscripcionService(
            TlaoamiDbContext context,
            IAsignacionGrupoService asignacionService,
            IAlumnoService alumnoService)
        {
            _context = context;
            _asignacionService = asignacionService;
            _alumnoService = alumnoService;
        }

        public async Task<IEnumerable<ReinscripcionPreviewItemDto>> PreviewAsync(Guid cicloOrigenId, Guid cicloDestinoId)
        {
            var alumnos = await _context.AsignacionesGrupo
                .Where(ag => ag.Grupo.CicloEscolarId == cicloOrigenId)
                .Select(ag => new
                {
                    ag.AlumnoId,
                    AlumnoNombre = (ag.Alumno.Nombre ?? string.Empty) + " " + (ag.Alumno.Apellido ?? string.Empty),
                    ag.GrupoId,
                    GrupoCodigo = ag.Grupo.Codigo
                })
                .Distinct()
                .ToListAsync();

            var yaReinscritos = (await _context.Reinscripciones
                .Where(r => r.CicloDestinoId == cicloDestinoId)
                .Select(r => r.AlumnoId)
                .ToListAsync())
                .ToHashSet();

            var result = new List<ReinscripcionPreviewItemDto>();

            foreach (var a in alumnos)
            {
                var estado = await _alumnoService.GetEstadoCuentaAsync(a.AlumnoId);
                if (estado == null)
                    continue;

                var bloqueado = estado.SaldoPendiente > 0.01m;

                result.Add(new ReinscripcionPreviewItemDto
                {
                    AlumnoId = a.AlumnoId,
                    NombreAlumno = a.AlumnoNombre.Trim(),
                    GrupoOrigenId = a.GrupoId,
                    GrupoOrigenCodigo = a.GrupoCodigo,
                    SaldoPendiente = estado.SaldoPendiente,
                    Bloqueado = bloqueado,
                    MotivoBloqueo = bloqueado ? "ADEUDO" : null,
                    YaReinscrito = yaReinscritos.Contains(a.AlumnoId)
                });
            }

            return result;
        }

        public async Task EjecutarAsync(ReinscripcionEjecutarDto dto)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            foreach (var item in dto.Items)
            {
                // Idempotencia básica: evitar duplicados por índice único
                var yaExiste = await _context.Reinscripciones.AnyAsync(r => r.AlumnoId == item.AlumnoId && r.CicloDestinoId == dto.CicloDestinoId);
                if (yaExiste)
                    continue;

                var estado = await _alumnoService.GetEstadoCuentaAsync(item.AlumnoId);
                if (estado == null)
                    throw new NotFoundException("ESTADO_CUENTA_NO_DISPONIBLE");

                // Captura de grupo/ciclo origen si existe
                var asignacionOrigen = await _context.AsignacionesGrupo
                    .Include(ag => ag.Grupo)
                    .FirstOrDefaultAsync(ag => ag.AlumnoId == item.AlumnoId && ag.Grupo != null && ag.Grupo.CicloEscolarId == dto.CicloOrigenId);

                if (estado.SaldoPendiente > 0.01m)
                {
                    _context.Reinscripciones.Add(new Reinscripcion
                    {
                        Id = Guid.NewGuid(),
                        AlumnoId = item.AlumnoId,
                        CicloOrigenId = asignacionOrigen?.Grupo?.CicloEscolarId ?? dto.CicloOrigenId,
                        GrupoOrigenId = asignacionOrigen?.GrupoId,
                        CicloDestinoId = dto.CicloDestinoId,
                        GrupoDestinoId = item.GrupoDestinoId,
                        Estado = "BLOQUEADA_ADEUDO",
                        MotivoBloqueo = "ADEUDO",
                        SaldoAlMomento = estado.SaldoPendiente,
                        CreatedAtUtc = DateTime.UtcNow
                    });
                    continue;
                }

                _context.AsignacionesGrupo.Add(new AlumnoGrupo
                {
                    Id = Guid.NewGuid(),
                    AlumnoId = item.AlumnoId,
                    GrupoId = item.GrupoDestinoId,
                    FechaInicio = DateTime.UtcNow,
                    Activo = true
                });

                _context.Reinscripciones.Add(new Reinscripcion
                {
                    Id = Guid.NewGuid(),
                    AlumnoId = item.AlumnoId,
                    CicloOrigenId = asignacionOrigen?.Grupo?.CicloEscolarId ?? dto.CicloOrigenId,
                    GrupoOrigenId = asignacionOrigen?.GrupoId,
                    CicloDestinoId = dto.CicloDestinoId,
                    GrupoDestinoId = item.GrupoDestinoId,
                    Estado = "COMPLETADA",
                    SaldoAlMomento = estado.SaldoPendiente,
                    CreatedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task<ReinscripcionDto> CrearReinscripcionAsync(ReinscripcionCreateDto dto, Guid? usuarioId = null)
        {
            // 1. Validar que alumno existe
            var alumno = await _context.Alumnos.FindAsync(dto.AlumnoId);
            if (alumno == null)
                throw new NotFoundException("Alumno no encontrado", code: "ALUMNO_NO_ENCONTRADO");

            // 2. Validar que ciclo destino existe
            var cicloDestino = await _context.CiclosEscolares.FindAsync(dto.CicloDestinoId);
            if (cicloDestino == null)
                throw new NotFoundException("Ciclo destino no encontrado", code: "CICLO_NO_ENCONTRADO");

            // 3. Validar que grupo destino existe y pertenece al ciclo destino
            var grupoDestino = await _context.Grupos
                .FirstOrDefaultAsync(g => g.Id == dto.GrupoDestinoId && g.CicloEscolarId == dto.CicloDestinoId);
            if (grupoDestino == null)
                throw new NotFoundException(
                    "Grupo destino no encontrado en el ciclo especificado",
                    code: "GRUPO_NO_ENCONTRADO");

            // 4. Obtener saldo actual (fuente de verdad: estado de cuenta)
            var estadoCuenta = await _alumnoService.GetEstadoCuentaAsync(dto.AlumnoId);
            if (estadoCuenta == null)
                throw new NotFoundException("Estado de cuenta no disponible", code: "ESTADO_CUENTA_NO_DISPONIBLE");
            
            decimal saldoActual = estadoCuenta.SaldoPendiente;

            // 5. VALIDAR ADEUDO: si saldo > 0.01 => bloquear
            if (saldoActual > 0.01m)
            {
                throw new BusinessException(
                    code: "REINSCRIPCION_BLOQUEADA_ADEUDO",
                    message: $"Alumno con adeudo pendiente: ${saldoActual:C}"
                );
            }

            // 6. Verificar que alumno NO esté ya inscrito en el ciclo destino (evitar duplicado)
            var yaInscrito = await _context.AsignacionesGrupo
                .AnyAsync(ag => ag.AlumnoId == dto.AlumnoId 
                    && ag.Grupo != null 
                    && ag.Grupo.CicloEscolarId == dto.CicloDestinoId);

            if (yaInscrito)
                throw new BusinessException(
                    "El alumno ya está inscrito en el ciclo destino",
                    code: "ALUMNO_YA_INSCRITO_EN_CICLO");

            // 7. Obtener asignación actual (grupo actual)
            var asignacionActual = await _context.AsignacionesGrupo
                .FirstOrDefaultAsync(ag => ag.AlumnoId == dto.AlumnoId);

            Guid? cicloOrigenId = asignacionActual?.Grupo?.CicloEscolarId;
            Guid? grupoOrigenId = asignacionActual?.GrupoId;

            // 8. Usar transacción para desasignar (si existe) y asignar nuevo
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 8a. Si hay asignación actual, desasignarla
                    if (asignacionActual != null)
                    {
                        _context.AsignacionesGrupo.Remove(asignacionActual);
                        await _context.SaveChangesAsync();
                    }

                    // 8b. Validar capacidad del grupo destino
                    if (grupoDestino.Capacidad.HasValue)
                    {
                        var countEnGrupoDestino = await _context.AsignacionesGrupo
                            .CountAsync(ag => ag.GrupoId == dto.GrupoDestinoId);

                        if (countEnGrupoDestino >= grupoDestino.Capacidad.Value)
                        {
                            throw new BusinessException(
                                "Grupo destino sin capacidad",
                                code: "GRUPO_SIN_CUPO");
                        }
                    }

                    // 8c. Crear nueva asignación
                    var nuevaAsignacion = new AlumnoGrupo
                    {
                        Id = Guid.NewGuid(),
                        AlumnoId = dto.AlumnoId,
                        GrupoId = dto.GrupoDestinoId,
                        FechaInicio = DateTime.UtcNow,
                        Activo = true
                    };

                    _context.AsignacionesGrupo.Add(nuevaAsignacion);

                    // 8d. Crear registro de reinscripción exitosa
                    var reinscripcionExitosa = new Reinscripcion
                    {
                        Id = Guid.NewGuid(),
                        AlumnoId = dto.AlumnoId,
                        CicloOrigenId = cicloOrigenId,
                        GrupoOrigenId = grupoOrigenId,
                        CicloDestinoId = dto.CicloDestinoId,
                        GrupoDestinoId = dto.GrupoDestinoId,
                        Estado = "Completada",
                        MotivoBloqueo = null,
                        SaldoAlMomento = saldoActual,
                        CreatedAtUtc = DateTime.UtcNow,
                        CompletedAtUtc = DateTime.UtcNow,
                        CreatedByUserId = usuarioId
                    };

                    _context.Reinscripciones.Add(reinscripcionExitosa);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return MapToDto(reinscripcionExitosa);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<ReinscripcionDto?> GetReinscripcionAsync(Guid reinscripcionId)
        {
            var reinscripcion = await _context.Reinscripciones.FindAsync(reinscripcionId);
            return reinscripcion == null ? null : MapToDto(reinscripcion);
        }

        public async Task<IEnumerable<ReinscripcionDto>> GetReinscripcionesPorAlumnoAsync(Guid alumnoId, Guid? cicloDestinoId = null)
        {
            var query = _context.Reinscripciones
                .Where(r => r.AlumnoId == alumnoId);

            if (cicloDestinoId.HasValue)
                query = query.Where(r => r.CicloDestinoId == cicloDestinoId.Value);

            var reinscripciones = await query
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync();

            return reinscripciones.Select(MapToDto);
        }

        // === Privados ===

        private static ReinscripcionDto MapToDto(Reinscripcion entity)
        {
            return new ReinscripcionDto
            {
                Id = entity.Id,
                AlumnoId = entity.AlumnoId,
                CicloOrigenId = entity.CicloOrigenId,
                GrupoOrigenId = entity.GrupoOrigenId,
                CicloDestinoId = entity.CicloDestinoId,
                GrupoDestinoId = entity.GrupoDestinoId,
                Estado = entity.Estado,
                MotivoBloqueo = entity.MotivoBloqueo,
                SaldoAlMomento = entity.SaldoAlMomento,
                CreatedAtUtc = entity.CreatedAtUtc,
                CompletedAtUtc = entity.CompletedAtUtc
            };
        }
    }
}
