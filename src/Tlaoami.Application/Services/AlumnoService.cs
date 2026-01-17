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
            var alumnos = await _context.Alumnos.ToListAsync();
            return alumnos.Select(MappingFunctions.ToAlumnoDto);
        }

        public async Task<AlumnoDto?> GetAlumnoByIdAsync(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            return alumno != null ? MappingFunctions.ToAlumnoDto(alumno) : null;
        }

        public async Task<EstadoCuentaDto?> GetEstadoCuentaAsync(Guid id)
        {
            var alumno = await _context.Alumnos.Include(a => a.Facturas)
                                                .ThenInclude(f => f.Pagos)
                                                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null) return null;

            return MappingFunctions.ToEstadoCuentaDto(alumno);
        }

        public async Task<AlumnoDto> CreateAlumnoAsync(AlumnoDto alumnoDto)
        {
            var alumno = new Alumno
            {
                Nombre = alumnoDto.Nombre,
                Apellido = alumnoDto.Apellido,
                Email = alumnoDto.Email,
                FechaInscripcion = alumnoDto.FechaInscripcion
            };

            _context.Alumnos.Add(alumno);
            await _context.SaveChangesAsync();

            return MappingFunctions.ToAlumnoDto(alumno);
        }

        public async Task UpdateAlumnoAsync(Guid id, AlumnoDto alumnoDto)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno != null)
            {
                alumno.Nombre = alumnoDto.Nombre;
                alumno.Apellido = alumnoDto.Apellido;
                alumno.Email = alumnoDto.Email;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAlumnoAsync(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno != null)
            {
                _context.Alumnos.Remove(alumno);
                await _context.SaveChangesAsync();
            }
        }
    }
}
