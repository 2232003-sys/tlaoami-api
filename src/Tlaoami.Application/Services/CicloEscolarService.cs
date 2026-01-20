using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class CicloEscolarService : ICicloEscolarService
    {
        private readonly TlaoamiDbContext _context;

        public CicloEscolarService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CicloEscolarDto>> GetAllCiclosAsync()
        {
            var ciclos = await _context.CiclosEscolares.ToListAsync();
            return ciclos.Select(MapToDto);
        }

        public async Task<CicloEscolarDto?> GetCicloByIdAsync(Guid id)
        {
            var ciclo = await _context.CiclosEscolares.FindAsync(id);
            return ciclo != null ? MapToDto(ciclo) : null;
        }

        public async Task<CicloEscolarDto?> GetCicloActivoAsync()
        {
            var ciclo = await _context.CiclosEscolares
                .FirstOrDefaultAsync(c => c.Activo);
            return ciclo != null ? MapToDto(ciclo) : null;
        }

        public async Task<CicloEscolarDto> CreateCicloAsync(CicloEscolarCreateDto dto)
        {
            var ciclo = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Activo = true
            };

            _context.CiclosEscolares.Add(ciclo);
            await _context.SaveChangesAsync();

            return MapToDto(ciclo);
        }

        public async Task<CicloEscolarDto> UpdateCicloAsync(Guid id, CicloEscolarCreateDto dto)
        {
            var ciclo = await _context.CiclosEscolares.FindAsync(id);
            if (ciclo == null)
                throw new Tlaoami.Application.Exceptions.NotFoundException("Ciclo escolar no encontrado", code: "CICLO_NO_ENCONTRADO");

            if (!string.IsNullOrEmpty(dto.Nombre))
                ciclo.Nombre = dto.Nombre;
            
            ciclo.FechaInicio = dto.FechaInicio;
            ciclo.FechaFin = dto.FechaFin;

            await _context.SaveChangesAsync();

            return MapToDto(ciclo);
        }

        public async Task<bool> DeleteCicloAsync(Guid id)
        {
            var ciclo = await _context.CiclosEscolares.FindAsync(id);
            if (ciclo == null)
                return false;

            _context.CiclosEscolares.Remove(ciclo);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetCicloActivoAsync(Guid id)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var ciclo = await _context.CiclosEscolares.FindAsync(id);
            if (ciclo == null)
                return false;

            // Desactivar todos los ciclos
            var todos = await _context.CiclosEscolares.ToListAsync();
            foreach (var c in todos)
            {
                c.Activo = false;
            }

            // Activar el ciclo seleccionado
            ciclo.Activo = true;
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }

        private static CicloEscolarDto MapToDto(CicloEscolar ciclo)
        {
            return new CicloEscolarDto
            {
                Id = ciclo.Id,
                Nombre = ciclo.Nombre,
                FechaInicio = ciclo.FechaInicio,
                FechaFin = ciclo.FechaFin,
                Activo = ciclo.Activo
            };
        }
    }
}
