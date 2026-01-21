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
    public class ReglasColegiaturaController : ControllerBase
    {
        private readonly IReglaColegiaturaService _service;

        public ReglasColegiaturaController(IReglaColegiaturaService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<List<ReglaColegiaturaDto>>> GetAll(
            [FromQuery] Guid? cicloId = null,
            [FromQuery] Guid? grupoId = null,
            [FromQuery] int? grado = null,
            [FromQuery] bool? activa = null)
        {
            var reglas = await _service.GetAllAsync(cicloId, grupoId, grado, activa);
            return Ok(reglas);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<ReglaColegiaturaDto>> GetById(Guid id)
        {
            var regla = await _service.GetByIdAsync(id);
            return Ok(regla);
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ReglaColegiaturaDto>> Create([FromBody] ReglaColegiaturaCreateDto dto)
        {
            var regla = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = regla.Id }, regla);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ReglaColegiaturaDto>> Update(Guid id, [FromBody] ReglaColegiaturaUpdateDto dto)
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
