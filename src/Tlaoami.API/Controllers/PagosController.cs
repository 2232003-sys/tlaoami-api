using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagosController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminAndAdministrativo)]
        public async Task<ActionResult<PagoDto>> RegistrarPago([FromBody] PagoCreateDto pagoCreateDto)
        {
            try
            {
                var (pagoDto, created) = await _pagoService.RegistrarPagoAsync(pagoCreateDto);
                if (created)
                    return CreatedAtAction(nameof(GetPago), new { id = pagoDto.Id }, pagoDto);
                return Ok(pagoDto);
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
