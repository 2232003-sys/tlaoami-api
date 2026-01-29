using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Tlaoami.Application.Finanzas;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Exceptions;
using Tlaoami.Domain.Enums;
using Tlaoami.API.Authorization;

namespace Tlaoami.API.Controllers
{
    /// <summary>
    /// Controlador para operaciones financieras centralizadas del ERP.
    /// Solo Owner (Admin) puede acceder.
    /// </summary>
    [ApiController]
    [Route("api/v1/finanzas")]
    [AuthorizeByRole(UserRole.Owner)]
    public class FinanzasController : ControllerBase
    {
        private readonly IGenerarCargosRecurrentesService _cargosService;

        public FinanzasController(IGenerarCargosRecurrentesService cargosService)
        {
            _cargosService = cargosService;
        }

        /// <summary>
        /// POST /finanzas/generar-cargos/{periodo}?cicloId={guid}&emitir=true
        /// Genera cargos recurrentes mensuales basados en asignaciones activas.
        /// Trigger manual inicial; luego se puede automatizar con cron/background job.
        /// </summary>
        /// <param name="periodo">Periodo en formato YYYY-MM (ej: "2026-01")</param>
        /// <param name="cicloId">ID del ciclo escolar</param>
        /// <param name="emitir">Si true, emite facturas; si false, deja en borrador (default: true)</param>
        [HttpPost("generar-cargos/{periodo}")]
        public async Task<ActionResult<GenerarCargosRecurrentesResultDto>> GenerarCargosRecurrentes(
            string periodo,
            [FromQuery] Guid cicloId,
            [FromQuery] bool emitir = true)
        {
            try
            {
                var result = await _cargosService.GenerarCargosAsync(periodo, cicloId, emitir);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, code = ex.Code });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
            catch (BusinessException ex)
            {
                return Conflict(new { error = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
