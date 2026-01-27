using Microsoft.AspNetCore.Mvc;
using Tlaoami.Application.Contracts;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/conciliacion")]
    public class ConciliacionController : ControllerBase
    {
        private readonly IConciliacionBancariaService _conciliacionService;
        private readonly ISugerenciasConciliacionService _sugerenciasService;
        private readonly IConsultaConciliacionesService _consultaService;
        private readonly IImportacionEstadoCuentaService _importacionService;

        public ConciliacionController(
            IConciliacionBancariaService conciliacionService,
            ISugerenciasConciliacionService sugerenciasService,
            IConsultaConciliacionesService consultaService,
            IImportacionEstadoCuentaService importacionService)
        {
            _conciliacionService = conciliacionService;
            _sugerenciasService = sugerenciasService;
            _consultaService = consultaService;
            _importacionService = importacionService;
        }

        [HttpGet("movimientos")]
        public async Task<ActionResult<IEnumerable<MovimientoBancarioDto>>> GetMovimientos(
            [FromQuery] EstadoConciliacion? estado,
            [FromQuery] TipoMovimiento? tipo,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 500) pageSize = 50;

            var movimientos = await _importacionService.GetMovimientosBancariosAsync(estado, tipo, desde, hasta, page, pageSize);
            return Ok(movimientos);
        }

        [HttpPost("importar-estado-cuenta")]
        public async Task<ActionResult<ImportacionResultadoDto>> ImportarEstadoCuenta([FromForm] IFormFile? archivoCsv)
        {
            try
            {
                if (archivoCsv == null || archivoCsv.Length == 0)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Archivo inválido",
                        Detail = "Debe proporcionar un archivo CSV no vacío en el campo 'archivoCsv'",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var resultado = await _importacionService.ImportarAsync(archivoCsv);
                return Ok(resultado);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new ProblemDetails { Title = "Error de importación", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
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
                    request.Comentario,
                    request.CrearPago,
                    request.Metodo ?? "Transferencia",
                    request.FechaPago,
                    request.AplicarACuenta);
                
                return Ok(new { message = "Movimiento conciliado correctamente" });
            }
            catch (ApplicationException ex)
            {
                return NotFound(new ProblemDetails { Title = "No encontrado", Detail = ex.Message, Status = StatusCodes.Status404NotFound });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ProblemDetails { Title = "Operación inválida", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
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
                return NotFound(new ProblemDetails { Title = "No encontrado", Detail = ex.Message, Status = StatusCodes.Status404NotFound });
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
                return NotFound(new ProblemDetails { Title = "No encontrado", Detail = ex.Message, Status = StatusCodes.Status404NotFound });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ProblemDetails { Title = "Operación inválida", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
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
                return NotFound(new ProblemDetails { Title = "No encontrado", Detail = ex.Message, Status = StatusCodes.Status404NotFound });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ProblemDetails { Title = "Operación inválida", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
        }

        [HttpGet("movimientos/{movimientoBancarioId}/detalle")]
        public async Task<ActionResult<MovimientoDetalleDto>> GetMovimientoDetalle(Guid movimientoBancarioId)
        {
            try
            {
                var detalle = await _consultaService.GetMovimientoDetalleAsync(movimientoBancarioId);
                return Ok(detalle);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new ProblemDetails { Title = "No encontrado", Detail = ex.Message, Status = StatusCodes.Status404NotFound });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ProblemDetails { Title = "Operación inválida", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
            }
        }

        [HttpPost("reportar-pago")]
        public async Task<IActionResult> ReportarPagoManual([FromBody] ReportarPagoManualDto dto)
        {
            var pagoId = await _conciliacionService.ReportarPagoManualAsync(dto);
            return Ok(new { pagoId });
        }

        [HttpGet("pagos-manuales")]
        public async Task<IActionResult> GetPagosManuales()
        {
            // TODO: Extraer escuelaId del contexto autenticado
            var escuelaId = Guid.Empty;
            var result = await _conciliacionService.GetPagosManualesAsync(escuelaId);
            return Ok(result);
        }

        [HttpPost("pagos-manuales/{pagoId}/conciliar")]
        public async Task<IActionResult> ConciliarPagoManual(Guid pagoId)
        {
            await _conciliacionService.ConciliarPagoManualAsync(pagoId);
            return Ok(new { mensaje = "Pago conciliado exitosamente" });
        }
    }

    public record ConciliarRequest(
        Guid MovimientoBancarioId,
        Guid? AlumnoId,
        Guid? FacturaId,
        string? Comentario,
        bool CrearPago = false,
        string? Metodo = "Transferencia",
        DateTime? FechaPago = null,
        bool AplicarACuenta = false);

    public record RevertirRequest(Guid MovimientoBancarioId);
}
