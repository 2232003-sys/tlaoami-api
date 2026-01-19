using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class AsignacionGrupoService : IAsignacionGrupoService
    {
        private readonly TlaoamiDbContext _context;

        public AsignacionGrupoService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<AlumnoGrupoDto> AsignarAlumnoAGrupoAsync(AsignarAlumnoGrupoDto dto)
        {
            // Verificar que alumno y grupo existen
            var alumno = await _context.Alumnos.FindAsync(dto.AlumnoId);
            if (alumno == null)
                throw new Exception("Alumno no encontrado");

            var grupo = await _context.Grupos.FindAsync(dto.GrupoId);
            if (grupo == null)
                throw new Exception("Grupo no encontrado");

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
            }

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
                Nombre = asignacionActiva.Grupo.Nombre,
                Grado = asignacionActiva.Grupo.Grado,
                Turno = asignacionActiva.Grupo.Turno,
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
