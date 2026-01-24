using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ReinscripcionesController : ControllerBase
    {
        private readonly IReinscripcionService _service;

        public ReinscripcionesController(IReinscripcionService service)
        {
            _service = service;
        }

        [HttpGet("preview")]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<IActionResult> Preview([FromQuery] Guid cicloOrigenId, [FromQuery] Guid cicloDestinoId)
        {
            var resultado = await _service.PreviewAsync(cicloOrigenId, cicloDestinoId);
            return Ok(resultado);
        }

        [HttpPost("ejecutar")]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<IActionResult> Ejecutar([FromBody] ReinscripcionEjecutarDto dto)
        {
            try
            {
                await _service.EjecutarAsync(dto);
                return Ok();
            }
            catch (BusinessException ex)
            {
                return Conflict(new { error = ex.Message, code = ex.Code });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
        }

        /// <summary>
        /// Crea una solicitud de reinscripción de alumno a nuevo ciclo/grupo.
        /// Valida adeudo: si saldo > 0.01 => 409 REINSCRIPCION_BLOQUEADA_ADEUDO.
        /// Si pasa validaciones => desasigna grupo anterior y asigna nuevo.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ReinscripcionDto>> CrearReinscripcion([FromBody] ReinscripcionCreateDto dto)
        {
            try
            {
                var usuarioId = ObtenerUsuarioIdDelJwt();
                var resultado = await _service.CrearReinscripcionAsync(dto, usuarioId);
                return CreatedAtAction(nameof(GetReinscripcion), new { id = resultado.Id }, resultado);
            }
            catch (BusinessException ex) when (ex.Code == "REINSCRIPCION_BLOQUEADA_ADEUDO")
            {
                return Conflict(new { message = ex.Message, code = ex.Code });
            }
            catch (BusinessException ex)
            {
                return Conflict(new { error = ex.Message, code = ex.Code });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
        }

        /// <summary>
        /// Obtiene detalles de una reinscripción específica.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<ReinscripcionDto>> GetReinscripcion(Guid id)
        {
            var resultado = await _service.GetReinscripcionAsync(id);
            if (resultado == null)
                return NotFound(new { code = "REINSCRIPCION_NO_ENCONTRADA", message = "Reinscripción no encontrada." });

            return Ok(resultado);
        }

        /// <summary>
        /// Lista reinscripciones de un alumno.
        /// Query opcional: ?cicloDestinoId=<uuid>
        /// </summary>
        [HttpGet("alumno/{alumnoId}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<IEnumerable<ReinscripcionDto>>> GetReinscripcionesPorAlumno(
            Guid alumnoId,
            [FromQuery] Guid? cicloDestinoId = null)
        {
            var resultados = await _service.GetReinscripcionesPorAlumnoAsync(alumnoId, cicloDestinoId);
            return Ok(resultados);
        }

        // === Métodos helper privados ===

        private Guid ObtenerUsuarioIdDelJwt()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("oid");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var usuarioId))
                throw new UnauthorizedAccessException("No se pudo extraer el ID del usuario del JWT.");

            return usuarioId;
        }
    }
}
