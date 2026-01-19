using Microsoft.AspNetCore.Mvc;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/pagos")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagosController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpPost]
        public async Task<ActionResult<PagoDto>> RegistrarPago([FromBody] PagoCreateDto pagoCreateDto)
        {
            try
            {
                var pagoDto = await _pagoService.RegistrarPagoAsync(pagoCreateDto);
                return CreatedAtAction(nameof(GetPago), new { id = pagoDto.Id }, pagoDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PagoDto>> GetPago(Guid id)
        {
            var pago = await _pagoService.GetPagoByIdAsync(id);
            if (pago == null)
            {
                return NotFound();
            }
            return Ok(pago);
        }

        [HttpGet("factura/{facturaId}")]
        public async Task<ActionResult<IEnumerable<PagoDto>>> GetPagosByFactura(Guid facturaId)
        {
            var pagos = await _pagoService.GetPagosByFacturaIdAsync(facturaId);
            return Ok(pagos);
        }
    }
}
