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
    }
}
