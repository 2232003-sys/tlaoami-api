using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        public GruposController(IGrupoService grupoService)
        {
            _grupoService = grupoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GrupoDto>>> GetGrupos()
        {
            var grupos = await _grupoService.GetAllGruposAsync();
            return Ok(grupos);
        }

        [HttpGet("ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<GrupoDto>>> GetGruposPorCiclo(Guid cicloId)
        {
            var grupos = await _grupoService.GetGruposPorCicloAsync(cicloId);
            return Ok(grupos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GrupoDto>> GetGrupo(Guid id)
        {
            var grupo = await _grupoService.GetGrupoByIdAsync(id);
            if (grupo == null)
                return NotFound();
            return Ok(grupo);
        }

        [HttpPost]
        public async Task<ActionResult<GrupoDto>> CreateGrupo([FromBody] GrupoCreateDto dto)
        {
            try
            {
                var grupo = await _grupoService.CreateGrupoAsync(dto);
                return CreatedAtAction(nameof(GetGrupo), new { id = grupo.Id }, grupo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GrupoDto>> UpdateGrupo(Guid id, [FromBody] GrupoCreateDto dto)
        {
            try
            {
                var grupo = await _grupoService.UpdateGrupoAsync(id, dto);
                return Ok(grupo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGrupo(Guid id)
        {
            var result = await _grupoService.DeleteGrupoAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [Authorize]
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
    }
}
