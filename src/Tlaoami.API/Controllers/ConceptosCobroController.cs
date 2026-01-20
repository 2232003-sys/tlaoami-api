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
    public class ConceptosCobroController : ControllerBase
    {
        private readonly IConceptosCobroService _service;

        public ConceptosCobroController(IConceptosCobroService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todos los conceptos de cobro, opcionalmente filtrados por estado activo.
        /// </summary>
        /// <param name="activo">Filtro: true=activos, false=inactivos, null=todos</param>
        [HttpGet]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<List<ConceptoCobroDto>>> GetAll([FromQuery] bool? activo = null)
        {
            var conceptos = await _service.GetAllAsync(activo);
            return Ok(conceptos);
        }

        /// <summary>
        /// Obtiene un concepto de cobro por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<ConceptoCobroDto>> GetById(Guid id)
        {
            var concepto = await _service.GetByIdAsync(id);
            return Ok(concepto);
        }

        /// <summary>
        /// Crea un nuevo concepto de cobro.
        /// Requiere rol Admin o Administrativo.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ConceptoCobroDto>> Create([FromBody] ConceptoCobroCreateDto dto)
        {
            var concepto = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = concepto.Id }, concepto);
        }

        /// <summary>
        /// Actualiza un concepto de cobro.
        /// No se puede cambiar la Clave (es inmutable).
        /// Requiere rol Admin o Administrativo.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ConceptoCobroDto>> Update(Guid id, [FromBody] ConceptoCobroUpdateDto dto)
        {
            var concepto = await _service.UpdateAsync(id, dto);
            return Ok(concepto);
        }

        /// <summary>
        /// Inactiva un concepto de cobro (soft delete).
        /// Permite que sea reactivado posteriormente.
        /// Requiere rol Admin.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _service.InactivateAsync(id);
            return NoContent();
        }
    }
}
