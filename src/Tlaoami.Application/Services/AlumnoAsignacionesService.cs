using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Services
{
    public class AlumnoAsignacionesService : IAlumnoAsignacionesService
    {
        private readonly TlaoamiDbContext _context;

        public AlumnoAsignacionesService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<AlumnoAsignacionDto> CreateAsignacionAsync(Guid alumnoId, AlumnoAsignacionCreateDto dto)
        {
            var alumno = await _context.Alumnos.FindAsync(alumnoId);
            if (alumno == null)
                throw new NotFoundException("ALUMNO_NOT_FOUND", $"Alumno {alumnoId} no encontrado");

            var concepto = await _context.ConceptosCobro.FindAsync(dto.ConceptoCobroId);
            if (concepto == null)
                throw new NotFoundException("CONCEPTO_NOT_FOUND", $"ConceptoCobro {dto.ConceptoCobroId} no encontrado");

            var ciclo = await _context.CiclosEscolares.FindAsync(dto.CicloId);
            if (ciclo == null)
                throw new NotFoundException("CICLO_NOT_FOUND", $"CicloEscolar {dto.CicloId} no encontrado");

            var asignacion = new AlumnoAsignacion
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumnoId,
                ConceptoCobroId = dto.ConceptoCobroId,
                CicloId = dto.CicloId,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                MontoOverride = dto.MontoOverride,
                Activo = dto.Activo,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.AlumnoAsignaciones.Add(asignacion);
            await _context.SaveChangesAsync();

            return MapToDto(asignacion);
        }

        public async Task<bool> CancelarAsignacionAsync(Guid asignacionId)
        {
            var asignacion = await _context.AlumnoAsignaciones.FindAsync(asignacionId);
            if (asignacion == null)
                return false;

            asignacion.Activo = false;
            asignacion.FechaFin = DateTime.UtcNow;
            asignacion.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<AlumnoAsignacionDto>> ListarAsignacionesPorAlumnoAsync(Guid alumnoId)
        {
            var asignaciones = await _context.AlumnoAsignaciones
                .Where(a => a.AlumnoId == alumnoId)
                .OrderByDescending(a => a.FechaInicio)
                .ToListAsync();

            return asignaciones.Select(MapToDto).ToList();
        }

        private AlumnoAsignacionDto MapToDto(AlumnoAsignacion entity)
        {
            return new AlumnoAsignacionDto
            {
                Id = entity.Id,
                AlumnoId = entity.AlumnoId,
                ConceptoCobroId = entity.ConceptoCobroId,
                CicloId = entity.CicloId,
                FechaInicio = entity.FechaInicio,
                FechaFin = entity.FechaFin,
                MontoOverride = entity.MontoOverride,
                Activo = entity.Activo
            };
        }
    }
}
