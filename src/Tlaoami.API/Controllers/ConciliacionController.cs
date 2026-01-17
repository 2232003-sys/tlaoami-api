using Microsoft.AspNetCore.Mvc;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Enumerations;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/conciliacion")]
    public class ConciliacionController : ControllerBase
    {
        private readonly IImportacionEstadoCuentaService _importacionService;

        public ConciliacionController(IImportacionEstadoCuentaService importacionService)
        {
            _importacionService = importacionService;
        }

        [HttpPost("importar-estado-cuenta")]
        public async Task<IActionResult> ImportarEstadoDeCuenta(IFormFile archivoCsv)
        {
            if (archivoCsv == null || archivoCsv.Length == 0)
            {
                return BadRequest("El archivo no puede estar vacío.");
            }

            var resultado = await _importacionService.ImportarAsync(archivoCsv);
            return Ok(resultado);
        }

        [HttpGet("movimientos")]
        public async Task<IActionResult> GetMovimientosBancarios(
            [FromQuery] EstadoConciliacion? estado,
            [FromQuery] TipoMovimiento? tipo,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            var movimientos = await _importacionService.GetMovimientosBancariosAsync(estado, tipo, desde, hasta);
            return Ok(movimientos);
        }
    }
}
