using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PagoDto>>> GetPagos()
        {
            var pagos = await _pagoService.GetAllPagosAsync();
            return Ok(pagos);
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

        [HttpPost]
        public async Task<ActionResult<PagoDto>> CreatePago([FromBody] PagoCreateDto pagoCreateDto)
        {
            var pagoDto = new PagoDto
            {
                FacturaId = pagoCreateDto.FacturaId,
                Monto = pagoCreateDto.Monto,
                FechaPago = pagoCreateDto.FechaPago,
                Metodo = pagoCreateDto.Metodo
            };

            var nuevoPago = await _pagoService.CreatePagoAsync(pagoDto);
            return CreatedAtAction(nameof(GetPago), new { id = nuevoPago.Id }, nuevoPago);
        }
    }
}
