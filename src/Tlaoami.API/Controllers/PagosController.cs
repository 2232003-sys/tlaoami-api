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
                return CreatedAtAction(nameof(RegistrarPago), new { id = pagoDto.Id }, pagoDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
