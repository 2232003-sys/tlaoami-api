using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ReinscripcionesController : ControllerBase
    {
        private readonly IReinscripcionService _reinscripcionService;

        public ReinscripcionesController(IReinscripcionService reinscripcionService)
        {
            _reinscripcionService = reinscripcionService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ReinscripcionResultDto>> Reinscribir([FromBody] ReinscripcionRequestDto dto)
        {
            var result = await _reinscripcionService.ReinscribirAsync(dto);
            return Ok(result);
        }
    }
}
