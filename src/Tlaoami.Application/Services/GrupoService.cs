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
                .Include(g => g.DocenteTitular)
                .ToListAsync();
            return grupos.Select(MapToDto);
        }

        public async Task<IEnumerable<GrupoDto>> GetGruposPorCicloAsync(Guid cicloId)
        {
            var grupos = await _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .Include(g => g.DocenteTitular)
                .Where(g => g.CicloEscolarId == cicloId)
                .ToListAsync();
            return grupos.Select(MapToDto);
        }

        public async Task<GrupoDto?> GetGrupoByIdAsync(Guid id)
        {
            var grupo = await _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .Include(g => g.DocenteTitular)
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

        public async Task<GrupoDto> AssignDocenteTitularAsync(Guid grupoId, Guid? docenteTitularId)
        {
            // Validar que el grupo existe
            var grupo = await _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .Include(g => g.DocenteTitular)
                .FirstOrDefaultAsync(g => g.Id == grupoId);

            if (grupo == null)
                throw new Tlaoami.Application.Exceptions.NotFoundException("Grupo no encontrado", code: "GRUPO_NO_ENCONTRADO");

            // Si se está asignando un docente (no null), validar que existe
            if (docenteTitularId.HasValue)
            {
                var docente = await _context.Users.FindAsync(docenteTitularId.Value);
                if (docente == null)
                    throw new Tlaoami.Application.Exceptions.NotFoundException("Usuario no encontrado", code: "DOCENTE_NO_ENCONTRADO");

                // TODO: Validar rol cuando RBAC esté completo
                // if (docente.Role != "Docente")
                //     throw new Tlaoami.Application.Exceptions.BusinessException("El usuario no tiene rol de docente", code: "USUARIO_NO_ES_DOCENTE");
            }

            // Idempotencia: si ya está asignado el mismo docente, no hacer nada
            if (grupo.DocenteTitularId == docenteTitularId)
            {
                return MapToDto(grupo);
            }

            // Asignar o quitar docente
            grupo.DocenteTitularId = docenteTitularId;
            await _context.SaveChangesAsync();

            // Recargar navegación si se asignó un docente
            if (docenteTitularId.HasValue)
            {
                await _context.Entry(grupo).Reference(g => g.DocenteTitular).LoadAsync();
            }

            return MapToDto(grupo);
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
                SalonCodigo = grupo.Salon?.Codigo,
                DocenteTitularId = grupo.DocenteTitularId,
                DocenteTitularNombre = grupo.DocenteTitular?.Username
            };
        }
    }
}
