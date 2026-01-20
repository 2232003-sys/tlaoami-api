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
    public class ReglasCobroService : IReglasCobroService
    {
        private readonly TlaoamiDbContext _context;

        public ReglasCobroService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReglaCobroDto>> GetAllAsync(Guid? cicloId = null, int? grado = null, bool? activa = null)
        {
            var query = _context.ReglasCobro.AsQueryable();

            if (cicloId.HasValue)
                query = query.Where(r => r.CicloId == cicloId.Value);

            if (grado.HasValue)
                query = query.Where(r => r.Grado == grado.Value || r.Grado == null);

            if (activa.HasValue)
                query = query.Where(r => r.Activa == activa.Value);

            var reglas = await query
                .OrderBy(r => r.CicloId)
                .ThenBy(r => r.Grado)
                .ThenBy(r => r.Turno)
                .ThenBy(r => r.ConceptoCobroId)
                .ToListAsync();

            return reglas.Select(MapToDto).ToList();
        }

        public async Task<ReglaCobroDto> GetByIdAsync(Guid id)
        {
            var regla = await _context.ReglasCobro.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de cobro con ID {id} no encontrada.", code: "REGLA_NO_ENCONTRADA");

            return MapToDto(regla);
        }

        public async Task<List<ReglaCobroDto>> GetByCicloAsync(Guid cicloId, bool? activa = null)
        {
            return await GetAllAsync(cicloId: cicloId, activa: activa);
        }

        public async Task<ReglaCobroDto> CreateAsync(ReglaCobroCreateDto dto)
        {
            // Validar entrada
            ValidateCreateDto(dto);

            // Verificar que ciclo existe
            var cicloExiste = await _context.CiclosEscolares.AnyAsync(c => c.Id == dto.CicloId);
            if (!cicloExiste)
                throw new NotFoundException($"Ciclo escolar con ID {dto.CicloId} no encontrado.", code: "CICLO_NO_ENCONTRADO");

            // Verificar que concepto existe
            var conceptoExiste = await _context.ConceptosCobro.AnyAsync(c => c.Id == dto.ConceptoCobroId);
            if (!conceptoExiste)
                throw new NotFoundException($"Concepto de cobro con ID {dto.ConceptoCobroId} no encontrado.", code: "CONCEPTO_NO_ENCONTRADO");

            // Verificar no existe regla duplicada (lógicamente)
            var duplicada = await _context.ReglasCobro.AnyAsync(r =>
                r.CicloId == dto.CicloId &&
                r.Grado == dto.Grado &&
                r.Turno == dto.Turno &&
                r.ConceptoCobroId == dto.ConceptoCobroId &&
                r.TipoGeneracion == dto.TipoGeneracion
            );

            if (duplicada)
                throw new BusinessException(
                    "Ya existe una regla con la misma combinación de ciclo, grado, turno, concepto y tipo de generación.",
                    code: "REGLA_DUPLICADA");

            var regla = new ReglaCobroPorCiclo
            {
                Id = Guid.NewGuid(),
                CicloId = dto.CicloId,
                Grado = dto.Grado,
                Turno = dto.Turno?.Trim(),
                ConceptoCobroId = dto.ConceptoCobroId,
                TipoGeneracion = dto.TipoGeneracion,
                DiaCorte = dto.DiaCorte,
                MontoBase = dto.MontoBase,
                Activa = dto.Activa,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = null
            };

            _context.ReglasCobro.Add(regla);
            await _context.SaveChangesAsync();

            return MapToDto(regla);
        }

        public async Task<ReglaCobroDto> UpdateAsync(Guid id, ReglaCobroUpdateDto dto)
        {
            var regla = await _context.ReglasCobro.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de cobro con ID {id} no encontrada.", code: "REGLA_NO_ENCONTRADA");

            // Actualizar campos opcionales
            if (dto.Grado.HasValue)
                ValidateGrado(dto.Grado.Value);
            else if (dto.Grado == null && regla.Grado.HasValue)
                regla.Grado = null; // Permitir null explícitamente

            if (!string.IsNullOrWhiteSpace(dto.Turno))
                regla.Turno = dto.Turno.Trim();
            else if (dto.Turno == "")
                regla.Turno = null;

            if (dto.TipoGeneracion.HasValue)
                regla.TipoGeneracion = dto.TipoGeneracion.Value;

            if (dto.DiaCorte.HasValue)
                ValidateDiaCorte(dto.DiaCorte.Value);

            if (dto.MontoBase.HasValue)
            {
                if (dto.MontoBase.Value <= 0)
                    throw new ValidationException("MontoBase debe ser mayor a 0.", code: "MONTO_INVALIDO");
                regla.MontoBase = dto.MontoBase.Value;
            }

            if (dto.Activa.HasValue)
                regla.Activa = dto.Activa.Value;

            regla.UpdatedAtUtc = DateTime.UtcNow;

            _context.ReglasCobro.Update(regla);
            await _context.SaveChangesAsync();

            return MapToDto(regla);
        }

        public async Task InactivateAsync(Guid id)
        {
            var regla = await _context.ReglasCobro.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de cobro con ID {id} no encontrada.", code: "REGLA_NO_ENCONTRADA");

            if (!regla.Activa)
                return; // Idempotente

            regla.Activa = false;
            regla.UpdatedAtUtc = DateTime.UtcNow;

            _context.ReglasCobro.Update(regla);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var regla = await _context.ReglasCobro.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de cobro con ID {id} no encontrada.", code: "REGLA_NO_ENCONTRADA");

            _context.ReglasCobro.Remove(regla);
            await _context.SaveChangesAsync();
        }

        // === Privados ===

        private void ValidateCreateDto(ReglaCobroCreateDto dto)
        {
            if (dto.CicloId == Guid.Empty)
                throw new ValidationException("CicloId es requerido.", code: "CICLO_REQUERIDO");

            if (dto.ConceptoCobroId == Guid.Empty)
                throw new ValidationException("ConceptoCobroId es requerido.", code: "CONCEPTO_REQUERIDO");

            if (dto.MontoBase <= 0)
                throw new ValidationException("MontoBase debe ser mayor a 0.", code: "MONTO_INVALIDO");

            if (dto.Grado.HasValue)
                ValidateGrado(dto.Grado.Value);

            if (dto.DiaCorte.HasValue)
                ValidateDiaCorte(dto.DiaCorte.Value);
        }

        private static void ValidateGrado(int grado)
        {
            if (grado < 1 || grado > 6)
                throw new ValidationException("Grado debe estar entre 1 y 6.", code: "GRADO_INVALIDO");
        }

        private static void ValidateDiaCorte(int diaCorte)
        {
            if (diaCorte < 1 || diaCorte > 28)
                throw new ValidationException("DiaCorte debe estar entre 1 y 28.", code: "DIA_CORTE_INVALIDO");
        }

        private static ReglaCobroDto MapToDto(ReglaCobroPorCiclo entity)
        {
            return new ReglaCobroDto
            {
                Id = entity.Id,
                CicloId = entity.CicloId,
                Grado = entity.Grado,
                Turno = entity.Turno,
                ConceptoCobroId = entity.ConceptoCobroId,
                TipoGeneracion = entity.TipoGeneracion,
                DiaCorte = entity.DiaCorte,
                MontoBase = entity.MontoBase,
                Activa = entity.Activa,
                CreatedAtUtc = entity.CreatedAtUtc,
                UpdatedAtUtc = entity.UpdatedAtUtc
            };
        }
    }
}
