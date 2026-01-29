using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Settings;
using Tlaoami.Domain.Enums;
using Tlaoami.API.Authorization;

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
        [Authorize]
        public async Task<ActionResult<EscuelaSettingsDto>> Get()
        {
            var dto = await _service.GetSettingsAsync();
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPut]
        [AuthorizeByRole(UserRole.Owner)]
        public async Task<ActionResult<EscuelaSettingsDto>> Put([FromBody] EscuelaSettingsDto dto)
        {
            var updated = await _service.UpdateSettingsAsync(dto);
            return Ok(updated);
        }
    }
}
