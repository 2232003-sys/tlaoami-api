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
    public class ReglaRecargoService : IReglaRecargoService
    {
        private readonly TlaoamiDbContext _context;

        public ReglaRecargoService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReglaRecargoDto>> GetAllAsync(Guid? cicloId = null, bool? activa = null)
        {
            var query = _context.ReglasRecargo.AsQueryable();

            if (cicloId.HasValue)
                query = query.Where(r => r.CicloId == cicloId.Value);

            if (activa.HasValue)
                query = query.Where(r => r.Activa == activa.Value);

            var reglas = await query
                .OrderBy(r => r.CicloId)
                .ToListAsync();

            return reglas.Select(MapToDto).ToList();
        }

        public async Task<ReglaRecargoDto> GetByIdAsync(Guid id)
        {
            var regla = await _context.ReglasRecargo.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de recargo con ID {id} no encontrada.", code: "RECARGO_NO_ENCONTRADO");

            return MapToDto(regla);
        }

        public async Task<ReglaRecargoDto> CreateAsync(ReglaRecargoCreateDto dto)
        {
            Validate(dto.DiasGracia, dto.Porcentaje);
            await EnsureForeignsAsync(dto.CicloId, dto.ConceptoCobroId);

            var exists = await _context.ReglasRecargo.AnyAsync(r => r.CicloId == dto.CicloId);
            if (exists)
                throw new BusinessException("Ya existe una regla de recargo para el ciclo indicado.", code: "RECARGO_DUPLICADO");

            var regla = new ReglaRecargo
            {
                Id = Guid.NewGuid(),
                CicloId = dto.CicloId,
                ConceptoCobroId = dto.ConceptoCobroId,
                DiasGracia = dto.DiasGracia,
                Porcentaje = dto.Porcentaje,
                Activa = dto.Activa,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.ReglasRecargo.Add(regla);
            await _context.SaveChangesAsync();

            return MapToDto(regla);
        }

        public async Task<ReglaRecargoDto> UpdateAsync(Guid id, ReglaRecargoUpdateDto dto)
        {
            var regla = await _context.ReglasRecargo.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de recargo con ID {id} no encontrada.", code: "RECARGO_NO_ENCONTRADO");

            if (dto.DiasGracia.HasValue)
            {
                Validate(dto.DiasGracia.Value, dto.Porcentaje ?? regla.Porcentaje);
                regla.DiasGracia = dto.DiasGracia.Value;
            }

            if (dto.Porcentaje.HasValue)
            {
                Validate(dto.DiasGracia ?? regla.DiasGracia, dto.Porcentaje.Value);
                regla.Porcentaje = dto.Porcentaje.Value;
            }

            if (dto.Activa.HasValue)
                regla.Activa = dto.Activa.Value;

            regla.UpdatedAtUtc = DateTime.UtcNow;
            _context.ReglasRecargo.Update(regla);
            await _context.SaveChangesAsync();

            return MapToDto(regla);
        }

        public async Task InactivateAsync(Guid id)
        {
            var regla = await _context.ReglasRecargo.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de recargo con ID {id} no encontrada.", code: "RECARGO_NO_ENCONTRADO");

            if (!regla.Activa)
                return;

            regla.Activa = false;
            regla.UpdatedAtUtc = DateTime.UtcNow;
            _context.ReglasRecargo.Update(regla);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var regla = await _context.ReglasRecargo.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de recargo con ID {id} no encontrada.", code: "RECARGO_NO_ENCONTRADO");

            _context.ReglasRecargo.Remove(regla);
            await _context.SaveChangesAsync();
        }

        private static void Validate(int diasGracia, decimal porcentaje)
        {
            if (diasGracia < 0 || diasGracia > 31)
                throw new ValidationException("DiasGracia debe estar entre 0 y 31.", code: "DIAS_GRACIA_INVALIDO");
            if (porcentaje <= 0 || porcentaje > 1)
                throw new ValidationException("Porcentaje de recargo debe ser > 0 y <= 1.0.", code: "PORCENTAJE_INVALIDO");
        }

        private async Task EnsureForeignsAsync(Guid cicloId, Guid conceptoId)
        {
            var cicloExiste = await _context.CiclosEscolares.AnyAsync(c => c.Id == cicloId);
            if (!cicloExiste)
                throw new NotFoundException($"Ciclo escolar con ID {cicloId} no encontrado.", code: "CICLO_NO_ENCONTRADO");

            var conceptoExiste = await _context.ConceptosCobro.AnyAsync(c => c.Id == conceptoId);
            if (!conceptoExiste)
                throw new NotFoundException($"Concepto de cobro con ID {conceptoId} no encontrado.", code: "CONCEPTO_NO_ENCONTRADO");
        }

        private static ReglaRecargoDto MapToDto(ReglaRecargo entity)
        {
            return new ReglaRecargoDto
            {
                Id = entity.Id,
                CicloId = entity.CicloId,
                ConceptoCobroId = entity.ConceptoCobroId,
                DiasGracia = entity.DiasGracia,
                Porcentaje = entity.Porcentaje,
                Activa = entity.Activa,
                CreatedAtUtc = entity.CreatedAtUtc,
                UpdatedAtUtc = entity.UpdatedAtUtc
            };
        }
    }
}
