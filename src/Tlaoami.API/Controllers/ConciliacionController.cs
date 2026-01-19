using Microsoft.AspNetCore.Mvc;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/conciliacion")]
    public class ConciliacionController : ControllerBase
    {
        private readonly IConciliacionBancariaService _conciliacionService;
        private readonly ISugerenciasConciliacionService _sugerenciasService;
        private readonly IConsultaConciliacionesService _consultaService;

        public ConciliacionController(
            IConciliacionBancariaService conciliacionService,
            ISugerenciasConciliacionService sugerenciasService,
            IConsultaConciliacionesService consultaService)
        {
            _conciliacionService = conciliacionService;
            _sugerenciasService = sugerenciasService;
            _consultaService = consultaService;
        }

        [HttpPost("conciliar")]
        public async Task<ActionResult> Conciliar([FromBody] ConciliarRequest request)
        {
            try
            {
                await _conciliacionService.ConciliarMovimientoAsync(
                    request.MovimientoBancarioId,
                    request.AlumnoId,
                    request.FacturaId,
                    request.Comentario);
                
                return Ok(new { message = "Movimiento conciliado correctamente" });
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("revertir")]
        public async Task<ActionResult> Revertir([FromBody] RevertirRequest request)
        {
            try
            {
                await _conciliacionService.RevertirConciliacionAsync(request.MovimientoBancarioId);
                return Ok(new { message = "Conciliación revertida correctamente" });
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("{movimientoBancarioId}/sugerencias")]
        public async Task<ActionResult<List<SugerenciaConciliacionDto>>> GetSugerencias(Guid movimientoBancarioId)
        {
            try
            {
                var sugerencias = await _sugerenciasService.GetSugerenciasAsync(movimientoBancarioId);
                return Ok(sugerencias);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("conciliados")]
        public async Task<ActionResult<List<ConciliacionDetalleDto>>> GetConciliados([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            try
            {
                var conciliaciones = await _consultaService.GetConciliacionesAsync(desde, hasta);
                return Ok(conciliaciones);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public record ConciliarRequest(
        Guid MovimientoBancarioId,
        Guid? AlumnoId,
        Guid? FacturaId,
        string? Comentario);

    public record RevertirRequest(Guid MovimientoBancarioId);
}
