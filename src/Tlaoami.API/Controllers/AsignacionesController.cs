using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AsignacionesController : ControllerBase
    {
        private readonly IAsignacionGrupoService _asignacionService;

        public AsignacionesController(IAsignacionGrupoService asignacionService)
        {
            _asignacionService = asignacionService;
        }

        [HttpPost("alumno-grupo")]
        public async Task<ActionResult<AlumnoGrupoDto>> AsignarAlumnoAGrupo([FromBody] AsignarAlumnoGrupoDto dto)
        {
            try
            {
                var asignacion = await _asignacionService.AsignarAlumnoAGrupoAsync(dto);
                return CreatedAtAction(nameof(AsignarAlumnoAGrupo), asignacion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("alumno/{alumnoId}/grupo-actual")]
        public async Task<ActionResult<GrupoDto>> GetGrupoActualDeAlumno(Guid alumnoId)
        {
            var grupo = await _asignacionService.GetGrupoActualDeAlumnoAsync(alumnoId);
            if (grupo == null)
                return NotFound(new { message = "El alumno no tiene grupo asignado" });
            return Ok(grupo);
        }

        [HttpPost("alumno/{alumnoId}/desasignar")]
        public async Task<ActionResult> DesasignarAlumnoDeGrupo(Guid alumnoId)
        {
            var result = await _asignacionService.DesasignarAlumnoDeGrupoAsync(alumnoId);
            if (!result)
                return NotFound(new { message = "El alumno no tiene grupo asignado" });
            return NoContent();
        }
    }
}
