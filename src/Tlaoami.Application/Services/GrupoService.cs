using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
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

        public async Task<IEnumerable<GrupoDto>> GetAllGruposAsync(bool incluirInactivos = false)
        {
            var query = _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .Include(g => g.DocenteTitular)
                .Include(g => g.Alumnos)
                .AsQueryable();

            if (!incluirInactivos)
                query = query.Where(g => g.Activo);

            var grupos = await query.ToListAsync();
            return grupos.Select(MapToDto);
        }

        public async Task<IEnumerable<GrupoDto>> GetGruposPorCicloAsync(Guid cicloId, bool incluirInactivos = false)
        {
            var query = _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .Include(g => g.DocenteTitular)
                .Include(g => g.Alumnos)
                .Where(g => g.CicloEscolarId == cicloId);

            if (!incluirInactivos)
                query = query.Where(g => g.Activo);

            var grupos = await query.ToListAsync();
            return grupos.Select(MapToDto);
        }

        public async Task<GrupoDto?> GetGrupoByIdAsync(Guid id)
        {
            var grupo = await _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .Include(g => g.DocenteTitular)
                .Include(g => g.Alumnos)
                .FirstOrDefaultAsync(g => g.Id == id);
            return grupo != null ? MapToDto(grupo) : null;
        }

        public async Task<GrupoDto> CreateGrupoAsync(GrupoCreateDto dto)
        {
            // Validar Capacidad > 0
            if (dto.Capacidad.HasValue && dto.Capacidad.Value <= 0)
                throw new BusinessException("La capacidad debe ser mayor a 0", code: "CAPACIDAD_INVALIDA");

            // Validar Código único
            var codigoExiste = await _context.Grupos.AnyAsync(g => g.Codigo == dto.Codigo);
            if (codigoExiste)
                throw new BusinessException($"Ya existe un grupo con el código '{dto.Codigo}'", code: "CODIGO_DUPLICADO");

            // Validar CicloId válido
            var cicloExists = await _context.CiclosEscolares.AnyAsync(c => c.Id == dto.CicloEscolarId);
            if (!cicloExists)
                throw new NotFoundException("Ciclo escolar no encontrado", code: "CICLO_NO_ENCONTRADO");

            var grupo = new Grupo
            {
                Id = Guid.NewGuid(),
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                Grado = dto.Grado,
                Seccion = dto.Seccion,
                Turno = dto.Turno,
                Capacidad = dto.Capacidad,
                Activo = true,
                CicloEscolarId = dto.CicloEscolarId,
                SalonId = dto.SalonId,
                DocenteTitularId = dto.DocenteTitularId
            };

            _context.Grupos.Add(grupo);
            await _context.SaveChangesAsync();

            // Reload to get related data
            await _context.Entry(grupo).Reference(g => g.CicloEscolar).LoadAsync();
            await _context.Entry(grupo).Collection(g => g.Alumnos).LoadAsync();

            return MapToDto(grupo);
        }

        public async Task<GrupoDto> UpdateGrupoAsync(Guid id, GrupoUpdateDto dto)
        {
            var grupo = await _context.Grupos
                .Include(g => g.Alumnos)
                .FirstOrDefaultAsync(g => g.Id == id);
            
            if (grupo == null)
                throw new NotFoundException("Grupo no encontrado", code: "GRUPO_NO_ENCONTRADO");

            // Validar Código único si se está cambiando
            if (dto.Codigo != null && dto.Codigo != grupo.Codigo)
            {
                var codigoExiste = await _context.Grupos.AnyAsync(g => g.Codigo == dto.Codigo && g.Id != id);
                if (codigoExiste)
                    throw new BusinessException($"Ya existe un grupo con el código '{dto.Codigo}'", code: "CODIGO_DUPLICADO");
                grupo.Codigo = dto.Codigo;
            }

            // Validar Capacidad > 0
            if (dto.Capacidad.HasValue && dto.Capacidad.Value <= 0)
                throw new BusinessException("La capacidad debe ser mayor a 0", code: "CAPACIDAD_INVALIDA");

            // Validar CicloId válido si se está cambiando
            if (dto.CicloEscolarId.HasValue && dto.CicloEscolarId.Value != grupo.CicloEscolarId)
            {
                var cicloExists = await _context.CiclosEscolares.AnyAsync(c => c.Id == dto.CicloEscolarId.Value);
                if (!cicloExists)
                    throw new NotFoundException("Ciclo escolar no encontrado", code: "CICLO_NO_ENCONTRADO");
                grupo.CicloEscolarId = dto.CicloEscolarId.Value;
            }

            if (dto.Nombre != null) grupo.Nombre = dto.Nombre;
            if (dto.Grado.HasValue) grupo.Grado = dto.Grado.Value;
            if (dto.Seccion != null) grupo.Seccion = dto.Seccion;
            if (dto.Turno != null) grupo.Turno = dto.Turno;
            if (dto.Capacidad.HasValue) grupo.Capacidad = dto.Capacidad;
            if (dto.Activo.HasValue) grupo.Activo = dto.Activo.Value;
            grupo.SalonId = dto.SalonId; // permite setear null
            if (dto.DocenteTitularId.HasValue) grupo.DocenteTitularId = dto.DocenteTitularId;

            await _context.SaveChangesAsync();

            // Reload to get related data
            await _context.Entry(grupo).Reference(g => g.CicloEscolar).LoadAsync();
            await _context.Entry(grupo).Reference(g => g.Salon).LoadAsync();
            await _context.Entry(grupo).Reference(g => g.DocenteTitular).LoadAsync();

            return MapToDto(grupo);
        }

        public async Task<bool> DeleteGrupoAsync(Guid id)
        {
            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null)
                return false;

            // Soft delete
            grupo.Activo = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<GrupoDto> AssignDocenteTitularAsync(Guid grupoId, Guid? docenteTitularId)
        {
            var grupo = await _context.Grupos
                .Include(g => g.CicloEscolar)
                .Include(g => g.Salon)
                .Include(g => g.DocenteTitular)
                .Include(g => g.Alumnos)
                .FirstOrDefaultAsync(g => g.Id == grupoId);

            if (grupo == null)
                throw new NotFoundException("Grupo no encontrado", code: "GRUPO_NO_ENCONTRADO");

            // Si se está asignando un docente (no null), validar que existe
            if (docenteTitularId.HasValue)
            {
                var docente = await _context.Users.FindAsync(docenteTitularId.Value);
                if (docente == null)
                    throw new NotFoundException("Usuario no encontrado", code: "DOCENTE_NO_ENCONTRADO");
            }

            // Idempotencia: si ya está asignado el mismo docente, no hacer nada
            if (grupo.DocenteTitularId == docenteTitularId)
            {
                return MapToDto(grupo);
            }

            grupo.DocenteTitularId = docenteTitularId;
            await _context.SaveChangesAsync();

            if (docenteTitularId.HasValue)
            {
                await _context.Entry(grupo).Reference(g => g.DocenteTitular).LoadAsync();
            }

            return MapToDto(grupo);
        }

        public async Task<IEnumerable<AlumnoEnGrupoDto>> GetAlumnosPorGrupoAsync(Guid grupoId)
        {
            var grupo = await _context.Grupos
                .Include(g => g.Alumnos)
                    .ThenInclude(ag => ag.Alumno)
                .FirstOrDefaultAsync(g => g.Id == grupoId);

            if (grupo == null)
                throw new NotFoundException("Grupo no encontrado", code: "GRUPO_NO_ENCONTRADO");

            // Filtrar solo asignaciones activas (grupo actual)
            var alumnosActivos = grupo.Alumnos
                .Where(ag => ag.FechaFin == null || ag.FechaFin > DateTime.UtcNow)
                .Select(ag => new AlumnoEnGrupoDto
                {
                    AlumnoId = ag.AlumnoId,
                    Matricula = ag.Alumno?.Matricula ?? "",
                    Nombre = ag.Alumno?.Nombre ?? "",
                    Apellido = ag.Alumno?.Apellido ?? "",
                    FechaAsignacion = ag.FechaInicio
                });

            return alumnosActivos;
        }

        private static GrupoDto MapToDto(Grupo grupo)
        {
            return new GrupoDto
            {
                Id = grupo.Id,
                Codigo = grupo.Codigo,
                Nombre = grupo.Nombre,
                Grado = grupo.Grado,
                Seccion = grupo.Seccion,
                Turno = grupo.Turno,
                Capacidad = grupo.Capacidad,
                Activo = grupo.Activo,
                CicloEscolarId = grupo.CicloEscolarId,
                CicloNombre = grupo.CicloEscolar?.Nombre,
                SalonId = grupo.SalonId,
                SalonCodigo = grupo.Salon?.Codigo,
                DocenteTitularId = grupo.DocenteTitularId,
                DocenteTitularNombre = grupo.DocenteTitular?.Username,
                AlumnosInscritos = grupo.Alumnos?.Count(ag => ag.FechaFin == null || ag.FechaFin > DateTime.UtcNow) ?? 0
            };
        }
    }
}
