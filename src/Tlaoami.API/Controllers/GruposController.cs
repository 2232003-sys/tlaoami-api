using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Exceptions;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GruposController : ControllerBase
    {
        private readonly IGrupoService _grupoService;
        private readonly TlaoamiDbContext _db;

        public GruposController(IGrupoService grupoService, TlaoamiDbContext db)
        {
            _grupoService = grupoService;
            _db = db;
        }

        /// <summary>
        /// Obtener todos los grupos (por defecto solo activos)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GrupoDto>>> GetGrupos([FromQuery] bool incluirInactivos = false)
        {
            var grupos = await _grupoService.GetAllGruposAsync(incluirInactivos);
            return Ok(grupos);
        }

        /// <summary>
        /// Obtener grupos por ciclo escolar
        /// </summary>
        [HttpGet("ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<GrupoDto>>> GetGruposPorCiclo(Guid cicloId, [FromQuery] bool incluirInactivos = false)
        {
            var grupos = await _grupoService.GetGruposPorCicloAsync(cicloId, incluirInactivos);
            return Ok(grupos);
        }

        /// <summary>
        /// Obtener grupo por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GrupoDto>> GetGrupo(Guid id)
        {
            var grupo = await _grupoService.GetGrupoByIdAsync(id);
            if (grupo == null)
                return NotFound(new { error = "Grupo no encontrado", code = "GRUPO_NO_ENCONTRADO" });
            return Ok(grupo);
        }

        /// <summary>
        /// Crear nuevo grupo
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GrupoDto>> CreateGrupo([FromBody] GrupoCreateDto dto)
        {
            try
            {
                var grupo = await _grupoService.CreateGrupoAsync(dto);
                return CreatedAtAction(nameof(GetGrupo), new { id = grupo.Id }, grupo);
            }
            catch (BusinessException ex) when (ex.Code == "SALON_CAPACIDAD_INSUFICIENTE")
            {
                return Conflict(ex.Message);
            }
            catch (BusinessException ex)
            {
                return Conflict(new { error = ex.Message, code = ex.Code });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar grupo existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<GrupoDto>> UpdateGrupo(Guid id, [FromBody] GrupoUpdateDto dto)
        {
            try
            {
                var grupo = await _grupoService.UpdateGrupoAsync(id, dto);
                return Ok(grupo);
            }
            catch (BusinessException ex) when (ex.Code == "SALON_CAPACIDAD_INSUFICIENTE")
            {
                return Conflict(ex.Message);
            }
            catch (BusinessException ex)
            {
                return Conflict(new { error = ex.Message, code = ex.Code });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar grupo (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGrupo(Guid id)
        {
            var result = await _grupoService.DeleteGrupoAsync(id);
            if (!result)
                return NotFound(new { error = "Grupo no encontrado", code = "GRUPO_NO_ENCONTRADO" });
            return NoContent();
        }

        /// <summary>
        /// Asignar o quitar docente titular
        /// </summary>
        [HttpPut("{id}/docente-titular")]
        public async Task<ActionResult<GrupoDto>> AssignDocenteTitular(Guid id, [FromBody] GrupoUpdateDocenteTitularDto dto)
        {
            try
            {
                var grupo = await _grupoService.AssignDocenteTitularAsync(id, dto.DocenteTitularId);
                return Ok(grupo);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
            catch (BusinessException ex)
            {
                return Conflict(new { error = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener alumnos asignados al grupo (grupo actual)
        /// </summary>
        [HttpGet("{id}/alumnos")]
        public async Task<ActionResult<IEnumerable<AlumnoEnGrupoDto>>> GetAlumnosPorGrupo(Guid id)
        {
            try
            {
                var alumnos = await _grupoService.GetAlumnosPorGrupoAsync(id);
                return Ok(alumnos);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Asignar salón a un grupo
        /// </summary>
        [HttpPut("{grupoId}/salon/{salonId}")]
        public async Task<IActionResult> AsignarSalon(Guid grupoId, Guid salonId)
        {
            var grupo = await _db.Grupos.FirstOrDefaultAsync(g => g.Id == grupoId);
            if (grupo is null) return NotFound(new { error = "Grupo no encontrado.", code = "GRUPO_NO_ENCONTRADO" });

            var salon = await _db.Salones.AsNoTracking().FirstOrDefaultAsync(s => s.Id == salonId);
            if (salon is null) return NotFound(new { error = "Salón no encontrado.", code = "SALON_NO_ENCONTRADO" });
            if (!salon.Activo) return BadRequest(new { error = "El salón está inactivo.", code = "SALON_INACTIVO" });

            grupo.SalonId = salonId;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Quitar salón asignado a un grupo
        /// </summary>
        [HttpDelete("{grupoId}/salon")]
        public async Task<IActionResult> QuitarSalon(Guid grupoId)
        {
            var grupo = await _db.Grupos.FirstOrDefaultAsync(g => g.Id == grupoId);
            if (grupo is null) return NotFound(new { error = "Grupo no encontrado.", code = "GRUPO_NO_ENCONTRADO" });

            grupo.SalonId = null;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
