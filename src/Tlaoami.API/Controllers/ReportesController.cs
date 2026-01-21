using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        /// <summary>
        /// Reporte de adeudos por alumno
        /// </summary>
        [HttpGet("adeudos")]
        public async Task<ActionResult<IEnumerable<AdeudoDto>>> GetAdeudos(
            [FromQuery] Guid? cicloId = null,
            [FromQuery] Guid? grupoId = null,
            [FromQuery] int? grado = null,
            [FromQuery] DateTime? fechaCorte = null)
        {
            try
            {
                var adeudos = await _reporteService.GetAdeudosAsync(cicloId, grupoId, grado, fechaCorte);
                return Ok(adeudos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reporte de pagos recibidos
        /// </summary>
        [HttpGet("pagos")]
        public async Task<ActionResult<IEnumerable<PagoReporteDto>>> GetPagos(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] Guid? grupoId = null,
            [FromQuery] string? metodo = null)
        {
            try
            {
                if (from == default || to == default)
                {
                    return BadRequest(new { error = "Parámetros 'from' y 'to' son requeridos" });
                }

                if (to < from)
                {
                    return BadRequest(new { error = "Fecha 'to' debe ser mayor o igual a 'from'" });
                }

                var pagos = await _reporteService.GetPagosAsync(from, to, grupoId, metodo);
                return Ok(pagos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Exportar adeudos a CSV
        /// </summary>
        [HttpGet("adeudos/export")]
        public async Task<IActionResult> ExportAdeudos(
            [FromQuery] Guid? cicloId = null,
            [FromQuery] Guid? grupoId = null,
            [FromQuery] int? grado = null,
            [FromQuery] DateTime? fechaCorte = null)
        {
            try
            {
                var csv = await _reporteService.ExportAdeudosToCsvAsync(cicloId, grupoId, grado, fechaCorte);
                var bytes = Encoding.UTF8.GetBytes(csv);
                var fileName = $"adeudos_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Exportar pagos a CSV
        /// </summary>
        [HttpGet("pagos/export")]
        public async Task<IActionResult> ExportPagos(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] Guid? grupoId = null,
            [FromQuery] string? metodo = null)
        {
            try
            {
                if (from == default || to == default)
                {
                    return BadRequest(new { error = "Parámetros 'from' y 'to' son requeridos" });
                }

                var csv = await _reporteService.ExportPagosToCsvAsync(from, to, grupoId, metodo);
                var bytes = Encoding.UTF8.GetBytes(csv);
                var fileName = $"pagos_{from:yyyyMMdd}_{to:yyyyMMdd}.csv";
                
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
