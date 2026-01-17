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
    }
}
