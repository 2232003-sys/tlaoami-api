using Microsoft.AspNetCore.Mvc;
using Tlaoami.Application.Dtos.PagosOnline;
using Tlaoami.Application.Interfaces.PagosOnline;

namespace Tlaoami.API.Controllers;

[ApiController]
[Route("api/v1/pagos-online")]
public class PagosOnlineController : ControllerBase
{
    private readonly IPagosOnlineService _pagosOnlineService;

    public PagosOnlineController(IPagosOnlineService pagosOnlineService)
    {
        _pagosOnlineService = pagosOnlineService;
    }

    [HttpPost("intents")]
    public async Task<ActionResult<PaymentIntentDto>> Crear([FromBody] CrearPaymentIntentDto dto)
    {
        try
        {
            var result = await _pagosOnlineService.CrearAsync(dto);
            return Ok(result);
        }
        catch (ApplicationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("intents/{id:guid}")]
    public async Task<ActionResult<PaymentIntentDto>> GetById([FromRoute] Guid id)
    {
        try
        {
            var result = await _pagosOnlineService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("facturas/{facturaId:guid}")]
    public async Task<ActionResult<IEnumerable<PaymentIntentDto>>> GetByFacturaId([FromRoute] Guid facturaId)
    {
        var result = await _pagosOnlineService.GetByFacturaIdAsync(facturaId);
        return Ok(result);
    }

    [HttpPost("{id:guid}/confirmar")]
    public async Task<ActionResult<PaymentIntentDto>> Confirmar([FromRoute] Guid id, [FromBody] ConfirmarPaymentIntentDto dto)
    {
        try
        {
            var result = await _pagosOnlineService.ConfirmarPagoAsync(id, dto.Usuario, dto.Comentario);
            return Ok(result);
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

    [HttpPost("{id:guid}/cancelar")]
    public async Task<ActionResult<PaymentIntentDto>> Cancelar([FromRoute] Guid id, [FromBody] CancelarPaymentIntentDto dto)
    {
        try
        {
            var result = await _pagosOnlineService.CancelarAsync(id, dto.Usuario, dto.Comentario);
            return Ok(result);
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

    [HttpPost("{id:guid}/webhook-simulado")]
    public async Task<ActionResult<PaymentIntentDto>> WebhookSimulado([FromRoute] Guid id, [FromBody] WebhookSimuladoDto dto)
    {
        try
        {
            var result = await _pagosOnlineService.ProcesarWebhookSimuladoAsync(id, dto.Estado, dto.Comentario);
            return Ok(result);
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
