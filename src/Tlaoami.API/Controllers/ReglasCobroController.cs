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
    public class ReglasCobroController : ControllerBase
    {
        private readonly IReglasCobroService _service;

        public ReglasCobroController(IReglasCobroService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todas las reglas de cobro, opcionalmente filtradas.
        /// </summary>
        /// <param name="cicloId">Filtro: ciclo específico (optional)</param>
        /// <param name="grado">Filtro: grado (1..6, optional)</param>
        /// <param name="activa">Filtro: true=activas, false=inactivas, null=todas</param>
        [HttpGet]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<List<ReglaCobroDto>>> GetAll(
            [FromQuery] Guid? cicloId = null,
            [FromQuery] int? grado = null,
            [FromQuery] bool? activa = null)
        {
            var reglas = await _service.GetAllAsync(cicloId, grado, activa);
            return Ok(reglas);
        }

        /// <summary>
        /// Obtiene una regla de cobro por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<ReglaCobroDto>> GetById(Guid id)
        {
            var regla = await _service.GetByIdAsync(id);
            return Ok(regla);
        }

        /// <summary>
        /// Obtiene todas las reglas para un ciclo específico.
        /// </summary>
        [HttpGet("ciclo/{cicloId}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<List<ReglaCobroDto>>> GetByCiclo(
            Guid cicloId,
            [FromQuery] bool? activa = null)
        {
            var reglas = await _service.GetByCicloAsync(cicloId, activa);
            return Ok(reglas);
        }

        /// <summary>
        /// Crea una nueva regla de cobro.
        /// Requiere rol Admin o Administrativo.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ReglaCobroDto>> Create([FromBody] ReglaCobroCreateDto dto)
        {
            var regla = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = regla.Id }, regla);
        }

        /// <summary>
        /// Actualiza una regla de cobro.
        /// CicloId y ConceptoCobroId son inmutables.
        /// Requiere rol Admin o Administrativo.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ReglaCobroDto>> Update(Guid id, [FromBody] ReglaCobroUpdateDto dto)
        {
            var regla = await _service.UpdateAsync(id, dto);
            return Ok(regla);
        }

        /// <summary>
        /// Inactiva una regla de cobro (soft delete).
        /// Permite que sea reactivada posteriormente.
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
