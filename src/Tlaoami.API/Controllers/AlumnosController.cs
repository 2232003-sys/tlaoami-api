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
    public class AlumnosController : ControllerBase
    {
        private readonly IAlumnoService _alumnoService;

        public AlumnosController(IAlumnoService alumnoService)
        {
            _alumnoService = alumnoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlumnoDto>>> GetAlumnos()
        {
            var alumnos = await _alumnoService.GetAllAlumnosAsync();
            return Ok(alumnos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AlumnoDto>> GetAlumno(Guid id)
        {
            var alumno = await _alumnoService.GetAlumnoByIdAsync(id);
            if (alumno == null)
            {
                return NotFound();
            }
            return Ok(alumno);
        }

        [HttpGet("matricula/{matricula}")]
        public async Task<ActionResult<AlumnoDto>> GetAlumnoByMatricula(string matricula)
        {
            var alumno = await _alumnoService.GetAlumnoByMatriculaAsync(matricula);
            if (alumno == null)
                return NotFound();
            return Ok(alumno);
        }

        [HttpGet("{id}/grupo-actual")]
        public async Task<ActionResult<AlumnoDto>> GetAlumnoConGrupoActual(Guid id)
        {
            var alumno = await _alumnoService.GetAlumnoConGrupoActualAsync(id);
            if (alumno == null)
                return NotFound();
            return Ok(alumno);
        }

        [HttpGet("{id}/estado-cuenta")]
        public async Task<ActionResult<EstadoCuentaDto>> GetEstadoCuenta(Guid id)
        {
            var estadoCuenta = await _alumnoService.GetEstadoCuentaAsync(id);
            if (estadoCuenta == null)
            {
                return NotFound();
            }
            return Ok(estadoCuenta);
        }

        [HttpPost]
        public async Task<ActionResult<AlumnoDto>> CreateAlumno([FromBody] AlumnoCreateDto dto)
        {
            try
            {
                var alumno = await _alumnoService.CreateAlumnoAsync(dto);
                return CreatedAtAction(nameof(GetAlumno), new { id = alumno.Id }, alumno);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AlumnoDto>> UpdateAlumno(Guid id, [FromBody] AlumnoUpdateDto dto)
        {
            try
            {
                var alumno = await _alumnoService.UpdateAlumnoAsync(id, dto);
                return Ok(alumno);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAlumno(Guid id)
        {
            var result = await _alumnoService.DeleteAlumnoAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}
