using Microsoft.AspNetCore.Mvc;
using Tlaoami.Application.Dtos.PagosOnline;
using Tlaoami.Application.Interfaces.PagosOnline;

namespace Tlaoami.API.Controllers;

[ApiController]
[Route("api/v1/pagos-online/intents")]
public class PagosOnlineController : ControllerBase
{
    private readonly IPagosOnlineService _pagosOnlineService;

    public PagosOnlineController(IPagosOnlineService pagosOnlineService)
    {
        _pagosOnlineService = pagosOnlineService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentIntentDto>> Crear([FromBody] CrearPaymentIntentDto dto)
    {
        try
        {
            var result = await _pagosOnlineService.CrearAsync(dto);
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

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentIntentDto>> GetById(Guid id)
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
}
