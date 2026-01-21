using System;
using System.Collections.Generic;
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
    public class BecaAlumnoService : IBecaAlumnoService
    {
        private readonly TlaoamiDbContext _context;

        public BecaAlumnoService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<List<BecaAlumnoDto>> GetAllAsync(Guid? cicloId = null, Guid? alumnoId = null, bool? activa = null)
        {
            var query = _context.BecasAlumno.AsQueryable();

            if (cicloId.HasValue)
                query = query.Where(b => b.CicloId == cicloId.Value);

            if (alumnoId.HasValue)
                query = query.Where(b => b.AlumnoId == alumnoId.Value);

            if (activa.HasValue)
                query = query.Where(b => b.Activa == activa.Value);

            var becas = await query
                .OrderBy(b => b.AlumnoId)
                .ThenBy(b => b.CicloId)
                .ToListAsync();

            return becas.Select(MapToDto).ToList();
        }

        public async Task<BecaAlumnoDto> GetByIdAsync(Guid id)
        {
            var beca = await _context.BecasAlumno.FirstOrDefaultAsync(b => b.Id == id);
            if (beca == null)
                throw new NotFoundException($"Beca con ID {id} no encontrada.", code: "BECA_NO_ENCONTRADA");

            return MapToDto(beca);
        }

        public async Task<BecaAlumnoDto> CreateAsync(BecaAlumnoCreateDto dto)
        {
            Validate(dto.Tipo, dto.Valor);
            await EnsureAlumnoYCiclo(dto.AlumnoId, dto.CicloId);

            var exists = await _context.BecasAlumno.AnyAsync(b => b.AlumnoId == dto.AlumnoId && b.CicloId == dto.CicloId);
            if (exists)
                throw new BusinessException("Ya existe una beca activa para el alumno en el ciclo indicado.", code: "BECA_DUPLICADA");

            var beca = new BecaAlumno
            {
                Id = Guid.NewGuid(),
                AlumnoId = dto.AlumnoId,
                CicloId = dto.CicloId,
                Tipo = dto.Tipo,
                Valor = dto.Valor,
                Activa = dto.Activa,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.BecasAlumno.Add(beca);
            await _context.SaveChangesAsync();

            return MapToDto(beca);
        }

        public async Task<BecaAlumnoDto> UpdateAsync(Guid id, BecaAlumnoUpdateDto dto)
        {
            var beca = await _context.BecasAlumno.FirstOrDefaultAsync(b => b.Id == id);
            if (beca == null)
                throw new NotFoundException($"Beca con ID {id} no encontrada.", code: "BECA_NO_ENCONTRADA");

            if (dto.Tipo.HasValue)
                beca.Tipo = dto.Tipo.Value;

            if (dto.Valor.HasValue)
                beca.Valor = dto.Valor.Value;

            if (dto.Tipo.HasValue || dto.Valor.HasValue)
                Validate(beca.Tipo, beca.Valor);

            if (dto.Activa.HasValue)
                beca.Activa = dto.Activa.Value;

            beca.UpdatedAtUtc = DateTime.UtcNow;
            _context.BecasAlumno.Update(beca);
            await _context.SaveChangesAsync();

            return MapToDto(beca);
        }

        public async Task InactivateAsync(Guid id)
        {
            var beca = await _context.BecasAlumno.FirstOrDefaultAsync(b => b.Id == id);
            if (beca == null)
                throw new NotFoundException($"Beca con ID {id} no encontrada.", code: "BECA_NO_ENCONTRADA");

            if (!beca.Activa)
                return;

            beca.Activa = false;
            beca.UpdatedAtUtc = DateTime.UtcNow;
            _context.BecasAlumno.Update(beca);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var beca = await _context.BecasAlumno.FirstOrDefaultAsync(b => b.Id == id);
            if (beca == null)
                throw new NotFoundException($"Beca con ID {id} no encontrada.", code: "BECA_NO_ENCONTRADA");

            _context.BecasAlumno.Remove(beca);
            await _context.SaveChangesAsync();
        }

        private static void Validate(BecaTipo tipo, decimal valor)
        {
            if (valor <= 0)
                throw new ValidationException("Valor de beca debe ser mayor a 0.", code: "BECA_VALOR_INVALIDO");

            if (tipo == BecaTipo.Porcentaje && valor > 1m)
                throw new ValidationException("Porcentaje de beca debe ser <= 1.0 (ej. 0.25 = 25%).", code: "BECA_PORCENTAJE_INVALIDO");
        }

        private async Task EnsureAlumnoYCiclo(Guid alumnoId, Guid cicloId)
        {
            var alumnoExiste = await _context.Alumnos.AnyAsync(a => a.Id == alumnoId);
            if (!alumnoExiste)
                throw new NotFoundException($"Alumno con ID {alumnoId} no encontrado.", code: "ALUMNO_NO_ENCONTRADO");

            var cicloExiste = await _context.CiclosEscolares.AnyAsync(c => c.Id == cicloId);
            if (!cicloExiste)
                throw new NotFoundException($"Ciclo escolar con ID {cicloId} no encontrado.", code: "CICLO_NO_ENCONTRADO");
        }

        private static BecaAlumnoDto MapToDto(BecaAlumno entity)
        {
            return new BecaAlumnoDto
            {
                Id = entity.Id,
                AlumnoId = entity.AlumnoId,
                CicloId = entity.CicloId,
                Tipo = entity.Tipo,
                Valor = entity.Valor,
                Activa = entity.Activa,
                CreatedAtUtc = entity.CreatedAtUtc,
                UpdatedAtUtc = entity.UpdatedAtUtc
            };
        }
    }
}
