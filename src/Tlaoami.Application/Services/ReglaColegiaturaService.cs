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
    public class ReglaColegiaturaService : IReglaColegiaturaService
    {
        private readonly TlaoamiDbContext _context;

        public ReglaColegiaturaService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReglaColegiaturaDto>> GetAllAsync(Guid? cicloId = null, Guid? grupoId = null, int? grado = null, bool? activa = null)
        {
            var query = _context.ReglasColegiatura.AsQueryable();

            if (cicloId.HasValue)
                query = query.Where(r => r.CicloId == cicloId.Value);

            if (grupoId.HasValue)
                query = query.Where(r => r.GrupoId == grupoId.Value);

            if (grado.HasValue)
                query = query.Where(r => r.Grado == grado.Value || r.Grado == null);

            if (activa.HasValue)
                query = query.Where(r => r.Activa == activa.Value);

            var reglas = await query
                .OrderBy(r => r.CicloId)
                .ThenBy(r => r.GrupoId)
                .ThenBy(r => r.Grado)
                .ThenBy(r => r.Turno)
                .ToListAsync();

            return reglas.Select(MapToDto).ToList();
        }

        public async Task<ReglaColegiaturaDto> GetByIdAsync(Guid id)
        {
            var regla = await _context.ReglasColegiatura.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de colegiatura con ID {id} no encontrada.", code: "REGLA_NO_ENCONTRADA");

            return MapToDto(regla);
        }

        public async Task<ReglaColegiaturaDto> CreateAsync(ReglaColegiaturaCreateDto dto)
        {
            ValidateCreate(dto);

            await EnsureForeignsAsync(dto.CicloId, dto.ConceptoCobroId, dto.GrupoId);

            var turno = dto.Turno?.Trim();

            var exists = await _context.ReglasColegiatura.AnyAsync(r =>
                r.CicloId == dto.CicloId &&
                r.GrupoId == dto.GrupoId &&
                r.Grado == dto.Grado &&
                r.Turno == turno &&
                r.ConceptoCobroId == dto.ConceptoCobroId);

            if (exists)
                throw new BusinessException("Ya existe una regla con la misma combinación de ciclo/grupo/grado/turno/concepto.", code: "REGLA_DUPLICADA");

            var regla = new ReglaColegiatura
            {
                Id = Guid.NewGuid(),
                CicloId = dto.CicloId,
                GrupoId = dto.GrupoId,
                Grado = dto.Grado,
                Turno = turno,
                ConceptoCobroId = dto.ConceptoCobroId,
                MontoBase = dto.MontoBase,
                DiaVencimiento = dto.DiaVencimiento,
                Activa = dto.Activa,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.ReglasColegiatura.Add(regla);
            await _context.SaveChangesAsync();

            return MapToDto(regla);
        }

        public async Task<ReglaColegiaturaDto> UpdateAsync(Guid id, ReglaColegiaturaUpdateDto dto)
        {
            var regla = await _context.ReglasColegiatura.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de colegiatura con ID {id} no encontrada.", code: "REGLA_NO_ENCONTRADA");

            if (dto.GrupoId.HasValue)
                regla.GrupoId = dto.GrupoId.Value;

            if (dto.Grado.HasValue)
            {
                ValidateGrado(dto.Grado.Value);
                regla.Grado = dto.Grado.Value;
            }

            if (dto.Turno != null)
                regla.Turno = string.IsNullOrWhiteSpace(dto.Turno) ? null : dto.Turno.Trim();

            if (dto.MontoBase.HasValue)
            {
                if (dto.MontoBase.Value <= 0)
                    throw new ValidationException("MontoBase debe ser mayor a 0.", code: "MONTO_INVALIDO");
                regla.MontoBase = dto.MontoBase.Value;
            }

            if (dto.DiaVencimiento.HasValue)
            {
                ValidateDiaVencimiento(dto.DiaVencimiento.Value);
                regla.DiaVencimiento = dto.DiaVencimiento.Value;
            }

            if (dto.Activa.HasValue)
                regla.Activa = dto.Activa.Value;

            var duplicate = await _context.ReglasColegiatura.AnyAsync(r =>
                r.Id != id &&
                r.CicloId == regla.CicloId &&
                r.GrupoId == regla.GrupoId &&
                r.Grado == regla.Grado &&
                r.Turno == regla.Turno &&
                r.ConceptoCobroId == regla.ConceptoCobroId);

            if (duplicate)
                throw new BusinessException("Ya existe una regla con la misma combinación de ciclo/grupo/grado/turno/concepto.", code: "REGLA_DUPLICADA");

            regla.UpdatedAtUtc = DateTime.UtcNow;
            _context.ReglasColegiatura.Update(regla);
            await _context.SaveChangesAsync();

            return MapToDto(regla);
        }

        public async Task InactivateAsync(Guid id)
        {
            var regla = await _context.ReglasColegiatura.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de colegiatura con ID {id} no encontrada.", code: "REGLA_NO_ENCONTRADA");

            if (!regla.Activa)
                return;

            regla.Activa = false;
            regla.UpdatedAtUtc = DateTime.UtcNow;
            _context.ReglasColegiatura.Update(regla);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var regla = await _context.ReglasColegiatura.FirstOrDefaultAsync(r => r.Id == id);
            if (regla == null)
                throw new NotFoundException($"Regla de colegiatura con ID {id} no encontrada.", code: "REGLA_NO_ENCONTRADA");

            _context.ReglasColegiatura.Remove(regla);
            await _context.SaveChangesAsync();
        }

        private static void ValidateCreate(ReglaColegiaturaCreateDto dto)
        {
            if (dto.CicloId == Guid.Empty)
                throw new ValidationException("CicloId es requerido.", code: "CICLO_REQUERIDO");
            if (dto.ConceptoCobroId == Guid.Empty)
                throw new ValidationException("ConceptoCobroId es requerido.", code: "CONCEPTO_REQUERIDO");
            if (dto.MontoBase <= 0)
                throw new ValidationException("MontoBase debe ser mayor a 0.", code: "MONTO_INVALIDO");
            ValidateDiaVencimiento(dto.DiaVencimiento);
            if (dto.Grado.HasValue)
                ValidateGrado(dto.Grado.Value);
        }

        private static void ValidateDiaVencimiento(int dia)
        {
            if (dia < 1 || dia > 28)
                throw new ValidationException("DiaVencimiento debe estar entre 1 y 28.", code: "DIA_VENCIMIENTO_INVALIDO");
        }

        private static void ValidateGrado(int grado)
        {
            if (grado < 1 || grado > 6)
                throw new ValidationException("Grado debe estar entre 1 y 6.", code: "GRADO_INVALIDO");
        }

        private async Task EnsureForeignsAsync(Guid cicloId, Guid conceptoId, Guid? grupoId)
        {
            var cicloExiste = await _context.CiclosEscolares.AnyAsync(c => c.Id == cicloId);
            if (!cicloExiste)
                throw new NotFoundException($"Ciclo escolar con ID {cicloId} no encontrado.", code: "CICLO_NO_ENCONTRADO");

            var conceptoExiste = await _context.ConceptosCobro.AnyAsync(c => c.Id == conceptoId);
            if (!conceptoExiste)
                throw new NotFoundException($"Concepto de cobro con ID {conceptoId} no encontrado.", code: "CONCEPTO_NO_ENCONTRADO");

            if (grupoId.HasValue)
            {
                var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.Id == grupoId.Value);
                if (grupo == null)
                    throw new NotFoundException($"Grupo con ID {grupoId} no encontrado.", code: "GRUPO_NO_ENCONTRADO");
                if (grupo.CicloEscolarId != cicloId)
                    throw new ValidationException("El grupo no pertenece al ciclo indicado.", code: "GRUPO_CICLO_INCONSISTENTE");
            }
        }

        private static ReglaColegiaturaDto MapToDto(ReglaColegiatura entity)
        {
            return new ReglaColegiaturaDto
            {
                Id = entity.Id,
                CicloId = entity.CicloId,
                GrupoId = entity.GrupoId,
                Grado = entity.Grado,
                Turno = entity.Turno,
                ConceptoCobroId = entity.ConceptoCobroId,
                MontoBase = entity.MontoBase,
                DiaVencimiento = entity.DiaVencimiento,
                Activa = entity.Activa,
                CreatedAtUtc = entity.CreatedAtUtc,
                UpdatedAtUtc = entity.UpdatedAtUtc
            };
        }
    }
}
