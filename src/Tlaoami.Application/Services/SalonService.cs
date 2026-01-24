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
    public class SalonService : ISalonService
    {
        private readonly TlaoamiDbContext _context;

        public SalonService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<SalonDto> CreateAsync(SalonCreateDto dto)
        {
            // Validar código único
            var codigoExiste = await _context.Salones
                .AnyAsync(s => s.Codigo == dto.Codigo);

            if (codigoExiste)
                throw new BusinessException(
                    $"Ya existe un salón con código '{dto.Codigo}'",
                    code: "SALON_CODIGO_DUPLICADO");

            var salon = new Salon
            {
                Id = Guid.NewGuid(),
                Codigo = dto.Codigo.Trim(),
                Nombre = dto.Nombre.Trim(),
                Capacidad = dto.Capacidad,
                Activo = true,              // 👈 CLAVE: forzar explícitamente
                CreatedAt = DateTime.UtcNow
            };

            _context.Salones.Add(salon);
            await _context.SaveChangesAsync();

            return MapToDto(salon);
        }

        public async Task<SalonDto?> GetByIdAsync(Guid id)
        {
            var salon = await _context.Salones.FindAsync(id);
            return salon != null ? MapToDto(salon) : null;
        }

        public async Task<IEnumerable<SalonDto>> GetAllAsync(bool? activo = null)
        {
            var query = _context.Salones.AsQueryable();

            if (activo.HasValue)
                query = query.Where(s => s.Activo == activo.Value);

            var salones = await query
                .OrderBy(s => s.Codigo)
                .ToListAsync();

            return salones.Select(MapToDto);
        }

        public async Task<SalonDto> UpdateAsync(Guid id, SalonUpdateDto dto)
        {
            var salon = await _context.Salones.FindAsync(id);

            if (salon == null)
                throw new NotFoundException("Salón no encontrado", code: "SALON_NO_ENCONTRADO");

            // Validar código único si se cambia
            if (!string.IsNullOrEmpty(dto.Codigo) && dto.Codigo != salon.Codigo)
            {
                var codigoExiste = await _context.Salones
                    .AnyAsync(s => s.Codigo == dto.Codigo && s.Id != id);

                if (codigoExiste)
                    throw new BusinessException(
                        $"Ya existe otro salón con código '{dto.Codigo}'",
                        code: "SALON_CODIGO_DUPLICADO");

                salon.Codigo = dto.Codigo;
            }

            if (!string.IsNullOrEmpty(dto.Nombre))
                salon.Nombre = dto.Nombre;

            if (dto.Capacidad.HasValue)
                salon.Capacidad = dto.Capacidad.Value;

            if (dto.Activo.HasValue)
                salon.Activo = dto.Activo.Value;

            salon.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(salon);
        }

        public async Task DeleteAsync(Guid id)
        {
            var salon = await _context.Salones.FindAsync(id);

            if (salon == null)
                throw new NotFoundException("Salón no encontrado", code: "SALON_NO_ENCONTRADO");

            // Verificar que no haya grupos activos asignados
            var gruposActivosConSalon = await _context.Grupos
                .AnyAsync(g => g.SalonId == id);

            if (gruposActivosConSalon)
                throw new BusinessException(
                    "No se puede inactivar el salón porque tiene grupos asignados",
                    code: "SALON_EN_USO");

            // Soft delete: solo marcar como inactivo
            salon.Activo = false;
            salon.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private static SalonDto MapToDto(Salon salon)
        {
            return new SalonDto
            {
                Id = salon.Id,
                Codigo = salon.Codigo,
                Nombre = salon.Nombre,
                Capacidad = salon.Capacidad,
                Activo = salon.Activo,
                CreatedAt = salon.CreatedAt,
                UpdatedAt = salon.UpdatedAt
            };
        }
    }
}
