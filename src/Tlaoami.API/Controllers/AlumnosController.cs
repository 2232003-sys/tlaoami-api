using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Services;
using Tlaoami.Domain;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AlumnosController : ControllerBase
    {
        private readonly IAlumnoService _alumnoService;
        private readonly IReceptorFiscalService _receptorFiscalService;

        public AlumnosController(IAlumnoService alumnoService, IReceptorFiscalService receptorFiscalService)
        {
            _alumnoService = alumnoService;
            _receptorFiscalService = receptorFiscalService;
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

        [AllowAnonymous]
        [HttpGet("{id}/estado-cuenta")]
        public async Task<ActionResult<EstadoCuentaDto>> GetEstadoCuenta(Guid id)
        {
            var estadoCuenta = await _alumnoService.GetEstadoCuentaAsync(id);
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

        [HttpGet("{id}/receptor-fiscal")]
        public async Task<ActionResult<ReceptorFiscalDto>> GetReceptorFiscal(Guid id)
        {
            var receptor = await _receptorFiscalService.GetByAlumnoIdAsync(id);
            if (receptor == null)
                return NotFound(new { error = "Receptor fiscal no encontrado", code = "RECEPTOR_FISCAL_NO_ENCONTRADO" });
            return Ok(receptor);
        }

        [HttpPut("{id}/receptor-fiscal")]
        public async Task<ActionResult<ReceptorFiscalDto>> UpsertReceptorFiscal(Guid id, [FromBody] ReceptorFiscalUpsertDto dto)
        {
            try
            {
                var receptor = await _receptorFiscalService.UpsertAsync(id, dto);
                return Ok(receptor);
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
