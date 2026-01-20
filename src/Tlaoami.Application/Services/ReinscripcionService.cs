using System;
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
                // Registrar intento de reinscripción bloqueada
                var reinscripcionBloqueada = new Reinscripcion
                {
                    Id = Guid.NewGuid(),
                    AlumnoId = dto.AlumnoId,
                    CicloDestinoId = dto.CicloDestinoId,
                    GrupoDestinoId = dto.GrupoDestinoId,
                    Estado = "Bloqueada",
                    MotivoBloqueo = "ADEUDO",
                    SaldoAlMomento = saldoActual,
                    CreatedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = DateTime.UtcNow,
                    CreatedByUserId = usuarioId
                };

                _context.Reinscripciones.Add(reinscripcionBloqueada);
                await _context.SaveChangesAsync();

                // Lanzar excepción
                throw new BusinessException(
                    $"Reinscripción bloqueada por adeudo. Saldo pendiente: ${saldoActual:F2}",
                    code: "REINSCRIPCION_BLOQUEADA_ADEUDO");
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
