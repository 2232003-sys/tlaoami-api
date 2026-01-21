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

        [HttpPost("{id}/emitir-recibo")]
        public async Task<ActionResult> EmitirRecibo(Guid id)
        {
            try
            {
                await _facturaService.EmitirReciboAsync(id);
                return Ok(new { message = "Recibo emitido (idempotente)" });
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

        [HttpPost("{id}/emitir")]
        public async Task<ActionResult> EmitirFactura(Guid id)
        {
            try
            {
                await _facturaService.EmitirFacturaAsync(id);
                return Ok(new { message = "Factura emitida (idempotente)" });
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

        [HttpPost("{id}/cancelar")]
        public async Task<ActionResult> CancelarFactura(Guid id, [FromBody] CancelarFacturaRequest? request)
        {
            try
            {
                await _facturaService.CancelarFacturaAsync(id, request?.Motivo);
                return Ok(new { message = "Factura cancelada (idempotente)" });
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacturaDetalleDto>>> GetFacturas(
            [FromQuery] Guid? alumnoId = null,
            [FromQuery] string? estado = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var facturas = await _facturaService.GetFacturasConFiltrosAsync(alumnoId, estado, desde, hasta);
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
        public async Task<ActionResult<FacturaDto>> CreateFactura([FromBody] CrearFacturaDto crearFacturaDto)
        {
            try
            {
                var factura = await _facturaService.CreateFacturaAsync(crearFacturaDto);
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

public class CancelarFacturaRequest
{
    public string? Motivo { get; set; }
}
