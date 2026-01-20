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
    public class ConceptosCobroService : IConceptosCobroService
    {
        private readonly TlaoamiDbContext _context;

        public ConceptosCobroService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<List<ConceptoCobroDto>> GetAllAsync(bool? activo = null)
        {
            var query = _context.ConceptosCobro.AsQueryable();

            if (activo.HasValue)
                query = query.Where(c => c.Activo == activo.Value);

            var conceptos = await query
                .OrderBy(c => c.Orden)
                .ThenBy(c => c.Clave)
                .ToListAsync();

            return conceptos.Select(MapToDto).ToList();
        }

        public async Task<ConceptoCobroDto> GetByIdAsync(Guid id)
        {
            var concepto = await _context.ConceptosCobro.FirstOrDefaultAsync(c => c.Id == id);
            if (concepto == null)
                throw new NotFoundException($"Concepto de cobro con ID {id} no encontrado.", code: "CONCEPTO_NO_ENCONTRADO");

            return MapToDto(concepto);
        }

        public async Task<ConceptoCobroDto> GetByClaveAsync(string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
                throw new ValidationException("La clave no puede estar vacía.", code: "CLAVE_REQUERIDA");

            var concepto = await _context.ConceptosCobro
                .FirstOrDefaultAsync(c => c.Clave.ToLower() == clave.ToLower());

            if (concepto == null)
                throw new NotFoundException($"Concepto de cobro con clave '{clave}' no encontrado.", code: "CONCEPTO_NO_ENCONTRADO");

            return MapToDto(concepto);
        }

        public async Task<ConceptoCobroDto> CreateAsync(ConceptoCobroCreateDto dto)
        {
            // Validar entrada
            ValidateCreateDto(dto);

            // Verificar que clave sea única (case-insensitive)
            var existe = await _context.ConceptosCobro
                .AnyAsync(c => c.Clave.ToLower() == dto.Clave.ToLower());

            if (existe)
                throw new BusinessException(
                    $"Ya existe un concepto de cobro con clave '{dto.Clave}'.",
                    code: "CLAVE_DUPLICADA");

            var concepto = new ConceptoCobro
            {
                Id = Guid.NewGuid(),
                Clave = dto.Clave.Trim().ToUpper(),
                Nombre = dto.Nombre.Trim(),
                Periodicidad = dto.Periodicidad,
                RequiereCFDI = dto.RequiereCFDI,
                Activo = dto.Activo,
                Orden = dto.Orden,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = null
            };

            _context.ConceptosCobro.Add(concepto);
            await _context.SaveChangesAsync();

            return MapToDto(concepto);
        }

        public async Task<ConceptoCobroDto> UpdateAsync(Guid id, ConceptoCobroUpdateDto dto)
        {
            var concepto = await _context.ConceptosCobro.FirstOrDefaultAsync(c => c.Id == id);
            if (concepto == null)
                throw new NotFoundException($"Concepto de cobro con ID {id} no encontrado.", code: "CONCEPTO_NO_ENCONTRADO");

            // Validar campos opcionales
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
            {
                if (dto.Nombre.Length < 3 || dto.Nombre.Length > 120)
                    throw new ValidationException("Nombre debe tener entre 3 y 120 caracteres.", code: "NOMBRE_INVALIDO");
                concepto.Nombre = dto.Nombre.Trim();
            }

            if (dto.Periodicidad.HasValue)
                concepto.Periodicidad = dto.Periodicidad;

            if (dto.RequiereCFDI.HasValue)
                concepto.RequiereCFDI = dto.RequiereCFDI.Value;

            if (dto.Activo.HasValue)
                concepto.Activo = dto.Activo.Value;

            if (dto.Orden.HasValue)
                concepto.Orden = dto.Orden.Value;

            concepto.UpdatedAtUtc = DateTime.UtcNow;

            _context.ConceptosCobro.Update(concepto);
            await _context.SaveChangesAsync();

            return MapToDto(concepto);
        }

        public async Task InactivateAsync(Guid id)
        {
            var concepto = await _context.ConceptosCobro.FirstOrDefaultAsync(c => c.Id == id);
            if (concepto == null)
                throw new NotFoundException($"Concepto de cobro con ID {id} no encontrado.", code: "CONCEPTO_NO_ENCONTRADO");

            if (!concepto.Activo)
                return; // Idempotente: si ya está inactivo, no hacer nada

            concepto.Activo = false;
            concepto.UpdatedAtUtc = DateTime.UtcNow;

            _context.ConceptosCobro.Update(concepto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var concepto = await _context.ConceptosCobro.FirstOrDefaultAsync(c => c.Id == id);
            if (concepto == null)
                throw new NotFoundException($"Concepto de cobro con ID {id} no encontrado.", code: "CONCEPTO_NO_ENCONTRADO");

            // TODO: Verificar que no esté referenciado por ReglaCobroPorCiclo
            // Por ahora, permitir eliminación (cuando ReglaCobroPorCiclo esté implementado, agregar validación)

            _context.ConceptosCobro.Remove(concepto);
            await _context.SaveChangesAsync();
        }

        // === Privados ===

        private void ValidateCreateDto(ConceptoCobroCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Clave))
                throw new ValidationException("Clave es requerida.", code: "CLAVE_REQUERIDA");

            if (dto.Clave.Length < 3 || dto.Clave.Length > 30)
                throw new ValidationException("Clave debe tener entre 3 y 30 caracteres.", code: "CLAVE_LONGITUD_INVALIDA");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ValidationException("Nombre es requerido.", code: "NOMBRE_REQUERIDO");

            if (dto.Nombre.Length < 3 || dto.Nombre.Length > 120)
                throw new ValidationException("Nombre debe tener entre 3 y 120 caracteres.", code: "NOMBRE_LONGITUD_INVALIDA");
        }

        private static ConceptoCobroDto MapToDto(ConceptoCobro entity)
        {
            return new ConceptoCobroDto
            {
                Id = entity.Id,
                Clave = entity.Clave,
                Nombre = entity.Nombre,
                Periodicidad = entity.Periodicidad,
                RequiereCFDI = entity.RequiereCFDI,
                Activo = entity.Activo,
                Orden = entity.Orden,
                CreatedAtUtc = entity.CreatedAtUtc,
                UpdatedAtUtc = entity.UpdatedAtUtc
            };
        }
    }
}
