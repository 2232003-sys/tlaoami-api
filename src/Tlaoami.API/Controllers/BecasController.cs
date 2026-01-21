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
    public class BecasController : ControllerBase
    {
        private readonly IBecaAlumnoService _service;

        public BecasController(IBecaAlumnoService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<List<BecaAlumnoDto>>> GetAll(
            [FromQuery] Guid? cicloId = null,
            [FromQuery] Guid? alumnoId = null,
            [FromQuery] bool? activa = null)
        {
            var becas = await _service.GetAllAsync(cicloId, alumnoId, activa);
            return Ok(becas);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<BecaAlumnoDto>> GetById(Guid id)
        {
            var beca = await _service.GetByIdAsync(id);
            return Ok(beca);
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<BecaAlumnoDto>> Create([FromBody] BecaAlumnoCreateDto dto)
        {
            var beca = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = beca.Id }, beca);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<BecaAlumnoDto>> Update(Guid id, [FromBody] BecaAlumnoUpdateDto dto)
        {
            var beca = await _service.UpdateAsync(id, dto);
            return Ok(beca);
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
