using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Exceptions;

namespace Tlaoami.API.Controllers
{
    /// <summary>
    /// Gestión de asignaciones de conceptos de cobro (colegiaturas, actividades) a alumnos.
    /// Define "qué debe pagar" sin generar cargos automáticamente.
    /// </summary>
    [ApiController]
    [Route("api/v1")]
    public class AlumnoAsignacionesController : ControllerBase
    {
        private readonly IAlumnoAsignacionesService _service;

        public AlumnoAsignacionesController(IAlumnoAsignacionesService service)
        {
            _service = service;
        }

        /// <summary>
        /// POST /alumnos/{id}/asignaciones
        /// Crea una nueva asignación de concepto de cobro para un alumno.
        /// </summary>
        [HttpPost("alumnos/{id}/asignaciones")]
        public async Task<ActionResult<AlumnoAsignacionDto>> CreateAsignacion(
            Guid id,
            [FromBody] AlumnoAsignacionCreateDto dto)
        {
            try
            {
                var asignacion = await _service.CreateAsignacionAsync(id, dto);
                return CreatedAtAction(nameof(GetAsignaciones), new { id }, asignacion);
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
        /// GET /alumnos/{id}/asignaciones
        /// Lista todas las asignaciones de un alumno (activas + históricas).
        /// </summary>
        [HttpGet("alumnos/{id}/asignaciones")]
        public async Task<ActionResult<IReadOnlyList<AlumnoAsignacionDto>>> GetAsignaciones(Guid id)
        {
            var asignaciones = await _service.ListarAsignacionesPorAlumnoAsync(id);
            return Ok(asignaciones);
        }

        /// <summary>
        /// DELETE /asignaciones/{asignacionId}
        /// Cancela (marca como inactiva) una asignación.
        /// </summary>
        [HttpDelete("asignaciones/{asignacionId}")]
        public async Task<ActionResult> CancelarAsignacion(Guid asignacionId)
        {
            var result = await _service.CancelarAsignacionAsync(asignacionId);
            if (!result)
                return NotFound(new { message = "Asignación no encontrada" });

            return NoContent();
        }
    }
}
