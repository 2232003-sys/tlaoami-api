using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class SalonesController : ControllerBase
    {
        private readonly ISalonService _salonService;

        public SalonesController(ISalonService salonService)
        {
            _salonService = salonService;
        }

        /// <summary>
        /// Obtiene todos los salones (por defecto solo activos)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalonDto>>> GetAll([FromQuery] bool? activo = true)
        {
            var salones = await _salonService.GetAllAsync(activo);
            return Ok(salones);
        }

        /// <summary>
        /// Obtiene un salón por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SalonDto>> GetById(Guid id)
        {
            var salon = await _salonService.GetByIdAsync(id);

            if (salon == null)
                return NotFound(new { code = "SALON_NO_ENCONTRADO", message = "Salón no encontrado" });

            return Ok(salon);
        }

        /// <summary>
        /// Crea un nuevo salón
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<SalonDto>> Create([FromBody] SalonCreateDto dto)
        {
            var salon = await _salonService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = salon.Id }, salon);
        }

        /// <summary>
        /// Actualiza un salón existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<SalonDto>> Update(Guid id, [FromBody] SalonUpdateDto dto)
        {
            var salon = await _salonService.UpdateAsync(id, dto);
            return Ok(salon);
        }

        /// <summary>
        /// Elimina un salón (solo si no tiene grupos asignados)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _salonService.DeleteAsync(id);
            return NoContent();
        }
    }
}
