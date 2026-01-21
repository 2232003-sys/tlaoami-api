using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class RecargosReglasController : ControllerBase
    {
        private readonly IReglaRecargoService _service;

        public RecargosReglasController(IReglaRecargoService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<List<ReglaRecargoDto>>> GetAll(
            [FromQuery] Guid? cicloId = null,
            [FromQuery] bool? activa = null)
        {
            var reglas = await _service.GetAllAsync(cicloId, activa);
            return Ok(reglas);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<ReglaRecargoDto>> GetById(Guid id)
        {
            var regla = await _service.GetByIdAsync(id);
            return Ok(regla);
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ReglaRecargoDto>> Create([FromBody] ReglaRecargoCreateDto dto)
        {
            var regla = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = regla.Id }, regla);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ReglaRecargoDto>> Update(Guid id, [FromBody] ReglaRecargoUpdateDto dto)
        {
            var regla = await _service.UpdateAsync(id, dto);
            return Ok(regla);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _service.InactivateAsync(id);
            return NoContent();
        }
    }
}
