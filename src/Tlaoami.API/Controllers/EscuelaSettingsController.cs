using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Settings;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/escuela/settings")]
    public class EscuelaSettingsController : ControllerBase
    {
        private readonly IEscuelaSettingsService _service;

        public EscuelaSettingsController(IEscuelaSettingsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<EscuelaSettingsDto>> Get()
        {
            var dto = await _service.GetSettingsAsync();
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPut]
        public async Task<ActionResult<EscuelaSettingsDto>> Put([FromBody] EscuelaSettingsDto dto)
        {
            var updated = await _service.UpdateSettingsAsync(dto);
            return Ok(updated);
        }
    }
}
