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
    public class GrupoService : IGrupoService
    {
        private readonly TlaoamiDbContext _context;

        public GrupoService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GrupoDto>> GetAllGruposAsync()
        {
            var grupos = await _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .ToListAsync();
            return grupos.Select(MapToDto);
        }

        public async Task<IEnumerable<GrupoDto>> GetGruposPorCicloAsync(Guid cicloId)
        {
            var grupos = await _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .Where(g => g.CicloEscolarId == cicloId)
                .ToListAsync();
            return grupos.Select(MapToDto);
        }

        public async Task<GrupoDto?> GetGrupoByIdAsync(Guid id)
        {
            var grupo = await _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .FirstOrDefaultAsync(g => g.Id == id);
            return grupo != null ? MapToDto(grupo) : null;
        }

        public async Task<GrupoDto> CreateGrupoAsync(GrupoCreateDto dto)
        {
            var grupo = new Grupo
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Grado = dto.Grado,
                Turno = dto.Turno,
                Capacidad = dto.Capacidad,
                CicloEscolarId = dto.CicloEscolarId
            };

            _context.Grupos.Add(grupo);
            await _context.SaveChangesAsync();

            // Reload to get related data
            await _context.Entry(grupo).Reference(g => g.CicloEscolar).LoadAsync();

            return MapToDto(grupo);
        }

        public async Task<GrupoDto> UpdateGrupoAsync(Guid id, GrupoCreateDto dto)
        {
            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null)
                throw new Tlaoami.Application.Exceptions.NotFoundException("Grupo no encontrado", code: "GRUPO_NO_ENCONTRADO");

            grupo.Nombre = dto.Nombre;
            grupo.Grado = dto.Grado;
            grupo.Turno = dto.Turno;
            grupo.Capacidad = dto.Capacidad;
            grupo.CicloEscolarId = dto.CicloEscolarId;

            await _context.SaveChangesAsync();

            // Reload to get related data
            await _context.Entry(grupo)
                .Reference(g => g.CicloEscolar)
                .LoadAsync();

            return MapToDto(grupo);
        }

        public async Task<bool> DeleteGrupoAsync(Guid id)
        {
            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null)
                return false;

            _context.Grupos.Remove(grupo);
            await _context.SaveChangesAsync();

            return true;
        }

        private static GrupoDto MapToDto(Grupo grupo)
        {
            return new GrupoDto
            {
                Id = grupo.Id,
                Nombre = grupo.Nombre,
                Grado = grupo.Grado,
                Turno = grupo.Turno,
                Capacidad = grupo.Capacidad,
                CicloEscolarId = grupo.CicloEscolarId,
                CicloNombre = grupo.CicloEscolar?.Nombre,
                SalonId = grupo.SalonId,
                SalonCodigo = grupo.Salon?.Codigo
            };
        }
    }
}
