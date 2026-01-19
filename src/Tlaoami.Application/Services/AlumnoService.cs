using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Mappers;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class AlumnoService : IAlumnoService
    {
        private readonly TlaoamiDbContext _context;

        public AlumnoService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AlumnoDto>> GetAllAlumnosAsync()
        {
            var alumnos = await _context.Alumnos
                .Where(a => a.Activo)
                .ToListAsync();
            return alumnos.Select(MappingFunctions.ToAlumnoDto);
        }

        public async Task<AlumnoDto?> GetAlumnoByIdAsync(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            return alumno != null ? MappingFunctions.ToAlumnoDto(alumno) : null;
        }

        public async Task<AlumnoDto?> GetAlumnoByMatriculaAsync(string matricula)
        {
            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a => a.Matricula == matricula);
            return alumno != null ? MappingFunctions.ToAlumnoDto(alumno) : null;
        }

        public async Task<AlumnoDto?> GetAlumnoConGrupoActualAsync(Guid id)
        {
            var alumno = await _context.Alumnos
                .Include(a => a.AsignacionesGrupo)
                .ThenInclude(ag => ag.Grupo)
                .ThenInclude(g => g!.CicloEscolar)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null)
                return null;

            var dto = MappingFunctions.ToAlumnoDto(alumno);

            // Obtener grupo activo
            var asignacionActiva = alumno.AsignacionesGrupo
                ?.FirstOrDefault(ag => ag.Activo && ag.FechaFin == null);

            if (asignacionActiva?.Grupo != null)
            {
                dto.GrupoActual = new GrupoDto
                {
                    Id = asignacionActiva.Grupo.Id,
                    Nombre = asignacionActiva.Grupo.Nombre,
                    Grado = asignacionActiva.Grupo.Grado,
                    Turno = asignacionActiva.Grupo.Turno,
                    CicloEscolarId = asignacionActiva.Grupo.CicloEscolarId,
                    CicloNombre = asignacionActiva.Grupo.CicloEscolar?.Nombre
                };
            }

            return dto;
        }

        public async Task<EstadoCuentaDto?> GetEstadoCuentaAsync(Guid id)
        {
            var alumno = await _context.Alumnos.Include(a => a.Facturas)
                                                .ThenInclude(f => f.Pagos)
                                                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null) return null;

            return MappingFunctions.ToEstadoCuentaDto(alumno);
        }

        public async Task<AlumnoDto> CreateAlumnoAsync(AlumnoCreateDto dto)
        {
            var alumno = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = dto.Matricula,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Telefono = dto.Telefono,
                Activo = true,
                FechaInscripcion = DateTime.UtcNow
            };

            _context.Alumnos.Add(alumno);
            await _context.SaveChangesAsync();

            return MappingFunctions.ToAlumnoDto(alumno);
        }

        public async Task<AlumnoDto> UpdateAlumnoAsync(Guid id, AlumnoUpdateDto dto)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null)
                throw new Exception("Alumno no encontrado");

            if (!string.IsNullOrEmpty(dto.Nombre))
                alumno.Nombre = dto.Nombre;
            if (!string.IsNullOrEmpty(dto.Apellido))
                alumno.Apellido = dto.Apellido;
            if (!string.IsNullOrEmpty(dto.Email))
                alumno.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Telefono))
                alumno.Telefono = dto.Telefono;
            if (dto.Activo.HasValue)
                alumno.Activo = dto.Activo.Value;

            await _context.SaveChangesAsync();

            return MappingFunctions.ToAlumnoDto(alumno);
        }

        public async Task<bool> DeleteAlumnoAsync(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null)
                return false;

            alumno.Activo = false;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
