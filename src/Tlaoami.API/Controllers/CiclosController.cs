using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CiclosController : ControllerBase
    {
        private readonly ICicloEscolarService _cicloService;

        public CiclosController(ICicloEscolarService cicloService)
        {
            _cicloService = cicloService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CicloEscolarDto>>> GetCiclos()
        {
            var ciclos = await _cicloService.GetAllCiclosAsync();
            return Ok(ciclos);
        }

        [HttpGet("activo")]
        public async Task<ActionResult<CicloEscolarDto>> GetCicloActivo()
        {
            var ciclo = await _cicloService.GetCicloActivoAsync();
            if (ciclo == null)
                return NotFound();
            return Ok(ciclo);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CicloEscolarDto>> GetCiclo(Guid id)
        {
            var ciclo = await _cicloService.GetCicloByIdAsync(id);
            if (ciclo == null)
                return NotFound();
            return Ok(ciclo);
        }

        [HttpPost]
        public async Task<ActionResult<CicloEscolarDto>> CreateCiclo([FromBody] CicloEscolarCreateDto dto)
        {
            try
            {
                var ciclo = await _cicloService.CreateCicloAsync(dto);
                return CreatedAtAction(nameof(GetCiclo), new { id = ciclo.Id }, ciclo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CicloEscolarDto>> UpdateCiclo(Guid id, [FromBody] CicloEscolarCreateDto dto)
        {
            try
            {
                var ciclo = await _cicloService.UpdateCicloAsync(id, dto);
                return Ok(ciclo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCiclo(Guid id)
        {
            var result = await _cicloService.DeleteCicloAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/activar")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> ActivarCiclo(Guid id)
        {
            var result = await _cicloService.SetCicloActivoAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}
