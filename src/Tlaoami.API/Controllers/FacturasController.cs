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
    public class FacturasController : ControllerBase
    {
        private readonly IFacturaService _facturaService;

        public FacturasController(IFacturaService facturaService)
        {
            _facturaService = facturaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacturaDto>>> GetFacturas()
        {
            var facturas = await _facturaService.GetAllFacturasAsync();
            return Ok(facturas);
        }

        [HttpGet("detalle")]
        public async Task<ActionResult<IEnumerable<FacturaDetalleDto>>> GetFacturasDetalle()
        {
            var facturas = await _facturaService.GetAllFacturasDetalleAsync();
            return Ok(facturas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FacturaDto>> GetFactura(Guid id)
        {
            var factura = await _facturaService.GetFacturaByIdAsync(id);
            if (factura == null)
            {
                return NotFound();
            }
            return Ok(factura);
        }

        [HttpGet("{id}/detalle")]
        public async Task<ActionResult<FacturaDetalleDto>> GetFacturaDetalle(Guid id)
        {
            var factura = await _facturaService.GetFacturaDetalleByIdAsync(id);
            if (factura == null)
            {
                return NotFound();
            }
            return Ok(factura);
        }

        [HttpGet("alumno/{alumnoId}")]
        public async Task<ActionResult<IEnumerable<FacturaDetalleDto>>> GetFacturasByAlumno(Guid alumnoId)
        {
            var facturas = await _facturaService.GetFacturasByAlumnoIdAsync(alumnoId);
            return Ok(facturas);
        }

        [HttpPost]
        public async Task<ActionResult<FacturaDto>> CreateFactura([FromBody] FacturaDto facturaDto)
        {
            try
            {
                var factura = await _facturaService.CreateFacturaAsync(facturaDto);
                return CreatedAtAction(nameof(GetFactura), new { id = factura.Id }, factura);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateFactura(Guid id, [FromBody] FacturaDto facturaDto)
        {
            try
            {
                await _facturaService.UpdateFacturaAsync(id, facturaDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFactura(Guid id)
        {
            try
            {
                await _facturaService.DeleteFacturaAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
