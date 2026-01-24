using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Application.Exceptions;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class AsignacionGrupoService : IAsignacionGrupoService
    {
        private readonly TlaoamiDbContext _context;
        private readonly IAlumnoService _alumnoService;

        public AsignacionGrupoService(TlaoamiDbContext context, IAlumnoService alumnoService)
        {
            _context = context;
            _alumnoService = alumnoService;
        }

        public async Task<AlumnoGrupoDto> AsignarAlumnoAGrupoAsync(AsignarAlumnoGrupoDto dto)
        {
            // Usar transacción para evitar condición de carrera y validar cerca del insert
            await using var tx = await _context.Database.BeginTransactionAsync();

            // Verificar que alumno y grupo existen
            var alumno = await _context.Alumnos.FindAsync(dto.AlumnoId);
            if (alumno == null)
                throw new NotFoundException("Alumno no encontrado", code: "ALUMNO_NO_ENCONTRADO");

            var grupo = await _context.Grupos.FindAsync(dto.GrupoId);
            if (grupo == null)
                throw new NotFoundException("Grupo no encontrado", code: "GRUPO_NO_ENCONTRADO");

            // VALIDAR ADEUDO: bloquear cambio de grupo si hay saldo pendiente
            var estadoCuenta = await _alumnoService.GetEstadoCuentaAsync(dto.AlumnoId);
            if (estadoCuenta.SaldoPendiente > 0.01m)
            {
                throw new BusinessException(
                    code: "CAMBIO_GRUPO_BLOQUEADO_ADEUDO",
                    message: $"El alumno tiene un adeudo pendiente de ${estadoCuenta.SaldoPendiente:N2}. No se puede cambiar de grupo."
                );
            }

            // Cerrar asignación activa anterior si existe
            var asignacionActiva = await _context.AsignacionesGrupo
                .FirstOrDefaultAsync(ag => 
                    ag.AlumnoId == dto.AlumnoId && 
                    ag.Activo && 
                    ag.FechaFin == null);

            if (asignacionActiva != null)
            {
                asignacionActiva.FechaFin = dto.FechaInicio.AddDays(-1);
                asignacionActiva.Activo = false;
                await _context.SaveChangesAsync();
            }

            // Validar capacidad del grupo (no sobrecargar) justo antes del insert
            var asignadosActivos = await _context.AsignacionesGrupo
                .CountAsync(ag => ag.GrupoId == dto.GrupoId && ag.Activo && ag.FechaFin == null);
            if (grupo.Capacidad.HasValue && asignadosActivos >= grupo.Capacidad.Value)
                throw new BusinessException("Capacidad del grupo alcanzada", code: "GRUPO_SIN_CUPO");

            // Crear nueva asignación
            var nuevaAsignacion = new AlumnoGrupo
            {
                Id = Guid.NewGuid(),
                AlumnoId = dto.AlumnoId,
                GrupoId = dto.GrupoId,
                FechaInicio = dto.FechaInicio,
                FechaFin = null,
                Activo = true
            };

            _context.AsignacionesGrupo.Add(nuevaAsignacion);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return MapToDto(nuevaAsignacion);
        }

        public async Task<GrupoDto?> GetGrupoActualDeAlumnoAsync(Guid alumnoId)
        {
            var asignacionActiva = await _context.AsignacionesGrupo
                .Include(ag => ag.Grupo)
                .ThenInclude(g => g!.CicloEscolar)
                .FirstOrDefaultAsync(ag =>
                    ag.AlumnoId == alumnoId &&
                    ag.Activo &&
                    ag.FechaFin == null);

            if (asignacionActiva?.Grupo == null)
                return null;

            return new GrupoDto
            {
                Id = asignacionActiva.Grupo.Id,
                Codigo = asignacionActiva.Grupo.Codigo,
                Nombre = asignacionActiva.Grupo.Nombre,
                Grado = asignacionActiva.Grupo.Grado,
                Seccion = asignacionActiva.Grupo.Seccion,
                Turno = asignacionActiva.Grupo.Turno,
                Activo = asignacionActiva.Grupo.Activo,
                CicloEscolarId = asignacionActiva.Grupo.CicloEscolarId,
                CicloNombre = asignacionActiva.Grupo.CicloEscolar?.Nombre
            };
        }

        public async Task<bool> DesasignarAlumnoDeGrupoAsync(Guid alumnoId)
        {
            var asignacionActiva = await _context.AsignacionesGrupo
                .FirstOrDefaultAsync(ag =>
                    ag.AlumnoId == alumnoId &&
                    ag.Activo &&
                    ag.FechaFin == null);

            if (asignacionActiva == null)
                return false;

            asignacionActiva.FechaFin = DateTime.UtcNow;
            asignacionActiva.Activo = false;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<AlumnoGrupoDto>> GetHistorialAsignacionesAlumnoAsync(Guid alumnoId)
        {
            var historial = await _context.AsignacionesGrupo
                .Where(ag => ag.AlumnoId == alumnoId)
                .OrderBy(ag => ag.FechaInicio)
                .ToListAsync();

            return historial.Select(MapToDto);
        }

        private static AlumnoGrupoDto MapToDto(AlumnoGrupo ag)
        {
            return new AlumnoGrupoDto
            {
                Id = ag.Id,
                AlumnoId = ag.AlumnoId,
                GrupoId = ag.GrupoId,
                FechaInicio = ag.FechaInicio,
                FechaFin = ag.FechaFin,
                Activo = ag.Activo
            };
        }
    }
}
