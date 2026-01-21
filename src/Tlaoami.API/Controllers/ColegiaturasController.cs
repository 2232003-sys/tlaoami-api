using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ColegiaturasController : ControllerBase
    {
        private readonly IColegiaturasService _service;

        public ColegiaturasController(IColegiaturasService service)
        {
            _service = service;
        }

        [HttpPost("generar")]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<ColegiaturaGeneracionResultDto>> Generar([FromBody] ColegiaturaGeneracionRequestDto dto)
        {
            var result = await _service.GenerarMensualAsync(dto);
            return Ok(result);
        }

        [HttpPost("aplicar-recargos")]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<RecargoAplicacionResultDto>> AplicarRecargos([FromBody] RecargoAplicacionRequestDto dto)
        {
            var result = await _service.AplicarRecargosAsync(dto);
            return Ok(result);
        }
    }
}
